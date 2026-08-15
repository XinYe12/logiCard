using System;
using System.Collections.Generic;
using LogiCard.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Which match phase the gear hand is presenting for. Program arms Bandage / Interact /
    /// Flashbang; Execute surfaces Adrenaline (UI_FLOW §4 / §7). Presentation only — does not
    /// drive <c>RoundPhase</c> or resolve.
    /// </summary>
    public enum GearHandPhase
    {
        Program = 0,
        Execute = 1,
    }

    /// <summary>
    /// Static display copy for one first-wave gear card. Costs stay placeholders until OPEN #16
    /// / card-economy numerics lock — never invent a TR number here.
    /// </summary>
    public readonly struct GearHandCardInfo
    {
        public CardId Id { get; }
        public string DisplayName { get; }
        public string CostLabel { get; }
        public string PhaseHint { get; }
        public bool OncePerMatch { get; }

        public GearHandCardInfo(CardId id, string displayName, string costLabel, string phaseHint, bool oncePerMatch)
        {
            Id = id;
            DisplayName = displayName;
            CostLabel = costLabel;
            PhaseHint = phaseHint;
            OncePerMatch = oncePerMatch;
        }
    }

    /// <summary>
    /// UI-only first-wave gear hand scaffold (C62), fanned Hearthstone-style with a real
    /// press→hold→drag-out→release play gesture for Bandage / Storm (Hand Deck Drag Play brief,
    /// 2026-08-15). Builds a horizontal cardstock strip for Bandage / Interact / Flashbang /
    /// Adrenaline / Storm using the same <c>Modal*</c> paper tokens as <see cref="ModalDialog"/>.
    /// No Sim, schedule, or resolve logic — <see cref="ProgramHud"/> calls into
    /// <c>BoardInputController</c> when a drag commits; Integrator wires remaining costs later
    /// when OPEN #16 closes.
    /// </summary>
    public sealed class GearHandView
    {
        /// <summary>Stable GameObject name prefix for PlayMode lookups (<c>Gear_Bandage</c>, …).</summary>
        public const string ButtonNamePrefix = "Gear_";

        /// <summary>Root panel name under the parent canvas/dock slot.</summary>
        public const string RootName = "GearHand";

        /// <summary>
        /// Screen-pixel distance a Bandage/Storm card must travel before release counts as "play"
        /// instead of "picked it up and put it back" — matched against
        /// <c>PointerEventData.pressPosition</c> vs the release position in
        /// <see cref="CardDragController.OnEndDrag"/>.
        /// </summary>
        public const float DragThresholdPixels = 60f;

        /// <summary>
        /// First-wave roster in dock order. Cost labels are explicit placeholders (em dash) until a
        /// card's numerics lock — Bandage's did (C63, Bandage HUD-side contract), so it alone shows a
        /// real cost; the rest stay em-dash so a future numeric pass cannot silently inherit invented
        /// values.
        /// </summary>
        public static readonly GearHandCardInfo[] FirstWave =
        {
            new GearHandCardInfo(CardId.Bandage, "Bandage", "3s", "PROGRAM", oncePerMatch: true),
            new GearHandCardInfo(CardId.Interact, "Interact", "TR —", "PROGRAM", oncePerMatch: false),
            new GearHandCardInfo(CardId.Flashbang, "Flashbang", "TR —", "PROGRAM", oncePerMatch: true),
            new GearHandCardInfo(CardId.Adrenaline, "Adrenaline", "TR —", "PLAYBACK", oncePerMatch: true),
            new GearHandCardInfo(CardId.Storm, "Storm", "TR —", "PROGRAM", oncePerMatch: true),
        };

        private readonly Dictionary<CardId, CardSlot> _slots = new Dictionary<CardId, CardSlot>();
        private readonly HashSet<CardId> _spent = new HashSet<CardId>();
        private GearHandPhase _phase = GearHandPhase.Program;
        private CardId? _armedId;
        private CardId? _draggingId;

        public RectTransform Root { get; private set; }
        public GearHandPhase Phase => _phase;
        public CardId? ArmedId => _armedId;

        /// <summary>Raised when the player arms a legal card. Never schedules or resolves.</summary>
        public event Action<CardId> CardArmed;

        /// <summary>Raised when arm clears (re-click armed card, phase swap, or <see cref="ClearArm"/>).</summary>
        public event Action ArmCleared;

        /// <summary>
        /// Raised when a Bandage/Storm card is dragged past <see cref="DragThresholdPixels"/> and
        /// released outside <see cref="Root"/> — the entire play gesture for these two cards.
        /// <see cref="ProgramHud"/> is the only subscriber; it calls
        /// <c>BoardInputController.TryQueueBandageAt</c>/<c>TryQueueStormAt</c> at the scrubber's
        /// current seconds. A short drag or a release back inside the hand never raises this — the
        /// card just snaps back with no play attempt (see <see cref="CardDragController"/>).
        /// </summary>
        public event Action<CardId> CardPlayRequested;

        private GearHandView()
        {
        }

        public static string ButtonName(CardId id) => ButtonNamePrefix + id;

        public Button GetButton(CardId id)
        {
            return _slots.TryGetValue(id, out CardSlot slot) ? slot.Button : null;
        }

        /// <summary>Test/inspection hook — the drag controller attached to Bandage/Storm cells (null for the rest).</summary>
        public CardDragController GetDragController(CardId id)
        {
            return _slots.TryGetValue(id, out CardSlot slot) ? slot.DragController : null;
        }

        /// <summary>
        /// Builds the hand strip in <paramref name="area"/> (normalized anchors under
        /// <paramref name="parent"/>). Caller owns parenting into the HUD dock later.
        /// </summary>
        public static GearHandView Build(
            UiFactory ui,
            RectTransform parent,
            Vector2 areaMin,
            Vector2 areaMax)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            RectTransform root = ui.CreatePanel(parent, RootName, new Color(0f, 0f, 0f, 0f), areaMin, areaMax);
            var hand = new GearHandView { Root = root };

            int count = FirstWave.Length;

            // Hearthstone-style fan (Hand Deck Drag Play brief): cards are widened past an even
            // split so neighbors overlap ~18%, evenly stepped so the first card's left edge sits at
            // 0 and the last card's right edge sits at 1 — no matter the count. A slight rotation +
            // vertical droop per step (biggest at the outer cards, none at the center) reads as a
            // held fan without needing a bottom-pivot rig; it is purely cosmetic and does not affect
            // drag math (thresholds/bounds are measured against Root, not the rotated cell).
            const float overlapWiden = 1.18f;
            float cardFrac = count > 0 ? Mathf.Min(1f, (1f / count) * overlapWiden) : 1f;
            float step = count > 1 ? (1f - cardFrac) / (count - 1) : 0f;
            float centerIndex = (count - 1) / 2f;
            const float insetY = 0.05f;
            const float fanRotationDegreesPerStep = 5f;
            const float fanDropPixelsPerStep = 9f;

            for (int i = 0; i < count; i++)
            {
                GearHandCardInfo info = FirstWave[i];
                float x0 = i * step;
                float x1 = x0 + cardFrac;

                RectTransform cell = ui.CreatePanel(root, $"Slot_{info.Id}", new Color(0f, 0f, 0f, 0f),
                    new Vector2(x0, insetY), new Vector2(x1, 1f - insetY));

                float fanStep = i - centerIndex;
                cell.localEulerAngles = new Vector3(0f, 0f, -fanStep * fanRotationDegreesPerStep);
                cell.anchoredPosition += new Vector2(0f, -Mathf.Abs(fanStep) * fanDropPixelsPerStep);

                // Soft shadow / paper rim / face — same stack language as ModalDialog.
                RectTransform shadow = ui.CreatePanel(cell, "CardShadow", UiStyle.ModalShadow,
                    Vector2.zero, Vector2.one, UiStyle.RoundSprite, Image.Type.Sliced);
                shadow.offsetMin = new Vector2(4f, -6f);
                shadow.offsetMax = new Vector2(6f, -2f);
                shadow.GetComponent<Image>().raycastTarget = false;

                RectTransform border = ui.CreatePanel(cell, "CardBorder", UiStyle.ModalCardBorder,
                    Vector2.zero, Vector2.one, UiStyle.RoundSprite, Image.Type.Sliced);
                border.offsetMin = new Vector2(-3f, -3f);
                border.offsetMax = new Vector2(3f, 3f);
                border.GetComponent<Image>().raycastTarget = false;

                Button button = ui.CreateButton(
                    cell,
                    ButtonName(info.Id),
                    string.Empty,
                    UiStyle.ModalCard,
                    UiStyle.ModalInk,
                    18,
                    null,
                    UiStyle.RoundSprite,
                    Image.Type.Sliced);
                UiFactory.Stretch(button.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

                // Hide the factory's full-bleed Label; card uses stacked title / cost / hint.
                Text factoryLabel = button.transform.Find("Label")?.GetComponent<Text>();
                if (factoryLabel != null)
                {
                    factoryLabel.text = string.Empty;
                    factoryLabel.enabled = false;
                }

                Text title = ui.CreateText(button.GetComponent<RectTransform>(), "Title", info.DisplayName,
                    20, TextAnchor.MiddleCenter, UiStyle.ModalInk, UiTextOverflow.Button);
                title.fontStyle = FontStyle.Bold;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 12;
                title.resizeTextMaxSize = 22;
                UiFactory.Stretch(title.rectTransform, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.88f));

                Text cost = ui.CreateText(button.GetComponent<RectTransform>(), "Cost", info.CostLabel,
                    14, TextAnchor.MiddleCenter, UiStyle.ModalInk, UiTextOverflow.SingleLine);
                UiFactory.Stretch(cost.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.50f));

                Text hint = ui.CreateText(button.GetComponent<RectTransform>(), "PhaseHint", info.PhaseHint,
                    12, TextAnchor.MiddleCenter, UiStyle.ModalDivider, UiTextOverflow.SingleLine);
                UiFactory.Stretch(hint.rectTransform, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.28f));

                // Bandage/Storm play by drag-out-of-hand-and-release (Hand Deck Drag Play brief) —
                // no click-to-arm for these two anymore, so they carry a drag controller instead of
                // an onClick listener. Interact/Flashbang/Adrenaline are untouched this wave: still
                // the original click-to-arm scaffold (Interact/Flashbang stay permanently blocked
                // below; Adrenaline's real trigger is ProgramHud's separate AdrenalineButton).
                bool dragToPlay = info.Id == CardId.Bandage || info.Id == CardId.Storm;
                CardDragController dragController = null;
                if (dragToPlay)
                {
                    dragController = cell.gameObject.AddComponent<CardDragController>();
                    dragController.Owner = hand;
                    dragController.CardId = info.Id;
                }
                else
                {
                    CardId captured = info.Id;
                    button.onClick.AddListener(() => hand.OnCardClicked(captured));
                }

                var slot = new CardSlot(info, cell, button, title, cost, hint, border.GetComponent<Image>(), dragController);
                hand._slots[info.Id] = slot;
            }

            hand.RefreshPresentation();
            return hand;
        }

        public void SetPhase(GearHandPhase phase)
        {
            if (_phase == phase)
            {
                return;
            }

            _phase = phase;
            // Arming a Program card into Execute (or the reverse) is meaningless for this scaffold.
            ClearArm();
            RefreshPresentation();
        }

        public void ClearArm()
        {
            if (!_armedId.HasValue)
            {
                return;
            }

            _armedId = null;
            RefreshPresentation();
            ArmCleared?.Invoke();
        }

        /// <summary>
        /// Marks a once-per-match card spent for presentation (greyed). Does not mutate match state.
        /// </summary>
        public void SetSpent(CardId id, bool spent = true)
        {
            if (spent)
            {
                _spent.Add(id);
            }
            else
            {
                _spent.Remove(id);
            }

            if (_armedId == id && spent)
            {
                ClearArm();
            }
            else
            {
                RefreshPresentation();
            }
        }

        public bool IsSpent(CardId id) => _spent.Contains(id);

        public bool IsInteractable(CardId id)
        {
            if (!_slots.ContainsKey(id) || _spent.Contains(id))
            {
                return false;
            }

            bool programCard = id != CardId.Adrenaline;
            return _phase == GearHandPhase.Program ? programCard : !programCard;
        }

        private void OnCardClicked(CardId id)
        {
            if (!IsInteractable(id))
            {
                return;
            }

            if (_armedId == id)
            {
                ClearArm();
                return;
            }

            _armedId = id;
            RefreshPresentation();
            CardArmed?.Invoke(id);
        }

        /// <summary>Called by <see cref="CardDragController.OnBeginDrag"/> once a real drag (not a click) starts.</summary>
        private void HandleCardDragBegin(CardId id)
        {
            _draggingId = id;
            RefreshPresentation();
        }

        /// <summary>
        /// Called by <see cref="CardDragController.OnEndDrag"/> for every drag that started, whether
        /// it committed or cancelled. <paramref name="committed"/> is true only when the drag cleared
        /// <see cref="DragThresholdPixels"/> and released outside <see cref="Root"/> — that is the
        /// only condition under which <see cref="CardPlayRequested"/> fires.
        /// </summary>
        private void HandleCardDragEnd(CardId id, bool committed)
        {
            _draggingId = null;
            RefreshPresentation();
            if (committed)
            {
                CardPlayRequested?.Invoke(id);
            }
        }

        private void RefreshPresentation()
        {
            foreach (KeyValuePair<CardId, CardSlot> pair in _slots)
            {
                CardSlot slot = pair.Value;
                bool spent = _spent.Contains(pair.Key);
                bool legal = IsInteractable(pair.Key);
                bool highlighted = _armedId == pair.Key || _draggingId == pair.Key;

                slot.Button.interactable = legal;

                Color face;
                Color ink;
                Color border;
                if (spent)
                {
                    face = Color.Lerp(UiStyle.ModalSecondaryButton, UiStyle.ModalDimmer, 0.35f);
                    ink = new Color(UiStyle.ModalInk.r, UiStyle.ModalInk.g, UiStyle.ModalInk.b, 0.45f);
                    border = new Color(UiStyle.ModalCardBorder.r, UiStyle.ModalCardBorder.g, UiStyle.ModalCardBorder.b, 0.5f);
                }
                else if (highlighted)
                {
                    face = UiStyle.ModalPrimaryButton;
                    ink = UiStyle.ModalPrimaryButtonText;
                    border = UiStyle.ModalPrimaryButton;
                }
                else if (!legal)
                {
                    face = UiStyle.ModalSecondaryButton;
                    ink = new Color(UiStyle.ModalInk.r, UiStyle.ModalInk.g, UiStyle.ModalInk.b, 0.55f);
                    border = new Color(UiStyle.ModalCardBorder.r, UiStyle.ModalCardBorder.g, UiStyle.ModalCardBorder.b, 0.65f);
                }
                else
                {
                    face = UiStyle.ModalCard;
                    ink = UiStyle.ModalInk;
                    border = UiStyle.ModalCardBorder;
                }

                slot.Button.GetComponent<Image>().color = face;
                slot.Border.color = border;
                slot.Title.color = ink;
                slot.Cost.color = ink;
                slot.PhaseHint.color = highlighted ? UiStyle.ModalPrimaryButtonText : UiStyle.ModalDivider;
            }
        }

        private sealed class CardSlot
        {
            public GearHandCardInfo Info { get; }
            public RectTransform Cell { get; }
            public Button Button { get; }
            public Text Title { get; }
            public Text Cost { get; }
            public Text PhaseHint { get; }
            public Image Border { get; }
            public CardDragController DragController { get; }

            public CardSlot(GearHandCardInfo info, RectTransform cell, Button button, Text title, Text cost, Text phaseHint,
                Image border, CardDragController dragController)
            {
                Info = info;
                Cell = cell;
                Button = button;
                Title = title;
                Cost = cost;
                PhaseHint = phaseHint;
                Border = border;
                DragController = dragController;
            }
        }

        /// <summary>
        /// Press→hold→drag-out→release gesture for Bandage/Storm (Hand Deck Drag Play brief). Lives
        /// on the card's cell (parent of the visual <see cref="Button"/>) rather than the button
        /// itself: Unity resolves <c>IDragHandler</c> by walking up from the raycast hit through
        /// parents until it finds one, and <see cref="Button"/>/<c>Selectable</c> never implements
        /// drag interfaces, so it walks straight past the button to here. A plain click that never
        /// crosses Unity's own drag threshold never reaches <see cref="OnBeginDrag"/> at all — and
        /// Bandage/Storm carry no <c>onClick</c> listener in the first place (see <see cref="Build"/>),
        /// so a short press-release is already a no-op cancel with nothing to unwind.
        /// </summary>
        public sealed class CardDragController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            /// <summary>Set once by <see cref="Build"/> right after <c>AddComponent</c>.</summary>
            public GearHandView Owner;

            /// <summary>Which card this cell represents. Set once by <see cref="Build"/>.</summary>
            public CardId CardId;

            private RectTransform _rect;
            private Vector2 _startAnchoredPosition;
            private Quaternion _startLocalRotation;
            private int _startSiblingIndex;
            private bool _dragging;

            /// <summary>
            /// Lazily resolved rather than cached in <c>Awake</c> — EditMode tests (and
            /// <c>AddComponent</c> generally, outside Play mode) don't reliably run the
            /// MonoBehaviour lifecycle before a directly-invoked interface method needs this.
            /// </summary>
            private RectTransform Rect => _rect != null ? _rect : (_rect = (RectTransform)transform);

            /// <summary>
            /// Intentionally empty. The only state a cancelled gesture needs to unwind (position,
            /// rotation, sibling index) is captured in <see cref="OnBeginDrag"/>, which only fires
            /// once Unity's own drag threshold is exceeded — a plain press+release that never drags
            /// leaves nothing here to revert. Implemented anyway per the brief's interface list, and
            /// as a documented seam if a future pass wants an on-press cue.
            /// </summary>
            public void OnPointerDown(PointerEventData eventData)
            {
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                _dragging = Owner != null && Owner.IsInteractable(CardId);
                if (!_dragging)
                {
                    return;
                }

                _startAnchoredPosition = Rect.anchoredPosition;
                _startLocalRotation = Rect.localRotation;
                _startSiblingIndex = Rect.GetSiblingIndex();

                // Bring the picked-up card in front of its fanned neighbors and straighten it out of
                // its resting tilt — reads as "lifted out of the hand" while dragging.
                Rect.SetAsLastSibling();
                Rect.localRotation = Quaternion.identity;
                Owner.HandleCardDragBegin(CardId);
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (!_dragging)
                {
                    return;
                }

                var parent = Rect.parent as RectTransform;
                if (parent == null)
                {
                    return;
                }

                // Convert the screen-space pointer delta into the parent's local space rather than
                // applying eventData.delta directly to anchoredPosition — the hand lives under a
                // CanvasScaler, so 1 screen pixel is not 1 UI unit except at the reference resolution.
                Vector2 previousScreen = eventData.position - eventData.delta;
                bool haveCurrent = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out Vector2 current);
                bool havePrevious = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, previousScreen, eventData.pressEventCamera, out Vector2 previous);

                if (haveCurrent && havePrevious)
                {
                    Rect.anchoredPosition += current - previous;
                }
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (!_dragging)
                {
                    return;
                }

                _dragging = false;

                float distance = Vector2.Distance(eventData.pressPosition, eventData.position);
                bool pastThreshold = distance >= DragThresholdPixels;
                bool outsideHand = Owner?.Root != null
                    && !RectTransformUtility.RectangleContainsScreenPoint(Owner.Root, eventData.position, eventData.pressEventCamera);

                // Snap back unconditionally — even a committed play only clears the card via SetSpent
                // once TryQueueBandageAt/TryQueueStormAt actually lands (see ProgramHud); if it gets
                // rejected the card must already be back home, not stranded off-hand.
                Rect.anchoredPosition = _startAnchoredPosition;
                Rect.localRotation = _startLocalRotation;
                Rect.SetSiblingIndex(_startSiblingIndex);

                Owner?.HandleCardDragEnd(CardId, pastThreshold && outsideHand);
            }
        }
    }
}
