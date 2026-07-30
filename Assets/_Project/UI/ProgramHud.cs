using System;
using System.Collections;
using LogiCard.Board;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Portrait, one-handed HUD (C30). Three stacked bands: a thin read-only top strip,
    /// the board in the middle (rendered by the camera viewport, not by this canvas),
    /// and a thumb zone at the bottom holding every interactive control.
    /// Owns Allot (Time Card) and Aftermath panels for the match loop (C33).
    /// </summary>
    public sealed class ProgramHud : MonoBehaviour
    {
        public const float TopStripHeight = 0.10f;
        public const float ThumbZoneHeight = 0.40f;

        private const float Pad = 32f;
        private const float Gap = 16f;
        private const float RowGap = 24f;
        private const float VerbRowHeight = 116f;
        private const float DebugRowHeight = 72f;
        private const float ActionRowHeight = 132f;
        private const float TransportButtonWidth = 200f;

        private static readonly float[] TimeCardPresets = { 30f, 60f, 120f };

        private static readonly Color Ink = new Color(0.93f, 0.92f, 0.88f, 1f);
        private static readonly Color PanelDark = new Color(0.10f, 0.10f, 0.12f, 1f);
        private static readonly Color PanelMid = new Color(0.17f, 0.17f, 0.20f, 1f);
        private static readonly Color PanelSunken = new Color(0.06f, 0.06f, 0.08f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.72f, 0.25f, 1f);
        private static readonly Color AccentDim = new Color(0.35f, 0.30f, 0.20f, 1f);

        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;
        private BoardInputController _input;
        private MatchClock _matchClock;

        private Text _phaseLabel;
        private Text _programTimerLabel;
        private Text _matchLabel;
        private Text _scrubLabel;
        private Slider _scrubber;
        private Button _playButton;
        private Text _playButtonLabel;
        private Button _moveModeButton;
        private Button _shootModeButton;
        private Text _queueText;
        private Text _outcomeLabel;
        private GameObject _programControls;
        private GameObject _allotPanel;
        private GameObject _aftermathPanel;
        private Text _allotChooserLabel;
        private Text _allotSliderLabel;
        private Slider _allotSlider;
        private Text _aftermathLabel;
        private Button _nextRoundButton;
        private Text _nextRoundButtonLabel;
        private Font _font;
        private bool _suppressSliderCallback;
        private bool _showPhaseDebugControls;
        private float _pendingAllotment = 60f;
        private bool _awaitingAftermath;

        /// <summary>
        /// Raised once the payload is built and input is locked, so the composition root can run the
        /// resolve. The HUD deliberately knows nothing about the resolver.
        /// </summary>
        public event Action LockedIn;

        /// <summary>Raised when the chooser confirms a Time Card value (seconds).</summary>
        public event Action<float> TimeCardPlayed;

        /// <summary>Raised from Aftermath when the player wants another round (or Match Over).</summary>
        public event Action NextRoundRequested;

        public void Init(
            TimeResourceClockDriver clock,
            RoundPhaseController phase,
            BoardInputController input,
            MatchClock matchClock,
            bool showPhaseDebugControls = false)
        {
            _clock = clock;
            _phase = phase;
            _input = input;
            _matchClock = matchClock;
            _showPhaseDebugControls = showPhaseDebugControls;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureEventSystem();
            RectTransform root = BuildCanvas();
            BuildTopStrip(root);
            BuildThumbZone(root);
            BuildOutcomeBanner(root);

            _clock.TimeChanged += OnClockTime;
            _phase.PhaseChanged += OnPhaseChanged;
            _input.QueueChanged += OnQueueChanged;

            OnPhaseChanged(_phase.Phase);
            OnClockTime(_clock.CurrentSeconds);
            OnQueueChanged(_input.Program);
            RefreshMatchLabel();
        }

        private void OnDestroy()
        {
            if (_clock != null)
            {
                _clock.TimeChanged -= OnClockTime;
            }

            if (_phase != null)
            {
                _phase.PhaseChanged -= OnPhaseChanged;
            }

            if (_input != null)
            {
                _input.QueueChanged -= OnQueueChanged;
            }
        }

        private void Update()
        {
            if (_phase == null || _programTimerLabel == null)
            {
                return;
            }

            _programTimerLabel.text = _phase.Phase == RoundPhase.Program
                ? $"PROGRAM  {_phase.ProgramSecondsRemaining:0}s"
                : "real-world";

            if (_playButtonLabel != null)
            {
                _playButtonLabel.text = _clock.IsPlaying ? "Pause" : "Play";
            }

            // When Execute cinema finishes, advance to Aftermath so the round can cycle (C33).
            if (_awaitingAftermath
                && _phase.Phase == RoundPhase.Execute
                && !_clock.IsPlaying
                && _clock.CurrentSeconds + 0.001f >= _clock.BudgetSeconds)
            {
                _awaitingAftermath = false;
                SwitchPhase(RoundPhase.Aftermath);
            }
        }

        // ---------- construction ----------

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            go.transform.SetParent(null);
        }

        private RectTransform BuildCanvas()
        {
            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Safe area root so notches never eat the thumb zone on Android.
            var safe = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            safe.SetParent(canvasGo.transform, false);
            Rect area = Screen.safeArea;
            safe.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safe.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safe.offsetMin = Vector2.zero;
            safe.offsetMax = Vector2.zero;
            return safe;
        }

        private void BuildTopStrip(RectTransform root)
        {
            RectTransform strip = CreatePanel(root, "TopStrip", PanelDark,
                new Vector2(0f, 1f - TopStripHeight), new Vector2(1f, 1f));

            _phaseLabel = CreateText(strip, "PhaseLabel", "ALLOT", 40, TextAnchor.MiddleLeft, Accent);
            Stretch(_phaseLabel.rectTransform, new Vector2(0f, 0.45f), new Vector2(0.45f, 1f), new Vector2(32f, 0f), Vector2.zero);

            _matchLabel = CreateText(strip, "MatchLabel", "MATCH", 26, TextAnchor.MiddleLeft, Ink);
            Stretch(_matchLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0.7f, 0.55f), new Vector2(32f, 0f), Vector2.zero);

            _programTimerLabel = CreateText(strip, "ProgramTimer", "real-world", 28, TextAnchor.MiddleRight, Ink);
            Stretch(_programTimerLabel.rectTransform, new Vector2(0.55f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-32f, 0f));
        }

        private void BuildThumbZone(RectTransform root)
        {
            RectTransform zone = CreatePanel(root, "ThumbZone", PanelDark,
                new Vector2(0f, 0f), new Vector2(1f, ThumbZoneHeight));

            BuildAllotPanel(zone);
            BuildAftermathPanel(zone);
            BuildProgramControls(zone);
        }

        private void BuildProgramControls(RectTransform zone)
        {
            _programControls = new GameObject("ProgramControls", typeof(RectTransform));
            var rt = _programControls.GetComponent<RectTransform>();
            rt.SetParent(zone, false);
            Stretch(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float cursor = -Pad;

            _scrubLabel = CreateText(rt, "ScrubLabel", "Time Resource  0.0s / 0.0s", 34, TextAnchor.MiddleLeft, Ink);
            PlaceRow(_scrubLabel.rectTransform, ref cursor, 48f, 12f);

            _scrubber = CreateSlider(rt, "Scrubber");
            PlaceRow(_scrubber.GetComponent<RectTransform>(), ref cursor, 56f, RowGap);
            _scrubber.onValueChanged.AddListener(OnScrubberMoved);

            BuildVerbRow(rt, ref cursor);

            if (_showPhaseDebugControls)
            {
                BuildPhaseDebugRow(rt, ref cursor);
            }

            BuildActionRow(rt);
            BuildQueuePanel(rt, cursor);
        }

        private void BuildAllotPanel(RectTransform zone)
        {
            _allotPanel = new GameObject("AllotPanel", typeof(RectTransform));
            var rt = _allotPanel.GetComponent<RectTransform>();
            rt.SetParent(zone, false);
            Stretch(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float cursor = -Pad;
            _allotChooserLabel = CreateText(rt, "ChooserLabel", "ATTACKER PLAYS TIME CARD", 36, TextAnchor.MiddleLeft, Accent);
            PlaceRow(_allotChooserLabel.rectTransform, ref cursor, 56f, RowGap);

            for (int i = 0; i < TimeCardPresets.Length; i++)
            {
                float preset = TimeCardPresets[i];
                Button presetButton = CreateButton(rt, $"TimeCard_{preset:0}", $"{preset:0}s", PanelMid, Ink, 34,
                    () => ConfirmTimeCard(preset));
                PlaceSplitCell(presetButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, i, TimeCardPresets.Length + 1);
            }

            Button allIn = CreateButton(rt, "TimeCard_AllIn", "ALL IN", Accent, new Color(0.1f, 0.09f, 0.07f), 34,
                () => ConfirmTimeCard(_matchClock != null ? _matchClock.RemainingSeconds : 0f));
            PlaceSplitCell(allIn.GetComponent<RectTransform>(), cursor, VerbRowHeight, TimeCardPresets.Length, TimeCardPresets.Length + 1);
            cursor -= VerbRowHeight + RowGap;

            _allotSliderLabel = CreateText(rt, "AllotSliderLabel", "Custom  60s", 30, TextAnchor.MiddleLeft, Ink);
            PlaceRow(_allotSliderLabel.rectTransform, ref cursor, 44f, 8f);

            _allotSlider = CreateSlider(rt, "AllotSlider");
            PlaceRow(_allotSlider.GetComponent<RectTransform>(), ref cursor, 56f, RowGap);
            _allotSlider.onValueChanged.AddListener(OnAllotSliderMoved);

            Button confirm = CreateButton(rt, "ConfirmTimeCard", "PLAY TIME CARD", Accent, new Color(0.1f, 0.09f, 0.07f), 40,
                () => ConfirmTimeCard(_pendingAllotment));
            PlaceRow(confirm.GetComponent<RectTransform>(), ref cursor, ActionRowHeight, 0f);
        }

        private void BuildAftermathPanel(RectTransform zone)
        {
            _aftermathPanel = new GameObject("AftermathPanel", typeof(RectTransform));
            var rt = _aftermathPanel.GetComponent<RectTransform>();
            rt.SetParent(zone, false);
            Stretch(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float cursor = -Pad;
            _aftermathLabel = CreateText(rt, "AftermathLabel", "ROUND COMPLETE", 40, TextAnchor.MiddleCenter, Accent);
            PlaceRow(_aftermathLabel.rectTransform, ref cursor, 120f, RowGap);

            _nextRoundButton = CreateButton(rt, "NextRoundButton", "NEXT ROUND", Accent, new Color(0.1f, 0.09f, 0.07f), 40,
                () => NextRoundRequested?.Invoke());
            PlaceRow(_nextRoundButton.GetComponent<RectTransform>(), ref cursor, ActionRowHeight, 0f);
            _nextRoundButtonLabel = _nextRoundButton.GetComponentInChildren<Text>();
        }

        /// <summary>MOVE / SHOOT split the full width, so both stay large single-thumb targets (C30).</summary>
        private void BuildVerbRow(RectTransform zone, ref float cursor)
        {
            _moveModeButton = CreateButton(zone, "Mode_Move", "MOVE", PanelMid, Ink, 36, () => SetMode(ActionVerb.Move));
            PlaceSplitCell(_moveModeButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, 0, 2);

            _shootModeButton = CreateButton(zone, "Mode_Shoot", "SHOOT", PanelMid, Ink, 36, () => SetMode(ActionVerb.Shoot));
            PlaceSplitCell(_shootModeButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, 1, 2);

            cursor -= VerbRowHeight + RowGap;
            RefreshModeButtons();
        }

        /// <summary>Day 2 phase-jump aid. Off by default so the thumb zone shows only real player actions.</summary>
        private void BuildPhaseDebugRow(RectTransform zone, ref float cursor)
        {
            CreatePhaseButton(zone, "Allot", RoundPhase.Allot, 0, cursor, 4);
            CreatePhaseButton(zone, "Program", RoundPhase.Program, 1, cursor, 4);
            CreatePhaseButton(zone, "Reveal", RoundPhase.Reveal, 2, cursor, 4);
            CreatePhaseButton(zone, "Execute", RoundPhase.Execute, 3, cursor, 4);
            cursor -= DebugRowHeight + RowGap;
        }

        private void BuildActionRow(RectTransform zone)
        {
            _playButton = CreateButton(zone, "PlayButton", "Play", PanelMid, Ink, 34, OnPlayPressed);
            PlaceActionCell(_playButton.GetComponent<RectTransform>(), Pad, Pad + TransportButtonWidth);
            _playButtonLabel = _playButton.GetComponentInChildren<Text>();

            float rewindLeft = Pad + TransportButtonWidth + Gap;
            Button rewind = CreateButton(zone, "RewindButton", "Rewind", PanelMid, Ink, 34, () => _clock.Rewind());
            PlaceActionCell(rewind.GetComponent<RectTransform>(), rewindLeft, rewindLeft + TransportButtonWidth);

            // Lock In takes every remaining pixel of the row so the commit action reads as primary.
            Button lockIn = CreateButton(zone, "LockInButton", "LOCK IN", Accent, new Color(0.1f, 0.09f, 0.07f), 44, OnLockInPressed);
            RectTransform rt = lockIn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.offsetMin = new Vector2(rewindLeft + TransportButtonWidth + Gap, Pad);
            rt.offsetMax = new Vector2(-Pad, Pad + ActionRowHeight);
        }

        /// <summary>
        /// The queue readout fills whatever height is left between the rows above and the action row
        /// below, so it absorbs aspect-ratio differences instead of clipping.
        /// </summary>
        private void BuildQueuePanel(RectTransform zone, float topOffset)
        {
            RectTransform panel = CreatePanel(zone, "QueuePanel", PanelSunken, Vector2.zero, Vector2.one);
            panel.offsetMin = new Vector2(Pad, Pad + ActionRowHeight + RowGap);
            panel.offsetMax = new Vector2(-Pad, topOffset);

            _queueText = CreateText(panel, "QueueReadout", "Used 0.0 / 0.0s", 28, TextAnchor.UpperLeft, Ink);
            Stretch(_queueText.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            _queueText.lineSpacing = 1.3f;
        }

        /// <summary>
        /// Sits just above the thumb zone so wound text lands between the board and the controls,
        /// where the eye already is during playback. Stub text is all Slice 1 needs (D7).
        /// </summary>
        private void BuildOutcomeBanner(RectTransform root)
        {
            _outcomeLabel = CreateText(root, "OutcomeBanner", string.Empty, 40, TextAnchor.MiddleCenter, Accent);
            Anchor(_outcomeLabel.rectTransform,
                new Vector2(0f, ThumbZoneHeight), new Vector2(1f, ThumbZoneHeight),
                new Vector2(Pad, 16f), new Vector2(-Pad, 104f));
        }

        public void ShowOutcome(string text)
        {
            if (_outcomeLabel != null)
            {
                _outcomeLabel.text = text ?? string.Empty;
            }
        }

        private void CreatePhaseButton(RectTransform parent, string label, RoundPhase phase, int index, float cursor, int count)
        {
            Button b = CreateButton(parent, $"Phase_{label}", label, PanelMid, Ink, 24, () => SwitchPhase(phase));
            PlaceSplitCell(b.GetComponent<RectTransform>(), cursor, DebugRowHeight, index, count);
        }

        // ---------- behaviour ----------

        private void SwitchPhase(RoundPhase phase)
        {
            _phase.GoTo(phase);

            switch (phase)
            {
                case RoundPhase.Allot:
                    _clock.Rewind();
                    break;
                case RoundPhase.Program:
                    _clock.Rewind();
                    break;
                case RoundPhase.Reveal:
                    _clock.Pause();
                    _clock.SetSeconds(0f);
                    break;
                case RoundPhase.Execute:
                    _clock.Play();
                    break;
                case RoundPhase.Aftermath:
                    _clock.Pause();
                    break;
            }
        }

        private void SetMode(ActionVerb mode)
        {
            _input.Mode = mode;
            RefreshModeButtons();
        }

        private void RefreshModeButtons()
        {
            if (_moveModeButton == null || _input == null)
            {
                return;
            }

            _moveModeButton.GetComponent<Image>().color = _input.Mode == ActionVerb.Move ? Accent : PanelMid;
            _shootModeButton.GetComponent<Image>().color = _input.Mode == ActionVerb.Shoot ? Accent : PanelMid;
        }

        private void OnQueueChanged(PawnProgram program)
        {
            if (_queueText == null || program == null)
            {
                return;
            }

            string text = $"Used {program.UsedSeconds:0.0} / {program.BudgetSeconds:0.0}s";
            if (program.Nodes.Count == 0)
            {
                text += "\n\nTap a tile to schedule a Move or Shoot.";
            }

            for (int i = 0; i < program.Nodes.Count; i++)
            {
                ActionNode node = program.Nodes[i];
                text += $"\n{i + 1}: {node.Verb} -> {node.GridPosition} @{node.ExecuteTime:0.0}s";
            }

            _queueText.text = text;
        }

        private void OnLockInPressed()
        {
            if (_phase.Phase != RoundPhase.Program)
            {
                return;
            }

            const float budgetEpsilon = 0.001f;
            if (_input.Program.UsedSeconds > _input.Program.BudgetSeconds + budgetEpsilon)
            {
                Debug.LogWarning("[logiCard] Lock In blocked: program exceeds Time Resource budget.");
                return;
            }

            TimelinePayload payload = _input.Program.Build();
            Debug.Log($"[logiCard] TimelinePayload locked: {payload.Nodes.Count} action(s).");
            foreach (ActionNode node in payload.Nodes)
            {
                string modifier = node.Modifier != null ? node.Modifier.displayName : "none";
                Debug.Log($"[logiCard]   {node.Verb} @ {node.ExecuteTime:0.00}s -> {node.GridPosition} " +
                          $"stance={StanceMath.Label(node.Stance)} modifier={modifier}");
            }

            _input.CommitToPlayback();
            LockedIn?.Invoke();

            StopAllCoroutines();
            StartCoroutine(LockInRoutine());
        }

        private IEnumerator LockInRoutine()
        {
            SwitchPhase(RoundPhase.Reveal);
            yield return new WaitForSeconds(0.8f);
            _awaitingAftermath = true;
            SwitchPhase(RoundPhase.Execute);
        }

        private void ConfirmTimeCard(float seconds)
        {
            if (_phase.Phase != RoundPhase.Allot)
            {
                return;
            }

            TimeCardPlayed?.Invoke(seconds);
        }

        private void OnAllotSliderMoved(float normalized)
        {
            if (_matchClock == null)
            {
                return;
            }

            float min = _matchClock.MinRoundSeconds;
            float max = Mathf.Max(min, _matchClock.RemainingSeconds);
            _pendingAllotment = Mathf.Lerp(min, max, Mathf.Clamp01(normalized));
            if (_allotSliderLabel != null)
            {
                _allotSliderLabel.text = $"Custom  {_pendingAllotment:0}s";
            }
        }

        private void OnPlayPressed()
        {
            if (_clock.IsPlaying)
            {
                _clock.Pause();
                return;
            }

            _phase.GoTo(RoundPhase.Execute);
            _awaitingAftermath = true;
            _clock.Play();
        }

        private void OnScrubberMoved(float value)
        {
            if (_suppressSliderCallback)
            {
                return;
            }

            _clock.Pause();
            _clock.SetNormalized(value);
        }

        private void OnClockTime(float seconds)
        {
            _suppressSliderCallback = true;
            if (_scrubber != null)
            {
                _scrubber.value = _clock.Normalized;
            }

            _suppressSliderCallback = false;

            if (_scrubLabel != null)
            {
                _scrubLabel.text = $"Time Resource  {seconds:0.0}s / {_clock.BudgetSeconds:0.0}s";
            }
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (_phaseLabel == null)
            {
                return;
            }

            _phaseLabel.text = phase.ToString().ToUpperInvariant();
            _phaseLabel.color = phase == RoundPhase.Execute || phase == RoundPhase.Allot ? Accent : Ink;

            if (_programControls != null)
            {
                _programControls.SetActive(phase == RoundPhase.Program
                    || phase == RoundPhase.Reveal
                    || phase == RoundPhase.Execute);
            }

            if (_allotPanel != null)
            {
                _allotPanel.SetActive(phase == RoundPhase.Allot);
            }

            if (_aftermathPanel != null)
            {
                _aftermathPanel.SetActive(phase == RoundPhase.Aftermath || phase == RoundPhase.MatchOver);
            }

            if (phase == RoundPhase.Program)
            {
                ShowOutcome(string.Empty);
            }

            if (phase == RoundPhase.Allot)
            {
                RefreshAllotPanel();
            }

            if (phase == RoundPhase.Aftermath || phase == RoundPhase.MatchOver)
            {
                RefreshAftermathPanel(phase);
            }

            RefreshMatchLabel();
        }

        private void RefreshMatchLabel()
        {
            if (_matchLabel == null || _matchClock == null)
            {
                return;
            }

            int minutes = Mathf.FloorToInt(_matchClock.RemainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(_matchClock.RemainingSeconds % 60f);
            string chooser = _matchClock.CurrentChooser == MatchSide.Attacker ? "ATTACKER" : "DEFENDER";
            _matchLabel.text = $"MATCH {minutes}:{seconds:00} left · R{_matchClock.RoundIndex} · {chooser} PICKS";
        }

        private void RefreshAllotPanel()
        {
            if (_matchClock == null)
            {
                return;
            }

            string chooser = _matchClock.CurrentChooser == MatchSide.Attacker ? "ATTACKER" : "DEFENDER";
            if (_allotChooserLabel != null)
            {
                _allotChooserLabel.text = $"{chooser} PLAYS TIME CARD";
            }

            float min = _matchClock.MinRoundSeconds;
            float max = Mathf.Max(min, _matchClock.RemainingSeconds);
            _pendingAllotment = Mathf.Clamp(60f, min, max);
            if (_allotSlider != null)
            {
                _allotSlider.SetValueWithoutNotify(max <= min ? 1f : (_pendingAllotment - min) / (max - min));
            }

            if (_allotSliderLabel != null)
            {
                _allotSliderLabel.text = $"Custom  {_pendingAllotment:0}s";
            }
        }

        private void RefreshAftermathPanel(RoundPhase phase)
        {
            bool matchOver = phase == RoundPhase.MatchOver
                || (_matchClock != null && !_matchClock.CanFundAnotherRound);

            if (_aftermathLabel != null)
            {
                _aftermathLabel.text = matchOver
                    ? "MATCH OVER"
                    : $"ROUND {_matchClock.RoundIndex} COMPLETE\n{_matchClock.RemainingSeconds:0}s left in match";
            }

            if (_nextRoundButtonLabel != null)
            {
                _nextRoundButtonLabel.text = matchOver ? "MATCH OVER" : "NEXT ROUND";
            }

            if (_nextRoundButton != null)
            {
                _nextRoundButton.interactable = !matchOver || phase == RoundPhase.Aftermath;
                // Keep the button clickable in Aftermath even when match is over so MatchOver can fire.
                if (phase == RoundPhase.MatchOver)
                {
                    _nextRoundButton.interactable = false;
                }
            }
        }

        // ---------- tiny uGUI helpers ----------

        private static RectTransform CreatePanel(RectTransform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private Text CreateText(RectTransform parent, string name, string content, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button CreateButton(RectTransform parent, string name, string label, Color bg, Color fg, int size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;

            Text text = CreateText(rt, "Label", label, size, TextAnchor.MiddleCenter, fg);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            return button;
        }

        private Slider CreateSlider(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            RectTransform background = CreatePanel(rt, "Background", AccentDim, Vector2.zero, Vector2.one);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
            fillArea.SetParent(rt, false);
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(0f, 0f);
            fillArea.offsetMax = new Vector2(0f, 0f);

            RectTransform fill = CreatePanel(fillArea, "Fill", Accent, Vector2.zero, Vector2.one);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.SetParent(rt, false);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(20f, 0f);
            handleArea.offsetMax = new Vector2(-20f, 0f);

            RectTransform handle = CreatePanel(handleArea, "Handle", Ink, Vector2.zero, Vector2.one);
            handle.sizeDelta = new Vector2(40f, 0f);

            var slider = go.GetComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<Image>();
            _ = background;
            return slider;
        }

        private static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        // ---------- thumb-zone row layout ----------

        /// <summary>Places a full-width row at the cursor, then moves the cursor below it.</summary>
        private static void PlaceRow(RectTransform rt, ref float cursor, float height, float gapAfter)
        {
            Anchor(rt, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(Pad, cursor - height), new Vector2(-Pad, cursor));
            cursor -= height + gapAfter;
        }

        /// <summary>Places one cell of a row divided into <paramref name="count"/> equal columns.</summary>
        private static void PlaceSplitCell(RectTransform rt, float cursor, float height, int index, int count)
        {
            float half = Gap * 0.5f;
            rt.anchorMin = new Vector2(index / (float)count, 1f);
            rt.anchorMax = new Vector2((index + 1) / (float)count, 1f);
            rt.offsetMin = new Vector2(index == 0 ? Pad : half, cursor - height);
            rt.offsetMax = new Vector2(index == count - 1 ? -Pad : -half, cursor);
        }

        /// <summary>Places a fixed-width control on the bottom action row.</summary>
        private static void PlaceActionCell(RectTransform rt, float left, float right)
        {
            Anchor(rt, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(left, Pad), new Vector2(right, Pad + ActionRowHeight));
        }
    }
}
