using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Minimal click-through shell for <c>docs/UI_FLOW.md</c>'s screen map (C48).
    /// Functional only — Phase 5 owns polish. Local/offline play is always available;
    /// Find Match is a short stub delay (real matchmaking is Phase 2).
    /// </summary>
    public sealed class AppFlowController : MonoBehaviour
    {
        public enum Screen
        {
            None,
            Boot,
            CharacterSelect,
            Lobby,
            Waiting,
            Reveal,
            RoundResult,
            MatchEnd,
        }

        private const float FindMatchStubSeconds = 1.25f;
        private const float RevealFlashSeconds = 1.2f;
        private const float WaitingStubSeconds = 0.6f;

        private static readonly Color Ink = new Color(0.93f, 0.92f, 0.88f, 1f);
        private static readonly Color PanelDark = new Color(0.10f, 0.10f, 0.12f, 0.96f);
        private static readonly Color PanelMid = new Color(0.17f, 0.17f, 0.20f, 1f);
        private static readonly Color Accent = new Color(0.98f, 0.72f, 0.25f, 1f);

        private Font _font;
        private RectTransform _root;
        private GameObject _boot;
        private GameObject _characterSelect;
        private GameObject _lobby;
        private GameObject _waiting;
        private GameObject _reveal;
        private GameObject _roundResult;
        private GameObject _matchEnd;
        private Text _characterDetail;
        private Text _lobbyStatus;
        private Text _roundResultLabel;
        private Text _matchEndLabel;
        private Button _scoutButton;
        private Button _juggernautButton;
        private string _selectedArchetype = "Scout";
        private bool _inMatch;

        /// <summary>
        /// Raised when Lobby Local Play / Find Match completes and the match HUD should show. The
        /// <c>bool</c> is <c>true</c> for Find Match (networked, C52's resolve relay) and <c>false</c>
        /// for Local Play (same-process, unchanged) — <c>GameBootstrap</c> uses it to pick
        /// <c>RoundPlayback</c>'s <c>IMatchResolver</c> for this match.
        /// </summary>
        public event Action<bool> EnteredMatch;

        /// <summary>Raised from Match End → Rematch (back to Lobby).</summary>
        public event Action RematchRequested;

        /// <summary>Raised from Match End → Quit (back to Title).</summary>
        public event Action QuitToTitleRequested;

        public Screen Current { get; private set; } = Screen.None;

        public string SelectedArchetype => _selectedArchetype;

        public bool IsInMatch => _inMatch;

        public void Init(RectTransform canvasRoot, Font font)
        {
            _font = font;
            _root = CreatePanel(canvasRoot, "AppFlow", PanelDark, Vector2.zero, Vector2.one);
            _root.SetAsLastSibling();

            _boot = BuildBoot();
            _characterSelect = BuildCharacterSelect();
            _lobby = BuildLobby();
            _waiting = BuildMessageScreen("WaitingScreen", "Simulating…");
            _reveal = BuildMessageScreen("RevealScreen", "REVEAL");
            _roundResult = BuildRoundResult();
            _matchEnd = BuildMatchEnd();

            Show(Screen.Boot);
        }

        /// <summary>
        /// Tests / Integrator helper: skip the pre-match shell and expose the Program HUD immediately.
        /// </summary>
        public void BypassToMatch()
        {
            StopAllCoroutines();
            _inMatch = true;
            Show(Screen.None);
            EnteredMatch?.Invoke(false);
        }

        public void ShowBoot()
        {
            _inMatch = false;
            Show(Screen.Boot);
        }

        public void ShowLobby()
        {
            _inMatch = false;
            if (_lobbyStatus != null)
            {
                _lobbyStatus.text = "1v1 Lobby — Find Match or play Local.";
            }

            Show(Screen.Lobby);
        }

        public void ShowRoundResult(string summary, bool matchOver)
        {
            if (_roundResultLabel != null)
            {
                _roundResultLabel.text = summary ?? "ROUND COMPLETE";
            }

            Show(matchOver ? Screen.MatchEnd : Screen.RoundResult);
            if (matchOver && _matchEndLabel != null)
            {
                _matchEndLabel.text = summary ?? "MATCH OVER";
            }
        }

        public void ShowMatchEnd(string summary)
        {
            if (_matchEndLabel != null)
            {
                _matchEndLabel.text = summary ?? "MATCH OVER";
            }

            Show(Screen.MatchEnd);
        }

        /// <summary>Brief Waiting → Reveal flash used around Lock In (UI_FLOW §5–§6).</summary>
        public IEnumerator PlayLockInBridge()
        {
            Show(Screen.Waiting);
            yield return new WaitForSeconds(WaitingStubSeconds);
            Show(Screen.Reveal);
            yield return new WaitForSeconds(RevealFlashSeconds);
            Show(Screen.None);
        }

        private void Show(Screen screen)
        {
            Current = screen;
            SetActive(_boot, screen == Screen.Boot);
            SetActive(_characterSelect, screen == Screen.CharacterSelect);
            SetActive(_lobby, screen == Screen.Lobby);
            SetActive(_waiting, screen == Screen.Waiting);
            SetActive(_reveal, screen == Screen.Reveal);
            SetActive(_roundResult, screen == Screen.RoundResult);
            SetActive(_matchEnd, screen == Screen.MatchEnd);
            _root.gameObject.SetActive(screen != Screen.None);
            if (screen != Screen.None)
            {
                _root.SetAsLastSibling();
            }
        }

        private GameObject BuildBoot()
        {
            GameObject screen = CreateScreen("BootTitle");
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = CreateText(rt, "Title", "logiCard", 72, TextAnchor.MiddleCenter, Accent);
            Stretch(title.rectTransform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.8f));

            Text tag = CreateText(rt, "Tag", "Landscape desktop tactics — programmed movement", 28, TextAnchor.MiddleCenter, Ink);
            Stretch(tag.rectTransform, new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.55f));

            Button play = CreateButton(rt, "TitlePlayButton", "PLAY", Accent, new Color(0.1f, 0.09f, 0.07f), 40,
                () => Show(Screen.CharacterSelect));
            Stretch(play.GetComponent<RectTransform>(), new Vector2(0.35f, 0.22f), new Vector2(0.65f, 0.36f));
            return screen;
        }

        private GameObject BuildCharacterSelect()
        {
            GameObject screen = CreateScreen("CharacterSelect");
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = CreateText(rt, "Title", "CHARACTER SELECT", 48, TextAnchor.MiddleCenter, Accent);
            Stretch(title.rectTransform, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f));

            _scoutButton = CreateButton(rt, "Pick_Scout", "SCOUT", PanelMid, Ink, 32, () => SelectArchetype("Scout"));
            Stretch(_scoutButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.48f), new Vector2(0.48f, 0.72f));

            _juggernautButton = CreateButton(rt, "Pick_Juggernaut", "JUGGERNAUT", PanelMid, Ink, 32,
                () => SelectArchetype("Juggernaut"));
            Stretch(_juggernautButton.GetComponent<RectTransform>(), new Vector2(0.52f, 0.48f), new Vector2(0.88f, 0.72f));

            _characterDetail = CreateText(rt, "Detail", string.Empty, 26, TextAnchor.MiddleCenter, Ink);
            Stretch(_characterDetail.rectTransform, new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.46f));

            Button confirm = CreateButton(rt, "ConfirmCharacter", "CONFIRM", Accent, new Color(0.1f, 0.09f, 0.07f), 34,
                () => Show(Screen.Lobby));
            Stretch(confirm.GetComponent<RectTransform>(), new Vector2(0.35f, 0.10f), new Vector2(0.65f, 0.22f));

            SelectArchetype("Scout");
            return screen;
        }

        private GameObject BuildLobby()
        {
            GameObject screen = CreateScreen("LobbyFindMatch");
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = CreateText(rt, "Title", "LOBBY", 48, TextAnchor.MiddleCenter, Accent);
            Stretch(title.rectTransform, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.92f));

            _lobbyStatus = CreateText(rt, "LobbyStatus", "1v1 Lobby — Find Match or play Local.", 26, TextAnchor.MiddleCenter, Ink);
            Stretch(_lobbyStatus.rectTransform, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.74f));

            Text roles = CreateText(rt, "Roles", "Labels: Attacker / Defender (spawn only)", 22, TextAnchor.MiddleCenter, Ink);
            Stretch(roles.rectTransform, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.58f));

            Button find = CreateButton(rt, "FindMatchButton", "FIND MATCH", Accent, new Color(0.1f, 0.09f, 0.07f), 32,
                () => StartCoroutine(FindMatchStub()));
            Stretch(find.GetComponent<RectTransform>(), new Vector2(0.18f, 0.28f), new Vector2(0.48f, 0.42f));

            Button local = CreateButton(rt, "LocalPlayButton", "LOCAL PLAY", PanelMid, Ink, 32, () => EnterMatch(viaRelay: false));
            Stretch(local.GetComponent<RectTransform>(), new Vector2(0.52f, 0.28f), new Vector2(0.82f, 0.42f));

            Text note = CreateText(rt, "Note",
                "Find Match connects to a resolve relay on 127.0.0.1:7777 — start one manually for now " +
                "(real matchmaking is still open, C52). Local stays same-process for testing.", 20,
                TextAnchor.MiddleCenter, Ink);
            Stretch(note.rectTransform, new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.24f));
            return screen;
        }

        private GameObject BuildRoundResult()
        {
            GameObject screen = CreateScreen("RoundResult");
            RectTransform rt = screen.GetComponent<RectTransform>();

            _roundResultLabel = CreateText(rt, "RoundResultLabel", "ROUND COMPLETE", 44, TextAnchor.MiddleCenter, Accent);
            Stretch(_roundResultLabel.rectTransform, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.7f));

            Button cont = CreateButton(rt, "ContinueButton", "CONTINUE", Accent, new Color(0.1f, 0.09f, 0.07f), 34,
                () => Show(Screen.None));
            Stretch(cont.GetComponent<RectTransform>(), new Vector2(0.35f, 0.22f), new Vector2(0.65f, 0.36f));
            return screen;
        }

        private GameObject BuildMatchEnd()
        {
            GameObject screen = CreateScreen("MatchEnd");
            RectTransform rt = screen.GetComponent<RectTransform>();

            _matchEndLabel = CreateText(rt, "MatchEndLabel", "MATCH OVER", 48, TextAnchor.MiddleCenter, Accent);
            Stretch(_matchEndLabel.rectTransform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.78f));

            Button rematch = CreateButton(rt, "RematchButton", "REMATCH", Accent, new Color(0.1f, 0.09f, 0.07f), 32,
                () =>
                {
                    ShowLobby();
                    RematchRequested?.Invoke();
                });
            Stretch(rematch.GetComponent<RectTransform>(), new Vector2(0.18f, 0.28f), new Vector2(0.48f, 0.42f));

            Button quit = CreateButton(rt, "QuitToTitleButton", "QUIT", PanelMid, Ink, 32,
                () =>
                {
                    ShowBoot();
                    QuitToTitleRequested?.Invoke();
                });
            Stretch(quit.GetComponent<RectTransform>(), new Vector2(0.52f, 0.28f), new Vector2(0.82f, 0.42f));
            return screen;
        }

        private GameObject BuildMessageScreen(string name, string message)
        {
            GameObject screen = CreateScreen(name);
            RectTransform rt = screen.GetComponent<RectTransform>();
            Text label = CreateText(rt, "Message", message, 52, TextAnchor.MiddleCenter, Accent);
            Stretch(label.rectTransform, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f));
            return screen;
        }

        /// <summary>
        /// Queue/session handshake is still a stub (real matchmaking is OPEN, C52) — this only mimics
        /// the wait for pacing. The actual network connection happens later, at this match's first Lock
        /// In (<see cref="RoundPlayback.ResolveAndArm"/> via the relay <c>IMatchResolver</c> GameBootstrap
        /// wires in below), not here.
        /// </summary>
        private IEnumerator FindMatchStub()
        {
            if (_lobbyStatus != null)
            {
                _lobbyStatus.text = "Searching for opponent…";
            }

            yield return new WaitForSeconds(FindMatchStubSeconds);
            if (_lobbyStatus != null)
            {
                _lobbyStatus.text = "Matched. Entering match.";
            }

            EnterMatch(viaRelay: true);
        }

        private void EnterMatch(bool viaRelay)
        {
            _inMatch = true;
            Show(Screen.None);
            EnteredMatch?.Invoke(viaRelay);
        }

        private void SelectArchetype(string archetype)
        {
            _selectedArchetype = archetype;
            if (_characterDetail != null)
            {
                _characterDetail.text = archetype == "Juggernaut"
                    ? "Juggernaut — Speed: slow · Agility: stance/shoot switch costs · Strength: doors faster"
                    : "Scout — Speed: fast · Agility: free stance/shoot switches · Strength: standard doors";
            }

            if (_scoutButton != null)
            {
                _scoutButton.GetComponent<Image>().color = archetype == "Scout" ? Accent : PanelMid;
                _juggernautButton.GetComponent<Image>().color = archetype == "Juggernaut" ? Accent : PanelMid;
            }
        }

        private GameObject CreateScreen(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(_root, false);
            Stretch(rt, Vector2.zero, Vector2.one);
            go.GetComponent<Image>().color = PanelDark;
            go.SetActive(false);
            return go;
        }

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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button CreateButton(RectTransform parent, string name, string label, Color bg, Color fg, int size,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;

            Text text = CreateText(rt, "Label", label, size, TextAnchor.MiddleCenter, fg);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            return button;
        }

        private static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
}
