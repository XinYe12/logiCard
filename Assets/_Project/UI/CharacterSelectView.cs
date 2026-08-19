using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Character Select carousel: 2-item center/flank role crossfade (~650ms) matching the
    /// Reference 1 feel in <c>docs/UI_CHARACTER_SELECT_ANIMATION_REF.md</c> — not a React port.
    /// Card hit targets keep PlayMode names <c>Pick_Scout</c> / <c>Pick_Juggernaut</c>.
    /// Card faces are Kenney "UI Pack - Adventure" (CC0) 9-slice sprites — see
    /// <c>Assets/_Project/Art/UI/THIRD_PARTY.md</c> for provenance/why. Everything around them (the lit
    /// backdrop, headline, nav buttons, detail plate) is shell chrome from <see cref="UiFactory"/>;
    /// see <c>docs/ui/UI_SHELL_CHROME.md</c>.
    /// </summary>
    public sealed class CharacterSelectView : MonoBehaviour
    {
        private const float CrossfadeSeconds = UiMotion.DefaultDuration;
        private const string SpriteResourceRoot = "CharSelect/";

        private UiFactory _ui;
        private Image _bg;
        private Text _ghost;
        private Text _detail;
        private Card[] _cards;
        private GlowRing[] _glowRings;
        private int _activeIndex;
        private bool _animating;
        private Coroutine _motion;

        public string SelectedId => _cards != null && _cards.Length > 0
            ? _cards[_activeIndex].Id
            : "Scout";

        public event Action<string> SelectionChanged;

        /// <summary>
        /// Builds the carousel under an existing Character Select screen root.
        /// Confirm is owned by the host so navigation stays in <see cref="AppFlowController"/>.
        /// <paramref name="backdropGlow"/> is the screen's light-pool Image from
        /// <see cref="UiFactory.CreateShellBackdrop"/> — the archetype crossfade re-tints *that*, not a
        /// flat full-screen fill. (Before the 2026-08-18 shell chrome pass this lerped the screen root's
        /// own Image, which is why Character Select read as one muddy solid brown rectangle.)
        /// </summary>
        public static CharacterSelectView Build(UiFactory ui, RectTransform screenRoot, Image backdropGlow = null)
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

            view._ui = ui;
            view._bg = backdropGlow;
            view.BuildChrome(screenRoot);
            view.ApplyRolesInstant();
            view.NotifySelection();
            return view;
        }

        private void BuildChrome(RectTransform root)
        {
            Text brand = _ui.CreateText(root, "Brand", "LOGICARD", 18, TextAnchor.MiddleLeft, UiStyle.ShellMutedInk,
                UiTextOverflow.SingleLine);
            brand.font = _ui.Display;
            UiFactory.Stretch(brand.rectTransform, new Vector2(0.045f, 0.925f), new Vector2(0.4f, 0.975f));

            Text title = _ui.CreateHeadline(root, "Title", "CHARACTER SELECT", 40, UiStyle.ShellTitleInk);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.2f, 0.885f), new Vector2(0.8f, 0.975f));

            _ui.CreateRule(root, "TitleRule", new Vector2(0.47f, 0.868f), new Vector2(0.53f, 0.876f));

            _ghost = _ui.CreateText(root, "GhostHeadline", "SCOUT", 190, TextAnchor.MiddleCenter, UiStyle.CharSelectGhost,
                UiTextOverflow.SingleLine);
            _ghost.font = _ui.Display;
            _ghost.raycastTarget = false;
            UiFactory.Stretch(_ghost.rectTransform, new Vector2(0.02f, 0.30f), new Vector2(0.98f, 0.80f));
            // Just above the backdrop, below everything else — NOT SetAsFirstSibling, which would bury
            // it (and the stage) under the backdrop's own void/glow/grain/vignette layers.
            _ghost.transform.SetSiblingIndex(UiFactory.ShellBackdropLayerCount);

            // Stage floor sits above the detail plate (top edge 0.255) — the centre card is ~0.52 of the
            // frame tall, so a lower floor buries its name plate behind the parchment.
            RectTransform stage = _ui.CreatePanel(root, "CarouselStage", new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.29f), new Vector2(1f, 0.90f));
            stage.GetComponent<Image>().raycastTarget = false;
            stage.SetSiblingIndex(UiFactory.ShellBackdropLayerCount + 1);

            // Glow rings sit behind both cards (created first = lowest sibling index in stage) and
            // always track whichever card is currently the center, so the halo rides the crossfade.
            _glowRings = new[]
            {
                CreateGlowRing(stage, "GlowOuter", padding: 90f, maxAlpha: 0.22f),
                CreateGlowRing(stage, "GlowInner", padding: 34f, maxAlpha: 0.34f),
            };

            _cards = new[]
            {
                CreateCard(stage, "Scout", "SCOUT", "RECON"),
                CreateCard(stage, "Juggernaut", "JUGGERNAUT", "BREACHER"),
            };

            Button prev = _ui.CreateShellButton(root, "CharSelectPrev", "PREV", ShellButtonTone.Quiet, 22,
                () => Navigate(-1), riser: 6f);
            UiFactory.Stretch(prev.GetComponent<RectTransform>(), new Vector2(0.055f, 0.44f), new Vector2(0.145f, 0.53f));

            Button next = _ui.CreateShellButton(root, "CharSelectNext", "NEXT", ShellButtonTone.Quiet, 22,
                () => Navigate(1), riser: 6f);
            UiFactory.Stretch(next.GetComponent<RectTransform>(), new Vector2(0.855f, 0.44f), new Vector2(0.945f, 0.53f));

            RectTransform detailPlate = _ui.CreateShellPlate(root, "DetailPlate",
                new Vector2(0.17f, 0.155f), new Vector2(0.83f, 0.255f));
            _detail = _ui.CreateText(detailPlate, "Detail", string.Empty, 21, TextAnchor.MiddleCenter, UiStyle.ModalInk);
            UiFactory.Stretch(_detail.rectTransform, new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.92f));

            _activeIndex = 0;
        }

        /// <summary>
        /// Loads a Kenney "UI Pack - Adventure" sprite from <c>Resources/CharSelect/</c>. Cached per
        /// name — cheap either way since <see cref="Resources.Load{T}(string)"/> itself caches, but
        /// avoids repeat string concatenation across the handful of call sites.
        /// </summary>
        private static Sprite LoadSprite(string name) => Resources.Load<Sprite>(SpriteResourceRoot + name);

        private static Sprite CardSpriteFor(string id) =>
            LoadSprite(id == "Juggernaut" ? "panel_brown_dark" : "panel_brown");

        /// <summary>
        /// One archetype card. The Kenney 9-slice wood/parchment face stays (real texture, correct warm
        /// family), but the card is no longer "one enormous word on a panel": it gets a contact shadow
        /// so it lifts off the backdrop, a sunken emblem well with the archetype monogram, a dark name
        /// plate, and a small role line. That silhouette — shadowed panel, round well, plate, caption —
        /// is the toy/diorama read the shell is aiming for.
        /// </summary>
        private Card CreateCard(RectTransform stage, string id, string label, string role)
        {
            var go = new GameObject($"Pick_{id}", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(stage, false);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchorMin = new Vector2(0.5f, 0.04f);
            rt.anchorMax = new Vector2(0.5f, 0.04f);
            rt.sizeDelta = new Vector2(220f, 360f);

            // Kenney's panel_brown face is a cool grey-tan; untinted it reads washed-out and out-of-family
            // against the warm backdrop, while panel_brown_dark is already warm. Tint per sprite so both
            // cards land in the same cardstock family.
            Color baseTint = id == "Juggernaut"
                ? new Color(1f, 0.98f, 0.95f, 1f)
                : new Color(1f, 0.94f, 0.83f, 1f);

            var image = go.GetComponent<Image>();
            image.color = baseTint;
            image.sprite = CardSpriteFor(id);
            image.type = Image.Type.Sliced;

            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 1f;

            // Contact shadow as a child (not a sibling) so it inherits the card's own role lerp —
            // position, size and scale all ride the crossfade for free.
            RectTransform shadow = _ui.CreatePanel(rt, "CardShadow", UiStyle.ShellButtonShadow,
                Vector2.zero, Vector2.one, UiStyle.RoundSprite, Image.Type.Sliced);
            shadow.offsetMin = new Vector2(-8f, -20f);
            shadow.offsetMax = new Vector2(8f, -8f);
            shadow.GetComponent<Image>().raycastTarget = false;
            shadow.SetAsFirstSibling();

            // Emblem well: soft radial pool + monogram, standing in for a portrait until real archetype
            // art exists. Deliberately not one of docs/ui-collection/icons/ — those ship as JPEGs on
            // flat white with no alpha yet, so they would paste a white square onto the card.
            RectTransform well = _ui.CreatePanel(rt, "EmblemWell", new Color(0.24f, 0.15f, 0.08f, 0.32f),
                new Vector2(0.16f, 0.44f), new Vector2(0.84f, 0.88f), UiStyle.RadialSprite);
            well.GetComponent<Image>().raycastTarget = false;

            Text monogram = _ui.CreateText(well, "Monogram", label.Substring(0, 1), 96, TextAnchor.MiddleCenter,
                UiStyle.ShellTitleInk, UiTextOverflow.SingleLine);
            monogram.font = _ui.Display;
            monogram.raycastTarget = false;
            monogram.resizeTextForBestFit = true;
            monogram.resizeTextMinSize = 28;
            monogram.resizeTextMaxSize = 110;
            UiFactory.Stretch(monogram.rectTransform, new Vector2(0.18f, 0.14f), new Vector2(0.82f, 0.86f));
            var monogramShadow = monogram.gameObject.AddComponent<Shadow>();
            monogramShadow.effectColor = UiStyle.ShellTitleShadow;
            monogramShadow.effectDistance = new Vector2(3f, -3f);

            RectTransform plate = _ui.CreatePanel(rt, "NamePlate", UiStyle.ShellSlateFace,
                new Vector2(0.08f, 0.245f), new Vector2(0.92f, 0.395f), UiStyle.PillSprite, Image.Type.Sliced);
            plate.GetComponent<Image>().raycastTarget = false;

            // Body face for the name/role captions — same display-vs-read split as button labels
            // (see UiFactory.CreateShellButton); the Display face is reserved for the monogram here.
            Text text = _ui.CreateText(plate, "Label", label, 26, TextAnchor.MiddleCenter, UiStyle.ShellTitleInk,
                UiTextOverflow.Button);
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = 28;
            UiFactory.Stretch(text.rectTransform, new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.9f));

            Text roleText = _ui.CreateText(rt, "Role", role, 16, TextAnchor.MiddleCenter, UiStyle.ShellAccent,
                UiTextOverflow.SingleLine);
            roleText.fontStyle = FontStyle.Bold;
            roleText.raycastTarget = false;
            UiFactory.Stretch(roleText.rectTransform, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.225f));

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            string capturedId = id;
            button.onClick.AddListener(() => OnCardClicked(capturedId));

            return new Card
            {
                Id = id,
                Label = label,
                Root = rt,
                Group = group,
                FaceImage = image,
                BaseTint = baseTint,
            };
        }

        private GlowRing CreateGlowRing(RectTransform stage, string name, float padding, float maxAlpha)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(stage, false);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchorMin = new Vector2(0.5f, 0.04f);
            rt.anchorMax = new Vector2(0.5f, 0.04f);

            var image = go.GetComponent<Image>();
            // Radial falloff, not a sliced rounded rect — a hard-edged tinted rectangle behind the card
            // reads as a second panel, which is half of what made this screen look like placeholder art.
            image.sprite = UiStyle.RadialSprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0f);

            return new GlowRing(rt, image, padding, maxAlpha);
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

            Color bgFrom = _bg != null ? _bg.color : UiStyle.ShellGlowDefault;
            Color bgTo = BackgroundFor(center.Id);
            string ghostFrom = _ghost != null ? _ghost.text : center.Label;
            string ghostTo = center.Label;
            float ghostAlphaFrom = _ghost != null ? _ghost.color.a : UiStyle.CharSelectGhost.a;
            float ghostAlphaTo = UiStyle.CharSelectGhost.a;

            yield return UiMotion.Animate(CrossfadeSeconds, t =>
            {
                Role centerNow = Role.Lerp(fromCenter, toCenter, t);
                ApplyRole(center, centerNow);
                ApplyRole(flank, Role.Lerp(fromFlank, toFlank, t));
                UpdateGlow(center, centerNow);
                if (_bg != null)
                {
                    _bg.color = Color.LerpUnclamped(bgFrom, bgTo, t);
                }

                if (_ghost != null)
                {
                    Color c = UiStyle.CharSelectGhost;
                    c.a = Mathf.LerpUnclamped(ghostAlphaFrom, ghostAlphaTo, t);
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

                    _ghost.color = c;
                }

                center.Root.SetAsLastSibling();
            });

            ApplyRole(center, toCenter);
            ApplyRole(flank, toFlank);
            UpdateGlow(center, toCenter);
            if (_bg != null)
            {
                _bg.color = bgTo;
            }

            if (_ghost != null)
            {
                _ghost.text = ghostTo;
                _ghost.color = UiStyle.CharSelectGhost;
            }

            _animating = false;
            _motion = null;
        }

        private static Role CaptureRole(Card card)
        {
            float scale = card.Root.localScale.x;
            return new Role(
                card.Root.anchorMin.x,
                card.Root.sizeDelta.x,
                card.Root.sizeDelta.y,
                scale,
                card.Group.alpha);
        }

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
            center.Root.SetAsLastSibling();

            if (_bg != null)
            {
                _bg.color = BackgroundFor(center.Id);
            }

            if (_ghost != null)
            {
                _ghost.text = center.Label;
                _ghost.color = UiStyle.CharSelectGhost;
            }
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
                ring.Root.anchorMin = new Vector2(role.AnchorX, 0.04f);
                ring.Root.anchorMax = new Vector2(role.AnchorX, 0.04f);
                ring.Root.anchoredPosition = Vector2.zero;
                ring.Root.sizeDelta = new Vector2(role.Width + (ring.Padding * 2f), role.Height + (ring.Padding * 2f));
                ring.Root.localScale = new Vector3(role.Scale, role.Scale, 1f);

                Color c = tint;
                c.a = ring.MaxAlpha * strength;
                ring.Image.color = c;
            }
        }

        private static void ApplyRole(Card card, Role role)
        {
            card.Root.anchorMin = new Vector2(role.AnchorX, 0.04f);
            card.Root.anchorMax = new Vector2(role.AnchorX, 0.04f);
            card.Root.anchoredPosition = Vector2.zero;
            card.Root.sizeDelta = new Vector2(role.Width, role.Height);
            card.Root.localScale = new Vector3(role.Scale, role.Scale, 1f);
            card.Group.alpha = role.Alpha;

            // Dim flank faces slightly without a real blur pass — multiplies the sprite's own baked
            // wood/parchment color toward grey rather than tinting a flat fill, so the flank still
            // reads as "the same card, dimmer," not a different material.
            float mul = Mathf.Lerp(0.72f, 1f, Mathf.InverseLerp(0.75f, 1f, role.Alpha));
            Color tint = card.BaseTint;
            card.FaceImage.color = new Color(tint.r * mul, tint.g * mul, tint.b * mul, 1f);
        }

        private sealed class Card
        {
            public string Id;
            public string Label;
            public RectTransform Root;
            public CanvasGroup Group;
            public Image FaceImage;

            /// <summary>Per-sprite warm correction; the flank dim multiplies this rather than white.</summary>
            public Color BaseTint;
        }

        private readonly struct GlowRing
        {
            public readonly RectTransform Root;
            public readonly Image Image;
            public readonly float Padding;
            public readonly float MaxAlpha;

            public GlowRing(RectTransform root, Image image, float padding, float maxAlpha)
            {
                Root = root;
                Image = image;
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
