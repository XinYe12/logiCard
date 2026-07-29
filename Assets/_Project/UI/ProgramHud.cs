using System.Collections;
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
    /// Built in code so the demo has no prefab/scene merge conflicts this early.
    /// </summary>
    public sealed class ProgramHud : MonoBehaviour
    {
        public const float TopStripHeight = 0.10f;
        public const float ThumbZoneHeight = 0.40f;

        private static readonly Color Ink = new Color(0.93f, 0.92f, 0.88f, 1f);
        private static readonly Color PanelDark = new Color(0.10f, 0.10f, 0.12f, 1f);
        private static readonly Color PanelMid = new Color(0.17f, 0.17f, 0.20f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.72f, 0.25f, 1f);
        private static readonly Color AccentDim = new Color(0.35f, 0.30f, 0.20f, 1f);

        private TimeResourceClockDriver _clock;
        private RoundPhaseController _phase;

        private Text _phaseLabel;
        private Text _programTimerLabel;
        private Text _scrubLabel;
        private Slider _scrubber;
        private Button _playButton;
        private Text _playButtonLabel;
        private Font _font;
        private bool _suppressSliderCallback;

        public void Init(TimeResourceClockDriver clock, RoundPhaseController phase)
        {
            _clock = clock;
            _phase = phase;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureEventSystem();
            RectTransform root = BuildCanvas();
            BuildTopStrip(root);
            BuildThumbZone(root);

            _clock.TimeChanged += OnClockTime;
            _phase.PhaseChanged += OnPhaseChanged;

            OnPhaseChanged(_phase.Phase);
            OnClockTime(_clock.CurrentSeconds);
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

            _phaseLabel = CreateText(strip, "PhaseLabel", "PROGRAM", 44, TextAnchor.MiddleLeft, Accent);
            Stretch(_phaseLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0.6f, 1f), new Vector2(32f, 0f), Vector2.zero);

            _programTimerLabel = CreateText(strip, "ProgramTimer", "PROGRAM  30s", 32, TextAnchor.MiddleRight, Ink);
            Stretch(_programTimerLabel.rectTransform, new Vector2(0.6f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-32f, 0f));
        }

        private void BuildThumbZone(RectTransform root)
        {
            RectTransform zone = CreatePanel(root, "ThumbZone", PanelDark,
                new Vector2(0f, 0f), new Vector2(1f, ThumbZoneHeight));

            _scrubLabel = CreateText(zone, "ScrubLabel", "Time Resource  0.0s / 60.0s", 34, TextAnchor.MiddleLeft, Ink);
            Anchor(_scrubLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -70f), new Vector2(-32f, -20f));

            _scrubber = CreateSlider(zone, "Scrubber");
            Anchor(_scrubber.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(32f, -150f), new Vector2(-32f, -86f));
            _scrubber.onValueChanged.AddListener(OnScrubberMoved);

            // Phase debug row (Day 2 DoD: phases switch with debug buttons).
            CreatePhaseButton(zone, "Program", RoundPhase.Program, 0);
            CreatePhaseButton(zone, "Reveal", RoundPhase.Reveal, 1);
            CreatePhaseButton(zone, "Execute", RoundPhase.Execute, 2);

            _playButton = CreateButton(zone, "PlayButton", "Play", PanelMid, Ink, 34, OnPlayPressed);
            Anchor(_playButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(32f, 32f), new Vector2(272f, 140f));
            _playButtonLabel = _playButton.GetComponentInChildren<Text>();

            Button rewind = CreateButton(zone, "RewindButton", "Rewind", PanelMid, Ink, 34, () => _clock.Rewind());
            Anchor(rewind.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(288f, 32f), new Vector2(528f, 140f));

            Button lockIn = CreateButton(zone, "LockInButton", "LOCK IN", Accent, new Color(0.1f, 0.09f, 0.07f), 40, OnLockInPressed);
            Anchor(lockIn.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-392f, 32f), new Vector2(-32f, 140f));
        }

        private void CreatePhaseButton(RectTransform parent, string label, RoundPhase phase, int index)
        {
            Button b = CreateButton(parent, $"Phase_{label}", label, PanelMid, Ink, 30, () => SwitchPhase(phase));
            float x = 32f + (index * 248f);
            Anchor(b.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -256f), new Vector2(x + 232f, -172f));
        }

        // ---------- behaviour ----------

        private void SwitchPhase(RoundPhase phase)
        {
            _phase.GoTo(phase);

            switch (phase)
            {
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
            }
        }

        private void OnLockInPressed()
        {
            StopAllCoroutines();
            StartCoroutine(LockInRoutine());
        }

        private IEnumerator LockInRoutine()
        {
            SwitchPhase(RoundPhase.Reveal);
            yield return new WaitForSeconds(0.8f);
            SwitchPhase(RoundPhase.Execute);
        }

        private void OnPlayPressed()
        {
            if (_clock.IsPlaying)
            {
                _clock.Pause();
                return;
            }

            _phase.GoTo(RoundPhase.Execute);
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
            _phaseLabel.color = phase == RoundPhase.Execute ? Accent : Ink;
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
    }
}
