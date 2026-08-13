using System;
using System.Collections.Generic;
using LogiCard.Cards;
using UnityEngine;
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
    /// UI-only first-wave gear hand scaffold (C62). Builds a horizontal cardstock strip for
    /// Bandage / Interact / Flashbang / Adrenaline using the same <c>Modal*</c> paper tokens as
    /// <see cref="ModalDialog"/>. No Sim, schedule, or resolve logic — Integrator wires the dock
    /// and costs later when OPEN #16 closes.
    /// </summary>
    public sealed class GearHandView
    {
        /// <summary>Stable GameObject name prefix for PlayMode lookups (<c>Gear_Bandage</c>, …).</summary>
        public const string ButtonNamePrefix = "Gear_";

        /// <summary>Root panel name under the parent canvas/dock slot.</summary>
        public const string RootName = "GearHand";

        /// <summary>
        /// First-wave roster in dock order. Cost labels are explicit placeholders (em dash) so a
        /// future numeric pass cannot silently inherit invented values.
        /// </summary>
        public static readonly GearHandCardInfo[] FirstWave =
        {
            new GearHandCardInfo(CardId.Bandage, "Bandage", "TR —", "PROGRAM", oncePerMatch: false),
            new GearHandCardInfo(CardId.Interact, "Interact", "TR —", "PROGRAM", oncePerMatch: false),
            new GearHandCardInfo(CardId.Flashbang, "Flashbang", "TR —", "PROGRAM", oncePerMatch: true),
            new GearHandCardInfo(CardId.Adrenaline, "Adrenaline", "TR —", "PLAYBACK", oncePerMatch: true),
        };

        private readonly Dictionary<CardId, CardSlot> _slots = new Dictionary<CardId, CardSlot>();
        private readonly HashSet<CardId> _spent = new HashSet<CardId>();
        private GearHandPhase _phase = GearHandPhase.Program;
        private CardId? _armedId;

        public RectTransform Root { get; private set; }
        public GearHandPhase Phase => _phase;
        public CardId? ArmedId => _armedId;

        /// <summary>Raised when the player arms a legal card. Never schedules or resolves.</summary>
        public event Action<CardId> CardArmed;

        /// <summary>Raised when arm clears (re-click armed card, phase swap, or <see cref="ClearArm"/>).</summary>
        public event Action ArmCleared;

        private GearHandView()
        {
        }

        public static string ButtonName(CardId id) => ButtonNamePrefix + id;

        public Button GetButton(CardId id)
        {
            return _slots.TryGetValue(id, out CardSlot slot) ? slot.Button : null;
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
            for (int i = 0; i < count; i++)
            {
                GearHandCardInfo info = FirstWave[i];
                float x0 = i / (float)count;
                float x1 = (i + 1) / (float)count;
                const float inset = 0.02f;

                RectTransform cell = ui.CreatePanel(root, $"Slot_{info.Id}", new Color(0f, 0f, 0f, 0f),
                    new Vector2(x0 + inset, 0.06f), new Vector2(x1 - inset, 0.94f));

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

                var slot = new CardSlot(info, button, title, cost, hint, border.GetComponent<Image>());
                hand._slots[info.Id] = slot;

                CardId captured = info.Id;
                button.onClick.AddListener(() => hand.OnCardClicked(captured));
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

        private void RefreshPresentation()
        {
            foreach (KeyValuePair<CardId, CardSlot> pair in _slots)
            {
                CardSlot slot = pair.Value;
                bool spent = _spent.Contains(pair.Key);
                bool legal = IsInteractable(pair.Key);
                bool armed = _armedId == pair.Key;

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
                else if (armed)
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
                slot.PhaseHint.color = armed ? UiStyle.ModalPrimaryButtonText : UiStyle.ModalDivider;
            }
        }

        private sealed class CardSlot
        {
            public GearHandCardInfo Info { get; }
            public Button Button { get; }
            public Text Title { get; }
            public Text Cost { get; }
            public Text PhaseHint { get; }
            public Image Border { get; }

            public CardSlot(GearHandCardInfo info, Button button, Text title, Text cost, Text phaseHint, Image border)
            {
                Info = info;
                Button = button;
                Title = title;
                Cost = cost;
                PhaseHint = phaseHint;
                Border = border;
            }
        }
    }
}
