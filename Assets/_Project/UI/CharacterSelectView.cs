using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace LogiCard.UI
{
    /// <summary>
    /// PILOT — Character Select carousel rebuilt on Unity UI Toolkit (<see cref="UIDocument"/> /
    /// <see cref="VisualElement"/>) instead of uGUI, to evaluate a full-game UI Toolkit migration.
    /// See <c>docs/UI_TOOLKIT_MIGRATION_PROPOSAL.md</c> for the impact/risk/sequencing writeup this
    /// pilot fed. Same behavior contract as the uGUI version it replaces: 2-item center/flank
    /// crossfade (~650ms) via the same <see cref="UiMotion"/> helper, <c>Pick_Scout</c> /
    /// <c>Pick_Juggernaut</c> hit targets, ghost archetype headline, per-archetype background tint,
    /// halo glow. <c>ConfirmCharacter</c> stays a uGUI button built by <see cref="AppFlowController"/>
    /// — this screen is a hybrid (Toolkit carousel + uGUI Confirm sibling), the coexistence shape
    /// <c>docs/UI_TOOLS_RESEARCH.md</c> flagged as the realistic rollout path, not a green-field one.
    /// </summary>
    public sealed class CharacterSelectView : MonoBehaviour
    {
        private const float CrossfadeSeconds = UiMotion.DefaultDuration;
        private const float CardAnchorY = 0.04f;

        private UIDocument _document;
        private Font _font;
        private VisualElement _bg;
        private Label _ghost;
        private Label _detail;
        private Card[] _cards;
        private GlowRing[] _glowRings;
        private int _activeIndex;
        private bool _animating;
        private Coroutine _motion;

        public string SelectedId => _cards != null && _cards.Length > 0
            ? _cards[_activeIndex].Id
            : "Scout";

        public event Action<string> SelectionChanged;

        private void OnEnable()
        {
            if (_bg != null)
            {
                _bg.style.display = DisplayStyle.Flex;
            }
        }

        private void OnDisable()
        {
            if (_bg != null)
            {
                _bg.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// The Toolkit host lives at scene root (see <see cref="BuildChrome"/>) specifically so
        /// nothing else's SetActive can reach it — which also means nothing else will ever destroy
        /// it. Tear it down manually alongside this screen so repeated Boot→Bootstrap cycles (every
        /// PlayMode test's SetUp/TearDown, or a real rematch-to-title loop) don't leave orphaned
        /// UIDocuments behind that stale VisualElement queries could match instead of the live one.
        /// </summary>
        private void OnDestroy()
        {
            if (_document != null)
            {
                // Immediate, not deferred Destroy: this runs inside an active destroy cascade
                // (screenRoot going down with it), including PlayMode tests' DestroyImmediate(Bootstrap)
                // teardown — a deferred Destroy could still be pending, and thus still findable, when
                // the next test's SetUp queries for UIDocuments a moment later.
                DestroyImmediate(_document.gameObject);
            }
        }

        /// <summary>
        /// Builds the carousel under an existing Character Select screen root (full-bleed panel).
        /// Confirm is owned by the host so navigation stays in <see cref="AppFlowController"/>.
        /// </summary>
        public static CharacterSelectView Build(UiFactory ui, RectTransform screenRoot)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (screenRoot == null)
            {
                throw new ArgumentNullException(nameof(screenRoot));
            }

            CharacterSelectView view = screenRoot.gameObject.GetComponent<CharacterSelectView>();
            if (view == null)
            {
                view = screenRoot.gameObject.AddComponent<CharacterSelectView>();
            }

            view._font = ui.Font;
            view.BuildChrome(screenRoot);
            view.ApplyRolesInstant();
            view.NotifySelection();
            return view;
        }

        /// <summary>
        /// Unity tears down and rebuilds an empty <see cref="UIDocument.rootVisualElement"/> on
        /// every OnEnable/OnDisable — by design, not a bug (confirmed against Unity's own guidance:
        /// treat re-enable as "rebuild", or avoid disabling the GameObject at all). AppFlowController
        /// hides screens by disabling their GameObject wholesale, and also disables the whole AppFlow
        /// shell (<c>_root</c>, this screen's own parent) whenever it leaves the pre-match shell
        /// entirely (<c>Show(Screen.None)</c> — bypassed-to-match, tests' <c>BypassAppFlowForTests</c>).
        /// Either would cascade-disable a UIDocument parented anywhere under this screen or its
        /// ancestors and silently wipe the tree. So the UIDocument lives on its own GameObject at
        /// scene root — parented under nothing AppFlowController ever touches — and
        /// <see cref="OnEnable"/>/<see cref="OnDisable"/> below (fired by screenRoot's own
        /// activation, since <see cref="CharacterSelectView"/> itself still lives there) translate
        /// that into <c>rootVisualElement.style.display</c> instead — hide/show, never rebuild.
        /// </summary>
        private void BuildChrome(RectTransform root)
        {
            var hostGo = new GameObject("CharacterSelectToolkit");
            _document = hostGo.AddComponent<UIDocument>();
            _document.panelSettings = CreatePanelSettings();

            VisualElement rootVe = _document.rootVisualElement;
            rootVe.style.position = Position.Absolute;
            rootVe.style.left = 0;
            rootVe.style.right = 0;
            rootVe.style.top = 0;
            rootVe.style.bottom = 0;
            // Starts hidden — screenRoot itself starts inactive (AppFlowController.CreateScreen),
            // so this shouldn't paint until the first real Show(Screen.CharacterSelect) triggers
            // OnEnable below.
            rootVe.style.display = DisplayStyle.None;
            _bg = rootVe;

            // Ghost headline first = furthest back, matching the uGUI version's explicit
            // SetAsFirstSibling — everything else paints on top of it.
            _ghost = CreateLabel(rootVe, "GhostHeadline", "SCOUT", 120, TextAnchor.MiddleCenter, UiStyle.CharSelectGhost, bold: true);
            StretchVe(_ghost, 0.02f, 0.28f, 0.98f, 0.78f);

            var stage = new VisualElement { name = "CarouselStage" };
            StretchVe(stage, 0f, 0.18f, 1f, 0.88f);
            rootVe.Add(stage);

            // Glow rings sit behind both cards (added first = lowest in the paint order within
            // stage) and always track whichever card is currently center.
            _glowRings = new[]
            {
                CreateGlowRing(stage, "GlowOuter", padding: 60f, maxAlpha: 0.16f),
                CreateGlowRing(stage, "GlowInner", padding: 26f, maxAlpha: 0.30f),
            };

            _cards = new[]
            {
                CreateCard(stage, "Scout", "SCOUT", UiStyle.CharSelectCardScout),
                CreateCard(stage, "Juggernaut", "JUGGERNAUT", UiStyle.CharSelectCardJuggernaut),
            };

            Label brand = CreateLabel(rootVe, "Brand", "LOGICARD", 18, TextAnchor.UpperLeft, UiStyle.Ink, bold: true);
            StretchVe(brand, 0.04f, 0.92f, 0.4f, 0.98f);

            Label title = CreateLabel(rootVe, "Title", "CHARACTER SELECT", 28, TextAnchor.MiddleCenter, UiStyle.Ink, bold: true);
            StretchVe(title, 0.2f, 0.88f, 0.8f, 0.96f);

            CreateNavButton(rootVe, "CharSelectPrev", "<", () => Navigate(-1), 0.06f, 0.28f, 0.14f, 0.40f);
            CreateNavButton(rootVe, "CharSelectNext", ">", () => Navigate(1), 0.86f, 0.28f, 0.94f, 0.40f);

            _detail = CreateLabel(rootVe, "Detail", string.Empty, 22, TextAnchor.MiddleCenter, UiStyle.Ink, bold: false);
            StretchVe(_detail, 0.12f, 0.14f, 0.88f, 0.24f);

            _activeIndex = 0;
        }

        private static PanelSettings CreatePanelSettings()
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(
                Mathf.RoundToInt(UiStyle.ReferenceResolution.x),
                Mathf.RoundToInt(UiStyle.ReferenceResolution.y));
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = UiStyle.CanvasMatchWidthOrHeight;
            // Above the uGUI Canvas's default sortingOrder (0) so this screen's Toolkit content
            // composites in front of the leftover uGUI screen background (CreateScreen's Image) and
            // the sibling ConfirmCharacter uGUI button stays readable as an overlay on top of it too
            // — cross-technology sort order is real friction, flagged in the migration proposal.
            settings.sortingOrder = 10f;
            return settings;
        }

        private Label CreateLabel(VisualElement parent, string name, string text, int fontSize, TextAnchor align, Color color, bool bold)
        {
            var label = new Label(text) { name = name };
            label.style.color = color;
            label.style.fontSize = fontSize;
            label.style.unityTextAlign = align;
            if (_font != null)
            {
                label.style.unityFontDefinition = FontDefinition.FromFont(_font);
            }

            if (bold)
            {
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            label.pickingMode = PickingMode.Ignore;
            parent.Add(label);
            return label;
        }

        private void CreateNavButton(VisualElement parent, string name, string label, Action onClick,
            float xMin, float yMin, float xMax, float yMax)
        {
            var btn = new VisualElement { name = name };
            StretchVe(btn, xMin, yMin, xMax, yMax);
            btn.style.backgroundColor = new Color(1f, 1f, 1f, 0.14f);
            btn.style.borderTopWidth = 1.5f;
            btn.style.borderBottomWidth = 1.5f;
            btn.style.borderLeftWidth = 1.5f;
            btn.style.borderRightWidth = 1.5f;
            Color border = new Color(1f, 1f, 1f, 0.85f);
            btn.style.borderTopColor = border;
            btn.style.borderBottomColor = border;
            btn.style.borderLeftColor = border;
            btn.style.borderRightColor = border;
            SetCornerRadius(btn, 8f);
            btn.pickingMode = PickingMode.Position;
            btn.RegisterCallback<ClickEvent>(_ => onClick());
            parent.Add(btn);

            Label text = CreateLabel(btn, "Label", label, 36, TextAnchor.MiddleCenter, UiStyle.Ink, bold: true);
            StretchVe(text, 0f, 0f, 1f, 1f);
        }

        private Card CreateCard(VisualElement stage, string id, string label, Color face)
        {
            var box = new VisualElement { name = $"Pick_{id}" };
            box.style.position = Position.Absolute;
            box.style.backgroundColor = face;
            SetCornerRadius(box, 18f);
            box.pickingMode = PickingMode.Position;
            string capturedId = id;
            box.RegisterCallback<ClickEvent>(_ => OnCardClicked(capturedId));
            stage.Add(box);

            Label text = CreateLabel(box, "Label", label, 34, TextAnchor.MiddleCenter, UiStyle.InkDark, bold: true);
            StretchVe(text, 0.08f, 0.08f, 0.92f, 0.92f);

            return new Card
            {
                Id = id,
                Label = label,
                Face = face,
                Element = box,
            };
        }

        private GlowRing CreateGlowRing(VisualElement stage, string name, float padding, float maxAlpha)
        {
            var el = new VisualElement { name = name };
            el.style.position = Position.Absolute;
            el.pickingMode = PickingMode.Ignore;
            el.style.backgroundColor = new Color(1f, 1f, 1f, 0f);
            SetCornerRadius(el, 28f);
            stage.Add(el);
            return new GlowRing(el, padding, maxAlpha);
        }

        private static void SetCornerRadius(VisualElement ve, float radius)
        {
            ve.style.borderTopLeftRadius = radius;
            ve.style.borderTopRightRadius = radius;
            ve.style.borderBottomLeftRadius = radius;
            ve.style.borderBottomRightRadius = radius;
        }

        /// <summary>
        /// Maps a uGUI-style stretch anchor box (min/max fractions of the parent, Y-up) onto USS
        /// left/right/top/bottom percentages (Y-down) — the direct analog of
        /// <c>UiFactory.Stretch</c>, kept so every layout number ported 1:1 from the uGUI version.
        /// </summary>
        private static void StretchVe(VisualElement ve, float xMin, float yMin, float xMax, float yMax)
        {
            ve.style.position = Position.Absolute;
            ve.style.left = Length.Percent(xMin * 100f);
            ve.style.right = Length.Percent((1f - xMax) * 100f);
            ve.style.bottom = Length.Percent(yMin * 100f);
            ve.style.top = Length.Percent((1f - yMax) * 100f);
        }

        private void OnCardClicked(string id)
        {
            if (_animating || _cards == null)
            {
                return;
            }

            int index = IndexOf(id);
            if (index < 0 || index == _activeIndex)
            {
                return;
            }

            // Flank click → rotate so that card becomes center.
            int delta = index - _activeIndex;
            Navigate(delta);
        }

        private void Navigate(int delta)
        {
            if (_animating || _cards == null || _cards.Length < 2 || delta == 0)
            {
                return;
            }

            int count = _cards.Length;
            int nextIndex = ((_activeIndex + delta) % count + count) % count;
            if (nextIndex == _activeIndex)
            {
                return;
            }

            // Selection updates immediately so Confirm / PlayMode asserts stay sync-friendly.
            _activeIndex = nextIndex;
            NotifySelection();

            if (_motion != null)
            {
                StopCoroutine(_motion);
            }

            // With 2 items prev/next both swap; flank side follows travel so the rotate reads L/R.
            _motion = StartCoroutine(CrossfadeRoutine(delta > 0 ? 0.72f : 0.28f));
        }

        private IEnumerator CrossfadeRoutine(float flankAnchorX)
        {
            _animating = true;

            // After index bump, active is the new center. Capture live poses (no snap) then lerp.
            Card center = _cards[_activeIndex];
            Card flank = _cards[OtherIndex(_activeIndex)];
            Role fromCenter = CaptureRole(center);
            Role fromFlank = CaptureRole(flank);
            Role toCenter = Role.Center;
            Role toFlank = Role.Flank(flankAnchorX);

            Color bgFrom = _bg.style.backgroundColor.value;
            Color bgTo = BackgroundFor(center.Id);
            string ghostFrom = _ghost.text;
            string ghostTo = center.Label;
            float ghostAlphaFrom = _ghost.style.color.value.a;
            float ghostAlphaTo = UiStyle.CharSelectGhost.a;

            yield return UiMotion.Animate(CrossfadeSeconds, t =>
            {
                Role centerNow = Role.Lerp(fromCenter, toCenter, t);
                ApplyRole(center, centerNow);
                ApplyRole(flank, Role.Lerp(fromFlank, toFlank, t));
                UpdateGlow(center, centerNow);
                _bg.style.backgroundColor = Color.LerpUnclamped(bgFrom, bgTo, t);

                Color c = UiStyle.CharSelectGhost;
                // Soft label swap mid-crossfade.
                if (t >= 0.45f)
                {
                    c.a = Mathf.LerpUnclamped(0.2f, ghostAlphaTo, (t - 0.45f) / 0.55f);
                    _ghost.text = ghostTo;
                }
                else
                {
                    c.a = Mathf.LerpUnclamped(ghostAlphaFrom, 0.2f, t / 0.45f);
                    _ghost.text = ghostFrom;
                }

                _ghost.style.color = c;

                center.Element.BringToFront();
            });

            ApplyRole(center, toCenter);
            ApplyRole(flank, toFlank);
            UpdateGlow(center, toCenter);
            _bg.style.backgroundColor = bgTo;
            _ghost.text = ghostTo;
            _ghost.style.color = UiStyle.CharSelectGhost;

            _animating = false;
            _motion = null;
        }

        private static Role CaptureRole(Card card) => card.Current;

        private void ApplyRolesInstant()
        {
            if (_cards == null || _cards.Length == 0)
            {
                return;
            }

            Card center = _cards[_activeIndex];
            Card flank = _cards[OtherIndex(_activeIndex)];
            ApplyRole(center, Role.Center);
            ApplyRole(flank, Role.Flank(0.72f));
            UpdateGlow(center, Role.Center);
            center.Element.BringToFront();

            _bg.style.backgroundColor = BackgroundFor(center.Id);
            _ghost.text = center.Label;
            _ghost.style.color = UiStyle.CharSelectGhost;
        }

        private void NotifySelection()
        {
            string id = SelectedId;
            if (_detail != null)
            {
                _detail.text = id == "Juggernaut"
                    ? "Juggernaut — Speed: slow · Agility: stance/shoot switch costs · Strength: doors faster"
                    : "Scout — Speed: fast · Agility: free stance/shoot switches · Strength: standard doors";
            }

            SelectionChanged?.Invoke(id);
        }

        private int IndexOf(string id)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private int OtherIndex(int index) => index == 0 ? 1 : 0;

        private static Color BackgroundFor(string id) =>
            id == "Juggernaut" ? UiStyle.CharSelectBgJuggernaut : UiStyle.CharSelectBgScout;

        private static Color GlowFor(string id) =>
            id == "Juggernaut" ? UiStyle.CharSelectGlowJuggernaut : UiStyle.CharSelectGlowScout;

        /// <summary>
        /// Rides the same role lerp as the center card's own transform, so the halo grows/fades in
        /// step with the crossfade rather than snapping in once the card settles.
        /// </summary>
        private void UpdateGlow(Card center, Role role)
        {
            if (_glowRings == null)
            {
                return;
            }

            float strength = Mathf.InverseLerp(Role.Flank(0f).Scale, Role.Center.Scale, role.Scale);
            strength = Mathf.Clamp01(strength) * role.Alpha;
            Color tint = GlowFor(center.Id);

            for (int i = 0; i < _glowRings.Length; i++)
            {
                GlowRing ring = _glowRings[i];
                float w = role.Width + (ring.Padding * 2f);
                float h = role.Height + (ring.Padding * 2f);

                ring.Element.style.left = Length.Percent(role.AnchorX * 100f);
                ring.Element.style.bottom = Length.Percent(CardAnchorY * 100f);
                ring.Element.style.width = w;
                ring.Element.style.height = h;
                ring.Element.style.marginLeft = -w / 2f;
                ring.Element.style.scale = new Scale(new Vector3(role.Scale, role.Scale, 1f));
                ring.Element.style.transformOrigin = new TransformOrigin(Length.Percent(50f), Length.Percent(100f));

                Color c = tint;
                c.a = ring.MaxAlpha * strength;
                ring.Element.style.backgroundColor = c;
            }
        }

        private static void ApplyRole(Card card, Role role)
        {
            card.Current = role;

            VisualElement el = card.Element;
            el.style.left = Length.Percent(role.AnchorX * 100f);
            el.style.bottom = Length.Percent(CardAnchorY * 100f);
            el.style.width = role.Width;
            el.style.height = role.Height;
            el.style.marginLeft = -role.Width / 2f;
            el.style.scale = new Scale(new Vector3(role.Scale, role.Scale, 1f));
            el.style.transformOrigin = new TransformOrigin(Length.Percent(50f), Length.Percent(100f));
            el.style.opacity = role.Alpha;

            Color face = card.Face;
            // Dim flank faces slightly without a real blur pass.
            float mul = Mathf.Lerp(0.72f, 1f, Mathf.InverseLerp(0.75f, 1f, role.Alpha));
            el.style.backgroundColor = new Color(face.r * mul, face.g * mul, face.b * mul, 1f);
        }

        private sealed class Card
        {
            public string Id;
            public string Label;
            public Color Face;
            public VisualElement Element;
            public Role Current;
        }

        private readonly struct GlowRing
        {
            public readonly VisualElement Element;
            public readonly float Padding;
            public readonly float MaxAlpha;

            public GlowRing(VisualElement element, float padding, float maxAlpha)
            {
                Element = element;
                Padding = padding;
                MaxAlpha = maxAlpha;
            }
        }

        private readonly struct Role
        {
            public readonly float AnchorX;
            public readonly float Width;
            public readonly float Height;
            public readonly float Scale;
            public readonly float Alpha;

            public Role(float anchorX, float width, float height, float scale, float alpha)
            {
                AnchorX = anchorX;
                Width = width;
                Height = height;
                Scale = scale;
                Alpha = alpha;
            }

            public static Role Center => new Role(0.5f, 260f, 420f, 1.35f, 1f);

            public static Role Flank(float anchorX) => new Role(anchorX, 180f, 280f, 0.92f, 0.82f);

            public static Role Lerp(Role a, Role b, float t) => new Role(
                Mathf.LerpUnclamped(a.AnchorX, b.AnchorX, t),
                Mathf.LerpUnclamped(a.Width, b.Width, t),
                Mathf.LerpUnclamped(a.Height, b.Height, t),
                Mathf.LerpUnclamped(a.Scale, b.Scale, t),
                Mathf.LerpUnclamped(a.Alpha, b.Alpha, t));
        }
    }
}
