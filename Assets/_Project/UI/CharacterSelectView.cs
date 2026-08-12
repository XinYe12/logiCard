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
    /// </summary>
    public sealed class CharacterSelectView : MonoBehaviour
    {
        private const float CrossfadeSeconds = UiMotion.DefaultDuration;

        private UiFactory _ui;
        private Image _bg;
        private Text _ghost;
        private Text _detail;
        private Card[] _cards;
        private int _activeIndex;
        private bool _animating;
        private Coroutine _motion;

        public string SelectedId => _cards != null && _cards.Length > 0
            ? _cards[_activeIndex].Id
            : "Scout";

        public event Action<string> SelectionChanged;

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

            view._ui = ui;
            view._bg = screenRoot.GetComponent<Image>();
            view.BuildChrome(screenRoot);
            view.ApplyRolesInstant();
            view.NotifySelection();
            return view;
        }

        private void BuildChrome(RectTransform root)
        {
            Text brand = _ui.CreateText(root, "Brand", "LOGICARD", 18, TextAnchor.UpperLeft, UiStyle.Ink,
                UiTextOverflow.SingleLine);
            brand.fontStyle = FontStyle.Bold;
            UiFactory.Stretch(brand.rectTransform, new Vector2(0.04f, 0.92f), new Vector2(0.4f, 0.98f));

            Text title = _ui.CreateText(root, "Title", "CHARACTER SELECT", 28, TextAnchor.MiddleCenter, UiStyle.Ink,
                UiTextOverflow.SingleLine);
            title.fontStyle = FontStyle.Bold;
            UiFactory.Stretch(title.rectTransform, new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.96f));

            _ghost = _ui.CreateText(root, "GhostHeadline", "SCOUT", 120, TextAnchor.MiddleCenter, UiStyle.CharSelectGhost,
                UiTextOverflow.SingleLine);
            _ghost.fontStyle = FontStyle.Bold;
            _ghost.raycastTarget = false;
            UiFactory.Stretch(_ghost.rectTransform, new Vector2(0.02f, 0.28f), new Vector2(0.98f, 0.78f));
            _ghost.transform.SetAsFirstSibling();

            RectTransform stage = _ui.CreatePanel(root, "CarouselStage", new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.18f), new Vector2(1f, 0.88f));
            stage.SetSiblingIndex(1);

            _cards = new[]
            {
                CreateCard(stage, "Scout", "SCOUT", UiStyle.CharSelectCardScout),
                CreateCard(stage, "Juggernaut", "JUGGERNAUT", UiStyle.CharSelectCardJuggernaut),
            };

            Button prev = _ui.CreateButton(root, "CharSelectPrev", "<", new Color(1f, 1f, 1f, 0.12f), UiStyle.Ink, 36,
                () => Navigate(-1), UiStyle.RoundSprite, Image.Type.Sliced);
            UiFactory.Stretch(prev.GetComponent<RectTransform>(), new Vector2(0.06f, 0.28f), new Vector2(0.14f, 0.40f));
            StyleNavButton(prev);

            Button next = _ui.CreateButton(root, "CharSelectNext", ">", new Color(1f, 1f, 1f, 0.12f), UiStyle.Ink, 36,
                () => Navigate(1), UiStyle.RoundSprite, Image.Type.Sliced);
            UiFactory.Stretch(next.GetComponent<RectTransform>(), new Vector2(0.86f, 0.28f), new Vector2(0.94f, 0.40f));
            StyleNavButton(next);

            _detail = _ui.CreateText(root, "Detail", string.Empty, 22, TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(_detail.rectTransform, new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.24f));

            _activeIndex = 0;
        }

        private static void StyleNavButton(Button button)
        {
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(1f, 1f, 1f, 0.14f);
            }

            var outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private Card CreateCard(RectTransform stage, string id, string label, Color face)
        {
            var go = new GameObject($"Pick_{id}", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(stage, false);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchorMin = new Vector2(0.5f, 0.04f);
            rt.anchorMax = new Vector2(0.5f, 0.04f);
            rt.sizeDelta = new Vector2(220f, 360f);

            var image = go.GetComponent<Image>();
            image.color = face;
            image.sprite = UiStyle.RoundSprite;
            image.type = Image.Type.Sliced;

            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 1f;

            Text text = _ui.CreateText(rt, "Label", label, 34, TextAnchor.MiddleCenter, UiStyle.InkDark,
                UiTextOverflow.Button);
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 18;
            text.resizeTextMaxSize = 40;
            UiFactory.Stretch(text.rectTransform, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f));

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            string capturedId = id;
            button.onClick.AddListener(() => OnCardClicked(capturedId));

            return new Card
            {
                Id = id,
                Label = label,
                Face = face,
                Root = rt,
                Group = group,
                FaceImage = image,
            };
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

            Color bgFrom = _bg != null ? _bg.color : UiStyle.PanelDark;
            Color bgTo = BackgroundFor(center.Id);
            string ghostFrom = _ghost != null ? _ghost.text : center.Label;
            string ghostTo = center.Label;
            float ghostAlphaFrom = _ghost != null ? _ghost.color.a : UiStyle.CharSelectGhost.a;
            float ghostAlphaTo = UiStyle.CharSelectGhost.a;

            yield return UiMotion.Animate(CrossfadeSeconds, t =>
            {
                ApplyRole(center, Role.Lerp(fromCenter, toCenter, t));
                ApplyRole(flank, Role.Lerp(fromFlank, toFlank, t));
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

        private static void ApplyRole(Card card, Role role)
        {
            card.Root.anchorMin = new Vector2(role.AnchorX, 0.04f);
            card.Root.anchorMax = new Vector2(role.AnchorX, 0.04f);
            card.Root.anchoredPosition = Vector2.zero;
            card.Root.sizeDelta = new Vector2(role.Width, role.Height);
            card.Root.localScale = new Vector3(role.Scale, role.Scale, 1f);
            card.Group.alpha = role.Alpha;

            Color face = card.Face;
            face.a = 1f;
            // Dim flank faces slightly without a real blur pass.
            float mul = Mathf.Lerp(0.72f, 1f, Mathf.InverseLerp(0.75f, 1f, role.Alpha));
            card.FaceImage.color = new Color(face.r * mul, face.g * mul, face.b * mul, 1f);
        }

        private sealed class Card
        {
            public string Id;
            public string Label;
            public Color Face;
            public RectTransform Root;
            public CanvasGroup Group;
            public Image FaceImage;
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
