using System;
using System.Collections;
using System.Collections.Generic;
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
        public const float ThumbZoneHeight = 0.44f;

        private const float Pad = 32f;
        private const float Gap = 16f;
        private const float RowGap = 24f;
        private const float VerbRowHeight = 116f;
        private const float StanceRowHeight = 100f;
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
        private static readonly Color MoveMarkerColor = new Color(0.55f, 0.75f, 0.95f, 1f);
        private static readonly Color ShootMarkerColor = new Color(0.95f, 0.35f, 0.30f, 1f);
        private static readonly Color DoorMarkerColor = new Color(0.55f, 0.85f, 0.55f, 1f);

        // Day 9 — cardstock Time Card (paper in the thumb zone) vs AR scrubber (cool contrast on clay).
        private static readonly Color CardstockPaper = new Color(0.93f, 0.88f, 0.78f, 1f);
        private static readonly Color CardstockPaperDeep = new Color(0.86f, 0.78f, 0.64f, 1f);
        private static readonly Color CardstockInk = new Color(0.22f, 0.16f, 0.12f, 1f);
        private static readonly Color CardstockShadow = new Color(0.05f, 0.04f, 0.03f, 0.45f);
        private static readonly Color CardstockConfirm = new Color(0.78f, 0.42f, 0.22f, 1f);
        private static readonly Color ArTrack = new Color(0.08f, 0.14f, 0.22f, 1f);
        private static readonly Color ArFill = new Color(0.35f, 0.85f, 0.95f, 1f);
        private static readonly Color ArHandle = new Color(0.95f, 0.98f, 1f, 1f);

        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;
        private BoardInputController _input;
        private MatchClock _matchClock;

        private Text _phaseLabel;
        private Text _programTimerLabel;
        private Text _matchLabel;
        private Text _scrubLabel;
        private Slider _scrubber;
        private readonly List<GameObject> _scrubberMarkers = new List<GameObject>();
        private Button _playButton;
        private Text _playButtonLabel;
        private Button _moveModeButton;
        private Button _shootModeButton;
        private Button _doorModeButton;
        private Button _sprintButton;
        private Button _walkButton;
        private Button _crawlButton;
        private Button _undoWaypointButton;
        private Button _setPathButton;
        private Button _snapButton;
        private Button _holdButton;
        private Button _openDoorButton;
        private Button _closeDoorButton;
        private GameObject _moveStanceControls;
        private GameObject _shootModeControls;
        private GameObject _doorModeControls;
        private Text _stanceLabel;
        private Text _shootModeLabel;
        private Text _doorModeLabel;
        private RectTransform _canvasRoot;
        private GameObject _doorPromptRoot;
        private RectTransform _doorPromptRect;
        private Text _doorPromptLabel;
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
            _canvasRoot = root;
            BuildTopStrip(root);
            BuildThumbZone(root);
            BuildOutcomeBanner(root);
            BuildDoorPrompt(root);

            _clock.TimeChanged += OnClockTime;
            _phase.PhaseChanged += OnPhaseChanged;
            _input.QueueChanged += OnQueueChanged;
            _input.ActionRejected += OnActionRejected;

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
                _input.ActionRejected -= OnActionRejected;
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

            // AR scrubber: cool cyan/white on a dark track — deliberate contrast vs clay board (ART_DIRECTION §4).
            _scrubLabel = CreateText(rt, "ScrubLabel", "Time Resource  0.0s / 0.0s", 34, TextAnchor.MiddleLeft, ArFill);
            PlaceRow(_scrubLabel.rectTransform, ref cursor, 48f, 12f);

            _scrubber = CreateSlider(rt, "Scrubber", ArTrack, ArFill, ArHandle);
            PlaceRow(_scrubber.GetComponent<RectTransform>(), ref cursor, 56f, RowGap);
            _scrubber.onValueChanged.AddListener(OnScrubberMoved);

            BuildVerbRow(rt, ref cursor);
            BuildStanceRow(rt, ref cursor);

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

            // Soft cardstock shadow + paper face (Day 9 Demo art floor — physical Time Card in thumb zone).
            RectTransform shadow = CreatePanel(rt, "TimeCardShadow", CardstockShadow, Vector2.zero, Vector2.one);
            shadow.offsetMin = new Vector2(Pad + 10f, Pad);
            shadow.offsetMax = new Vector2(-Pad + 10f, -Pad - 10f);

            RectTransform card = CreatePanel(rt, "TimeCardFace", CardstockPaper, Vector2.zero, Vector2.one);
            card.offsetMin = new Vector2(Pad, Pad + 12f);
            card.offsetMax = new Vector2(-Pad, -Pad);

            float cursor = -Pad;
            _allotChooserLabel = CreateText(card, "ChooserLabel", "ATTACKER PLAYS TIME CARD", 36, TextAnchor.MiddleLeft, CardstockInk);
            PlaceRow(_allotChooserLabel.rectTransform, ref cursor, 56f, RowGap);

            for (int i = 0; i < TimeCardPresets.Length; i++)
            {
                float preset = TimeCardPresets[i];
                Button presetButton = CreateButton(card, $"TimeCard_{preset:0}", $"{preset:0}s", CardstockPaperDeep, CardstockInk, 34,
                    () => ConfirmTimeCard(preset));
                PlaceSplitCell(presetButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, i, TimeCardPresets.Length + 1);
            }

            Button allIn = CreateButton(card, "TimeCard_AllIn", "ALL IN", CardstockConfirm, CardstockPaper, 34,
                () => ConfirmTimeCard(_matchClock != null ? _matchClock.RemainingSeconds : 0f));
            PlaceSplitCell(allIn.GetComponent<RectTransform>(), cursor, VerbRowHeight, TimeCardPresets.Length, TimeCardPresets.Length + 1);
            cursor -= VerbRowHeight + RowGap;

            _allotSliderLabel = CreateText(card, "AllotSliderLabel", "Custom  60s", 30, TextAnchor.MiddleLeft, CardstockInk);
            PlaceRow(_allotSliderLabel.rectTransform, ref cursor, 44f, 8f);

            _allotSlider = CreateSlider(card, "AllotSlider", CardstockPaperDeep, CardstockConfirm, CardstockInk);
            PlaceRow(_allotSlider.GetComponent<RectTransform>(), ref cursor, 56f, RowGap);
            _allotSlider.onValueChanged.AddListener(OnAllotSliderMoved);

            Button confirm = CreateButton(card, "ConfirmTimeCard", "PLAY TIME CARD", CardstockConfirm, CardstockPaper, 40,
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

        /// <summary>MOVE / SHOOT / DOOR split the full width, so all three stay large single-thumb targets (C30).</summary>
        private void BuildVerbRow(RectTransform zone, ref float cursor)
        {
            _moveModeButton = CreateButton(zone, "Mode_Move", "MOVE", PanelMid, Ink, 36, () => SetMode(ActionVerb.Move));
            PlaceSplitCell(_moveModeButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, 0, 3);

            _shootModeButton = CreateButton(zone, "Mode_Shoot", "SHOOT", PanelMid, Ink, 36, () => SetMode(ActionVerb.Shoot));
            PlaceSplitCell(_shootModeButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, 1, 3);

            _doorModeButton = CreateButton(zone, "Mode_Door", "DOOR", PanelMid, Ink, 36, () => SetMode(ActionVerb.Door));
            PlaceSplitCell(_doorModeButton.GetComponent<RectTransform>(), cursor, VerbRowHeight, 2, 3);

            cursor -= VerbRowHeight + RowGap;
            RefreshModeButtons();
        }

        /// <summary>
        /// Move shows a direct Sprint/Walk/Crawl pick; Shoot shows Snap / Hold Angle. Both are
        /// direct choices with automatic cost — no time-allotment slider for either (C21, amended
        /// 2026-08-03; C25).
        /// </summary>
        private void BuildStanceRow(RectTransform zone, ref float cursor)
        {
            float rowTop = cursor;

            _moveStanceControls = new GameObject("MoveStanceControls", typeof(RectTransform));
            var moveRt = _moveStanceControls.GetComponent<RectTransform>();
            moveRt.SetParent(zone, false);
            Stretch(moveRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float moveCursor = rowTop;
            _stanceLabel = CreateText(moveRt, "StanceLabel", "STANCE  Walk", 28, TextAnchor.MiddleLeft, Ink);
            PlaceRow(_stanceLabel.rectTransform, ref moveCursor, 40f, 8f);

            _sprintButton = CreateButton(moveRt, "Stance_Sprint", "SPRINT", PanelMid, Ink, 28,
                () => SetStanceBand(StanceType.Sprint));
            PlaceSplitCell(_sprintButton.GetComponent<RectTransform>(), moveCursor, StanceRowHeight, 0, 5);

            _walkButton = CreateButton(moveRt, "Stance_Walk", "WALK", PanelMid, Ink, 28,
                () => SetStanceBand(StanceType.Walk));
            PlaceSplitCell(_walkButton.GetComponent<RectTransform>(), moveCursor, StanceRowHeight, 1, 5);

            _crawlButton = CreateButton(moveRt, "Stance_Crawl", "CRAWL", PanelMid, Ink, 28,
                () => SetStanceBand(StanceType.Crawl));
            PlaceSplitCell(_crawlButton.GetComponent<RectTransform>(), moveCursor, StanceRowHeight, 2, 4);

            _setPathButton = CreateButton(moveRt, "SetPathButton", "SET PATH", Accent, new Color(0.1f, 0.09f, 0.07f), 28,
                () => _input.TryCommitDraftPath());
            PlaceSplitCell(_setPathButton.GetComponent<RectTransform>(), moveCursor, StanceRowHeight, 3, 4);

            _shootModeControls = new GameObject("ShootModeControls", typeof(RectTransform));
            var shootRt = _shootModeControls.GetComponent<RectTransform>();
            shootRt.SetParent(zone, false);
            Stretch(shootRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float shootCursor = rowTop;
            _shootModeLabel = CreateText(shootRt, "ShootModeLabel", "SHOOT  Snap · 2s — free-aim (tap anywhere on the board)", 28, TextAnchor.MiddleLeft, Ink);
            PlaceRow(_shootModeLabel.rectTransform, ref shootCursor, 40f, 8f);

            _snapButton = CreateButton(shootRt, "Shoot_Snap", "SNAP  2s", PanelMid, Ink, 32,
                () => SetShootMode(ShootMode.SnapShot));
            PlaceSplitCell(_snapButton.GetComponent<RectTransform>(), shootCursor, StanceRowHeight, 0, 2);

            _holdButton = CreateButton(shootRt, "Shoot_Hold", "HOLD  3s", PanelMid, Ink, 32,
                () => SetShootMode(ShootMode.HoldAngle));
            PlaceSplitCell(_holdButton.GetComponent<RectTransform>(), shootCursor, StanceRowHeight, 1, 2);

            _doorModeControls = new GameObject("DoorModeControls", typeof(RectTransform));
            var doorRt = _doorModeControls.GetComponent<RectTransform>();
            doorRt.SetParent(zone, false);
            Stretch(doorRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float doorCursor = rowTop;
            // The actual OPEN/CLOSE controls now spawn as a floating prompt anchored right at the
            // selected door (BuildDoorPrompt/RefreshDoorPrompt) — playtest feedback 2026-08-06
            // wanted "a UI component you interact with to open/close the door" at the point of
            // interaction, not a fixed row down in the thumb zone disconnected from the door itself.
            // This label just orients the player before they've tapped one.
            _doorModeLabel = CreateText(doorRt, "DoorModeLabel", "DOOR — tap near a door to select it", 28, TextAnchor.MiddleLeft, Ink);
            PlaceRow(_doorModeLabel.rectTransform, ref doorCursor, 40f, 8f);

            // Move, Shoot and Door controls occupy the same zone rect (toggled via SetActive), all
            // the same height with no slider on any side, so a single formula covers all three.
            cursor = rowTop - 40f - 8f - StanceRowHeight - RowGap;
            RefreshVerbContextControls(_input != null ? _input.Program : null);
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

            // UNDO must stay mode-agnostic (playtest 2026-08-07): it used to live only on the Move
            // stance row, so Door/Shoot mode hid it — players hit Rewind instead and thought undo
            // "didn't clear the queue." Same name as before so existing lookups keep working.
            float undoLeft = rewindLeft + TransportButtonWidth + Gap;
            _undoWaypointButton = CreateButton(zone, "UndoWaypointButton", "UNDO", PanelMid, Ink, 34, OnUndoPressed);
            PlaceActionCell(_undoWaypointButton.GetComponent<RectTransform>(), undoLeft, undoLeft + TransportButtonWidth);

            // Lock In takes every remaining pixel of the row so the commit action reads as primary.
            Button lockIn = CreateButton(zone, "LockInButton", "LOCK IN", Accent, new Color(0.1f, 0.09f, 0.07f), 44, OnLockInPressed);
            RectTransform rt = lockIn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.offsetMin = new Vector2(undoLeft + TransportButtonWidth + Gap, Pad);
            rt.offsetMax = new Vector2(-Pad, Pad + ActionRowHeight);
        }

        private void OnUndoPressed()
        {
            if (_input == null)
            {
                return;
            }

            _input.TryUndoLastStep();
            RefreshUndoButton(_input.Program);
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

        /// <summary>
        /// Board-anchored OPEN/CLOSE button cluster (see <see cref="RefreshDoorPrompt"/> and
        /// <c>docs/UI_BOARD_ANCHORED_COMPONENTS.md</c> for the general pattern this follows) — built
        /// once, hidden until a door is selected, then repositioned beside it every time the
        /// selection changes. Anchored to the canvas's own center reference point (`(0.5,0.5)`,
        /// matching what <see cref="RectTransformUtility.ScreenPointToLocalPointInRectangle"/>
        /// measures from) with its OWN pivot at its left-center edge, so it extends to the right of
        /// — "beside" — wherever it's anchored, without covering the anchor point itself.
        /// </summary>
        private void BuildDoorPrompt(RectTransform root)
        {
            var promptGo = new GameObject("DoorPrompt", typeof(RectTransform), typeof(Image));
            _doorPromptRoot = promptGo;
            _doorPromptRect = promptGo.GetComponent<RectTransform>();
            _doorPromptRect.SetParent(root, false);
            _doorPromptRect.anchorMin = new Vector2(0.5f, 0.5f);
            _doorPromptRect.anchorMax = new Vector2(0.5f, 0.5f);
            _doorPromptRect.pivot = new Vector2(0f, 0.5f);
            _doorPromptRect.sizeDelta = new Vector2(120f, 116f);
            promptGo.GetComponent<Image>().color = PanelDark;

            // Identity + state leg of the content contract (docs/UI_BOARD_ANCHORED_COMPONENTS.md) —
            // every interaction prompt says *what* it's acting on and its *current* state, not just
            // the available actions. Pinned to the top 24px.
            _doorPromptLabel = CreateText(_doorPromptRect, "DoorPromptLabel", string.Empty, 13, TextAnchor.MiddleCenter, Ink);
            Anchor(_doorPromptLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(6f, -26f), new Vector2(-6f, -2f));

            // Two equal-height (42px) action buttons stacked below the label, all anchored to the
            // bottom so the pixel math is absolute and doesn't depend on the parent's own pivot.
            _openDoorButton = CreateButton(_doorPromptRect, "Door_Open", "OPEN", PanelMid, Ink, 22,
                () => ConfirmDoor(DoorAction.Open));
            Anchor(_openDoorButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(6f, 48f), new Vector2(-6f, 90f));

            _closeDoorButton = CreateButton(_doorPromptRect, "Door_Close", "CLOSE", PanelMid, Ink, 22,
                () => ConfirmDoor(DoorAction.Close));
            Anchor(_closeDoorButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(6f, 4f), new Vector2(-6f, 46f));

            promptGo.SetActive(false);
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
            if (mode != ActionVerb.Move)
            {
                _input.TryCommitDraftPath();
            }

            if (mode != ActionVerb.Door)
            {
                _input.CancelPendingDoor();
            }

            _input.Mode = mode;
            RefreshModeButtons();
            RefreshVerbContextControls(_input.Program);
        }

        private void SetStanceBand(StanceType stance)
        {
            _input.TrySetDraftStance(stance);
            RefreshVerbContextControls(_input.Program);
        }

        private void SetShootMode(ShootMode mode)
        {
            _input.PreferredShootMode = mode;
            RefreshVerbContextControls(_input.Program);
        }

        private void ConfirmDoor(DoorAction action)
        {
            if (!_input.TryConfirmPendingDoor(action, out string reason))
            {
                Debug.Log($"[logiCard] Door confirm rejected: {reason}");
            }

            RefreshVerbContextControls(_input.Program);
        }

        private void RefreshModeButtons()
        {
            if (_moveModeButton == null || _input == null)
            {
                return;
            }

            _moveModeButton.GetComponent<Image>().color = _input.Mode == ActionVerb.Move ? Accent : PanelMid;
            _shootModeButton.GetComponent<Image>().color = _input.Mode == ActionVerb.Shoot ? Accent : PanelMid;
            _doorModeButton.GetComponent<Image>().color = _input.Mode == ActionVerb.Door ? Accent : PanelMid;
        }

        private void RefreshVerbContextControls(PawnProgram program)
        {
            ActionVerb mode = _input != null ? _input.Mode : ActionVerb.Move;
            if (_moveStanceControls != null)
            {
                _moveStanceControls.SetActive(mode == ActionVerb.Move);
            }

            if (_shootModeControls != null)
            {
                _shootModeControls.SetActive(mode == ActionVerb.Shoot);
            }

            if (_doorModeControls != null)
            {
                _doorModeControls.SetActive(mode == ActionVerb.Door);
            }

            // The floating door prompt only makes sense in Door mode — hide it unconditionally here
            // so leaving Door mode always clears it, regardless of which refresh path got here
            // (RefreshDoorModeControls only re-shows it when there's still something pending).
            if (mode != ActionVerb.Door && _doorPromptRoot != null)
            {
                _doorPromptRoot.SetActive(false);
            }

            if (mode == ActionVerb.Shoot)
            {
                RefreshShootModeControls(program);
            }
            else if (mode == ActionVerb.Door)
            {
                RefreshDoorModeControls(program);
            }
            else
            {
                RefreshStanceControls(program);
            }
        }

        private void RefreshShootModeControls(PawnProgram program)
        {
            if (_shootModeLabel == null || _input == null)
            {
                return;
            }

            ShootMode mode = _input.PreferredShootMode;
            float cost = ShootCost.SecondsFor(mode);
            _shootModeLabel.text = mode == ShootMode.HoldAngle
                ? $"HOLD ANGLE  {cost:0}s — covers the aim lane; lethal; hits Sprint"
                : $"SNAP SHOT  {cost:0}s — aimed point only; wounds; misses Sprint";

            if (_snapButton != null)
            {
                _snapButton.GetComponent<Image>().color = mode == ShootMode.SnapShot ? Accent : PanelMid;
                _holdButton.GetComponent<Image>().color = mode == ShootMode.HoldAngle ? Accent : PanelMid;
            }
        }

        private void RefreshDoorModeControls(PawnProgram program)
        {
            if (_doorModeLabel == null || _input == null)
            {
                return;
            }

            float cost = program != null ? program.DoorInteractSeconds : 4f;
            Door pending = _input.PendingDoor;

            // Scheduled state (live ⊕ this program's Door toggles) — not raw GetDoorState alone.
            // Playtest 2026-08-07: live-only read stayed CLOSED after OPEN, so the prompt looked dead.
            string stateLabel = null;
            if (pending != null && program != null)
            {
                stateLabel = program.ScheduledDoorState(pending) == DoorState.Open ? "OPEN" : "CLOSED";
            }

            // Identity leg of the content contract (docs/UI_BOARD_ANCHORED_COMPONENTS.md) — falls
            // back to a generic label for any Door registered without a DisplayName.
            string targetName = pending?.DisplayName ?? "DOOR";

            _doorModeLabel.text = stateLabel != null
                ? $"{targetName} selected · {stateLabel} — board prompt books Open/Close for this round"
                : "DOOR — tap near a door to select it";

            RefreshDoorPrompt(pending, targetName, stateLabel, cost);
        }

        /// <summary>
        /// Board-anchored OPEN/CLOSE button cluster spawned beside the selected door — playtest
        /// feedback 2026-08-06 wanted an actual interactive UI element at the door, not an error
        /// message or a fixed row in the thumb zone. Projects the door's board-space midpoint
        /// through the game camera into screen space, then into this screen-space-overlay canvas's
        /// local space (see <c>docs/UI_BOARD_ANCHORED_COMPONENTS.md</c> for the general recipe and
        /// the anchor/pivot pitfall this once fell into). The camera and board never move, so this
        /// only needs to run when the selection changes, not every frame.
        /// </summary>
        private void RefreshDoorPrompt(Door pending, string targetName, string stateLabel, float cost)
        {
            if (_doorPromptRoot == null)
            {
                return;
            }

            if (pending == null || _input.BoardView == null || Camera.main == null)
            {
                _doorPromptRoot.SetActive(false);
                return;
            }

            PlanarPosition doorMid = PlanarPosition.Lerp(pending.Segment.A, pending.Segment.B, 0.5f);
            Vector3 worldPoint = _input.BoardView.WorldFromPlanar(doorMid);
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPoint);

            if (screenPoint.z <= 0f)
            {
                // Behind the camera — shouldn't happen on this fixed top-down rig, but don't show a
                // prompt pinned to a point that isn't actually on screen.
                _doorPromptRoot.SetActive(false);
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRoot, screenPoint, null, out Vector2 local);
            // Small rightward gap so the cluster sits beside the door instead of starting exactly on
            // top of its (and the pawn's) own rendered geometry.
            _doorPromptRect.anchoredPosition = local + new Vector2(18f, 0f);

            if (_doorPromptLabel != null)
            {
                _doorPromptLabel.text = $"{targetName} · {stateLabel}";
            }

            Text openLabel = _openDoorButton.GetComponentInChildren<Text>();
            Text closeLabel = _closeDoorButton.GetComponentInChildren<Text>();
            if (openLabel != null)
            {
                openLabel.text = $"OPEN\n{cost:0}s";
            }

            if (closeLabel != null)
            {
                closeLabel.text = $"CLOSE\n{cost:0}s";
            }

            // Highlight the action that would change live state (not the one that matches it) —
            // dimming "current" used to make CLOSE look selected/disabled while CLOSED (playtest).
            bool closed = stateLabel == "CLOSED";
            _openDoorButton.GetComponent<Image>().color = closed ? Accent : PanelMid;
            _closeDoorButton.GetComponent<Image>().color = closed ? PanelMid : Accent;
            _doorPromptRoot.SetActive(true);
        }

        private void RefreshStanceControls(PawnProgram program)
        {
            if (_stanceLabel == null || program == null)
            {
                return;
            }

            StanceType stance = program.HasDraft ? program.DraftStance : (_input != null ? _input.PreferredStance : StanceType.Walk);
            float distance = program.HasDraft ? program.DraftDistance : 1f;
            float cost = StanceAllotment.CostForTiles(distance, program.BaseSecondsPerTile, stance);

            _stanceLabel.text = program.HasDraft
                ? $"PATH {program.DraftDistance:0.0}m · {StanceMath.Label(stance)} · {cost:0.0}s — SET PATH to book"
                : $"STANCE  {StanceMath.Label(stance)} · {cost:0.0}s / m — tap the board to build a path";

            if (_sprintButton != null)
            {
                _sprintButton.GetComponent<Image>().color = stance == StanceType.Sprint ? Accent : PanelMid;
                _walkButton.GetComponent<Image>().color = stance == StanceType.Walk ? Accent : PanelMid;
                _crawlButton.GetComponent<Image>().color = stance == StanceType.Crawl ? Accent : PanelMid;
            }

            if (_setPathButton != null)
            {
                _setPathButton.interactable = program.HasDraft;
            }

            RefreshUndoButton(program);
        }

        private void RefreshUndoButton(PawnProgram program)
        {
            if (_undoWaypointButton == null)
            {
                return;
            }

            _undoWaypointButton.interactable = program != null && program.CanUndoLastStep;
        }

        /// <summary>
        /// A successful queue change means whatever the player was doing worked, so any leftover
        /// rejection text from a moment ago is stale — clear it rather than leaving it stuck on
        /// screen through the next few successful taps.
        /// </summary>
        private void OnActionRejected(string reason)
        {
            ShowOutcome(string.IsNullOrEmpty(reason) ? string.Empty : $"Can't do that — {reason}");
        }

        private void OnQueueChanged(PawnProgram program)
        {
            ShowOutcome(string.Empty);

            if (_queueText == null || program == null)
            {
                return;
            }

            string text = $"Used {program.UsedSeconds:0.0} / {program.BudgetSeconds:0.0}s";
            if (program.HasDraft)
            {
                float total = program.UsedSeconds + program.DraftAllottedSeconds;
                text +=
                    $"\nDRAFT {program.DraftDistance:0.0}m · {StanceMath.Label(program.DraftStance)} · " +
                    $"{program.DraftAllottedSeconds:0.0}s → total {total:0.0}s" +
                    (total > program.BudgetSeconds + 0.001f ? " (over budget — UNDO draft or Lock In drops it)" : "");
            }

            if (program.Nodes.Count == 0 && !program.HasDraft)
            {
                text += "\n\nTap the board to draw a path, pick stance, SET PATH. Or Shoot (free-aim).";
            }

            for (int i = 0; i < program.Nodes.Count; i++)
            {
                ActionNode node = program.Nodes[i];
                string detail;
                if (node.Verb == ActionVerb.Shoot)
                {
                    detail = ShootModeMath.Label(node.ShootMode);
                }
                else if (node.Verb == ActionVerb.Door)
                {
                    // Was falling through to StanceMath.Label(node.Stance) — a leftover/irrelevant
                    // value on a Door node — printing something like "(Walk)" for a door toggle.
                    detail = node.Door == DoorAction.Close ? "Close" : "Open";
                }
                else
                {
                    detail = StanceMath.Label(node.Stance);
                }

                text += $"\n{i + 1}: {node.Verb} -> {node.Position} @{node.ExecuteTime:0.0}s ({detail})";
            }

            _queueText.text = text;
            RefreshVerbContextControls(program);
            RefreshUndoButton(program);
            RefreshScrubberMarkers(program);
        }

        /// <summary>
        /// Event markers on the Time Resource scrubber (PRODUCT_MEMORY.md C24 / VISION.md — "observer
        /// reads cause/effect on the timeline scrubber"): one tick per booked operation, positioned at
        /// its ExecuteTime proportion of the round budget, color-coded by verb. Previously only the
        /// Queue text panel below listed operations — the scrubber itself was a bare fill bar.
        /// </summary>
        private void RefreshScrubberMarkers(PawnProgram program)
        {
            for (int i = 0; i < _scrubberMarkers.Count; i++)
            {
                if (_scrubberMarkers[i] != null)
                {
                    Destroy(_scrubberMarkers[i]);
                }
            }

            _scrubberMarkers.Clear();

            if (_scrubber == null || program == null || program.BudgetSeconds <= 0f)
            {
                return;
            }

            RectTransform track = _scrubber.GetComponent<RectTransform>();
            for (int i = 0; i < program.Nodes.Count; i++)
            {
                ActionNode node = program.Nodes[i];
                float t = Mathf.Clamp01(node.ExecuteTime / program.BudgetSeconds);
                Color color = node.Verb == ActionVerb.Shoot ? ShootMarkerColor
                    : node.Verb == ActionVerb.Door ? DoorMarkerColor
                    : MoveMarkerColor;

                var markerGo = new GameObject($"ScrubberMark_{i}_{node.Verb}", typeof(RectTransform), typeof(Image));
                var rt = markerGo.GetComponent<RectTransform>();
                rt.SetParent(track, false);
                rt.anchorMin = new Vector2(t, 0f);
                rt.anchorMax = new Vector2(t, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(-3f, -8f);
                rt.offsetMax = new Vector2(3f, 8f);

                var image = markerGo.GetComponent<Image>();
                image.color = color;
                image.raycastTarget = false;

                _scrubberMarkers.Add(markerGo);
            }
        }

        private void OnLockInPressed()
        {
            if (_phase.Phase != RoundPhase.Program)
            {
                return;
            }

            // BUG FOUND 2026-08-06 (playtest): this used to only check *already-committed*
            // UsedSeconds, never the pending draft's cost — a drafted-but-not-yet-"SET PATH"ed
            // move that didn't fit the budget sailed past this check, then got silently discarded
            // by CommitToPlayback while Lock In proceeded anyway. CommitToPlayback is now the
            // source of truth: it either commits a fitting draft, or drops an over-budget draft
            // and locks the committed queue (playtest 2026-08-07).
            if (!_input.CommitToPlayback())
            {
                return;
            }

            TimelinePayload payload = _input.Program.Build();
            Debug.Log($"[logiCard] TimelinePayload locked: {payload.Nodes.Count} action(s).");
            foreach (ActionNode node in payload.Nodes)
            {
                string modifier = node.Modifier != null ? node.Modifier.displayName : "none";
                Debug.Log($"[logiCard]   {node.Verb} @ {node.ExecuteTime:0.00}s -> {node.Position} " +
                          $"stance={StanceMath.Label(node.Stance)} shoot={ShootModeMath.Label(node.ShootMode)} " +
                          $"modifier={modifier}");
            }

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

            // Unlike the rest of _programControls, the floating door prompt only makes sense while
            // actively drafting — leaving it up through Reveal/Execute would float a stale, locked
            // OPEN/CLOSE prompt over the playback animation.
            if (_doorPromptRoot != null && phase != RoundPhase.Program)
            {
                _doorPromptRoot.SetActive(false);
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

        private Slider CreateSlider(RectTransform parent, string name, Color track, Color fillColor, Color handleColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            RectTransform background = CreatePanel(rt, "Background", track, Vector2.zero, Vector2.one);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
            fillArea.SetParent(rt, false);
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(0f, 0f);
            fillArea.offsetMax = new Vector2(0f, 0f);

            RectTransform fill = CreatePanel(fillArea, "Fill", fillColor, Vector2.zero, Vector2.one);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.SetParent(rt, false);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(20f, 0f);
            handleArea.offsetMax = new Vector2(-20f, 0f);

            RectTransform handle = CreatePanel(handleArea, "Handle", handleColor, Vector2.zero, Vector2.one);
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
