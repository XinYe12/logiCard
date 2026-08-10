using System;
using System.Collections;
using LogiCard.Board;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Pre-match shell for <c>docs/UI_FLOW.md</c>'s screen map (C48): Boot/Title → Character Select
    /// → Lobby → Waiting/Reveal bridge → Round Result → Match End. Built on <see cref="UiFactory"/>
    /// so chrome matches the in-match HUD.
    /// </summary>
    public sealed class AppFlowController : MonoBehaviour
    {
        public enum Screen
        {
            None,
            Boot,
            CharacterSelect,
            MapSelect,
            Lobby,
            Waiting,
            Reveal,
            RoundResult,
            MatchEnd,
        }

        private const float FindMatchStubSeconds = 1.25f;
        private const float RevealFlashSeconds = 1.2f;
        private const float WaitingStubSeconds = 0.6f;

        private UiFactory _ui;
        private RectTransform _root;
        private GameObject _boot;
        private GameObject _characterSelect;
        private GameObject _lobby;
        private GameObject _mapSelect;
        private GameObject _waiting;
        private GameObject _reveal;
        private GameObject _roundResult;
        private GameObject _matchEnd;
        private Text _characterDetail;
        private Text _mapDetail;
        private Text _lobbyStatus;
        private Text _roundResultLabel;
        private Text _matchEndLabel;
        private SelectionGrid _characterGrid;
        private SelectionGrid _mapGrid;
        private string _selectedArchetype = "Scout";
        private MapId _selectedMapId = MapId.FreightYard;
        private bool _inMatch;
        private ModalDialog _openDialog;

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

        public MapId SelectedMapId => _selectedMapId;

        public event Action<MapId> MapSelected;

        public void Init(RectTransform canvasRoot, Font font)
        {
            _ui = new UiFactory(font);
            _root = _ui.CreatePanel(canvasRoot, "AppFlow", UiStyle.PanelDark, Vector2.zero, Vector2.one);
            _root.SetAsLastSibling();

            _boot = BuildBoot();
            _characterSelect = BuildCharacterSelect();
            _mapSelect = BuildMapSelect();
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
            SetActive(_mapSelect, screen == Screen.MapSelect);
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

            Text title = _ui.CreateText(rt, "Title", "logiCard", 72, TextAnchor.MiddleCenter, UiStyle.Accent);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.8f));

            Text tag = _ui.CreateText(rt, "Tag", "Landscape desktop tactics — programmed movement", 28,
                TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(tag.rectTransform, new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.55f));

            Button play = _ui.CreateButton(rt, "TitlePlayButton", "PLAY", UiStyle.Accent, UiStyle.InkDark, 40,
                () => Show(Screen.CharacterSelect));
            UiFactory.Stretch(play.GetComponent<RectTransform>(), new Vector2(0.35f, 0.22f), new Vector2(0.65f, 0.36f));
            return screen;
        }

        private GameObject BuildCharacterSelect()
        {
            GameObject screen = CreateScreen("CharacterSelect");
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = _ui.CreateText(rt, "Title", "CHARACTER SELECT", 44, TextAnchor.MiddleCenter, UiStyle.Accent,
                UiTextOverflow.SingleLine);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.1f, 0.84f), new Vector2(0.9f, 0.95f));

            // Clear vertical bands: title → grid → detail → confirm (old grid/detail edge was only 0.02 apart).
            _characterGrid = SelectionGrid.Build(
                _ui,
                rt,
                new[]
                {
                    new SelectionOption("Scout", "SCOUT"),
                    new SelectionOption("Juggernaut", "JUGGERNAUT"),
                },
                new Vector2(0.14f, 0.50f),
                new Vector2(0.86f, 0.78f),
                columns: 2,
                fontSize: 34);
            _characterGrid.SelectionChanged += OnCharacterSelectionChanged;

            _characterDetail = _ui.CreateText(rt, "Detail", string.Empty, 24, TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(_characterDetail.rectTransform, new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.46f));

            Button confirm = _ui.CreateButton(rt, "ConfirmCharacter", "CONFIRM", UiStyle.Accent, UiStyle.InkDark, 32,
                () => Show(Screen.MapSelect));
            UiFactory.Stretch(confirm.GetComponent<RectTransform>(), new Vector2(0.34f, 0.10f), new Vector2(0.66f, 0.24f));

            OnCharacterSelectionChanged(_characterGrid.SelectedId);
            return screen;
        }

        private GameObject BuildMapSelect()
        {
            GameObject screen = CreateScreen("MapSelect");
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = _ui.CreateText(rt, "Title", "MAP SELECT", 44, TextAnchor.MiddleCenter, UiStyle.Accent,
                UiTextOverflow.SingleLine);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.1f, 0.84f), new Vector2(0.9f, 0.95f));

            _mapGrid = SelectionGrid.Build(
                _ui,
                rt,
                new[]
                {
                    new SelectionOption(MapId.FreightYard.ToString(), "FREIGHT YARD"),
                    new SelectionOption(MapId.RailPlatform.ToString(), "RAIL PLATFORM"),
                    new SelectionOption(MapId.VaultComplex.ToString(), "VAULT COMPLEX"),
                },
                new Vector2(0.14f, 0.50f),
                new Vector2(0.86f, 0.78f),
                columns: 3,
                fontSize: 28);
            _mapGrid.SelectionChanged += OnMapSelectionChanged;

            _mapDetail = _ui.CreateText(rt, "MapDetail", string.Empty, 22, TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(_mapDetail.rectTransform, new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.46f));

            Button confirm = _ui.CreateButton(rt, "ConfirmMap", "CONFIRM", UiStyle.Accent, UiStyle.InkDark, 32,
                () =>
                {
                    MapSelected?.Invoke(_selectedMapId);
                    Show(Screen.Lobby);
                });
            UiFactory.Stretch(confirm.GetComponent<RectTransform>(), new Vector2(0.34f, 0.10f), new Vector2(0.66f, 0.24f));

            OnMapSelectionChanged(_mapGrid.SelectedId);
            return screen;
        }

        private GameObject BuildLobby()
        {
            GameObject screen = CreateScreen("LobbyFindMatch");
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = _ui.CreateText(rt, "Title", "LOBBY", 48, TextAnchor.MiddleCenter, UiStyle.Accent);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.92f));

            _lobbyStatus = _ui.CreateText(rt, "LobbyStatus", "1v1 Lobby — Find Match or play Local.", 26,
                TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(_lobbyStatus.rectTransform, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.74f));

            Text roles = _ui.CreateText(rt, "Roles", "Labels: Attacker / Defender (spawn only)", 22,
                TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(roles.rectTransform, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.58f));

            Button find = _ui.CreateButton(rt, "FindMatchButton", "FIND MATCH", UiStyle.Accent, UiStyle.InkDark, 32,
                () => StartCoroutine(FindMatchStub()));
            UiFactory.Stretch(find.GetComponent<RectTransform>(), new Vector2(0.18f, 0.28f), new Vector2(0.48f, 0.42f));

            Button local = _ui.CreateButton(rt, "LocalPlayButton", "LOCAL PLAY", UiStyle.PanelMid, UiStyle.Ink, 32,
                () => EnterMatch(viaRelay: false));
            UiFactory.Stretch(local.GetComponent<RectTransform>(), new Vector2(0.52f, 0.28f), new Vector2(0.82f, 0.42f));

            Text note = _ui.CreateText(rt, "Note",
                "Find Match connects to a resolve relay on 127.0.0.1:7777 — start one manually for now " +
                "(real matchmaking is still open, C52). Local stays same-process for testing.", 20,
                TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(note.rectTransform, new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.24f));
            return screen;
        }

        private GameObject BuildRoundResult()
        {
            GameObject screen = CreateScreen("RoundResult");
            RectTransform rt = screen.GetComponent<RectTransform>();

            _roundResultLabel = _ui.CreateText(rt, "RoundResultLabel", "ROUND COMPLETE", 44, TextAnchor.MiddleCenter,
                UiStyle.Accent);
            UiFactory.Stretch(_roundResultLabel.rectTransform, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.7f));

            Button cont = _ui.CreateButton(rt, "ContinueButton", "CONTINUE", UiStyle.Accent, UiStyle.InkDark, 34,
                () => Show(Screen.None));
            UiFactory.Stretch(cont.GetComponent<RectTransform>(), new Vector2(0.35f, 0.22f), new Vector2(0.65f, 0.36f));
            return screen;
        }

        private GameObject BuildMatchEnd()
        {
            GameObject screen = CreateScreen("MatchEnd");
            RectTransform rt = screen.GetComponent<RectTransform>();

            _matchEndLabel = _ui.CreateText(rt, "MatchEndLabel", "MATCH OVER", 48, TextAnchor.MiddleCenter, UiStyle.Accent);
            UiFactory.Stretch(_matchEndLabel.rectTransform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.78f));

            Button rematch = _ui.CreateButton(rt, "RematchButton", "REMATCH", UiStyle.Accent, UiStyle.InkDark, 32,
                () =>
                {
                    ShowLobby();
                    RematchRequested?.Invoke();
                });
            UiFactory.Stretch(rematch.GetComponent<RectTransform>(), new Vector2(0.18f, 0.28f), new Vector2(0.48f, 0.42f));

            Button quit = _ui.CreateButton(rt, "QuitToTitleButton", "QUIT", UiStyle.PanelMid, UiStyle.Ink, 32,
                ConfirmQuitToTitle);
            UiFactory.Stretch(quit.GetComponent<RectTransform>(), new Vector2(0.52f, 0.28f), new Vector2(0.82f, 0.42f));
            return screen;
        }

        private void ConfirmQuitToTitle()
        {
            if (_openDialog != null && _openDialog.IsOpen)
            {
                return;
            }

            _openDialog = ModalDialog.Show(
                _ui,
                _root,
                "QUIT TO TITLE?",
                "Leave this match result and return to the title screen.",
                "QUIT",
                () =>
                {
                    _openDialog = null;
                    ShowBoot();
                    QuitToTitleRequested?.Invoke();
                },
                "CANCEL",
                () => _openDialog = null);
        }

        private GameObject BuildMessageScreen(string name, string message)
        {
            GameObject screen = CreateScreen(name);
            RectTransform rt = screen.GetComponent<RectTransform>();
            Text label = _ui.CreateText(rt, "Message", message, 52, TextAnchor.MiddleCenter, UiStyle.Accent);
            UiFactory.Stretch(label.rectTransform, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f));
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

        private void OnCharacterSelectionChanged(string archetype)
        {
            _selectedArchetype = archetype;
            if (_characterDetail != null)
            {
                _characterDetail.text = archetype == "Juggernaut"
                    ? "Juggernaut — Speed: slow · Agility: stance/shoot switch costs · Strength: doors faster"
                    : "Scout — Speed: fast · Agility: free stance/shoot switches · Strength: standard doors";
            }
        }

        private void OnMapSelectionChanged(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                return;
            }

            if (!Enum.TryParse(mapId, out MapId parsedMap))
            {
                return;
            }

            _selectedMapId = parsedMap;
            _mapDetail.text = _selectedMapId switch
            {
                MapId.RailPlatform => "Rail Platform — narrow corridor, crawlspace flank, and elevated objective.",
                MapId.VaultComplex => "Vault Complex — split entry, side rooms, and a deep vault objective.",
                _ => "Freight Yard — open approach, hall chokepoint, and vault objective.",
            };
            MapSelected?.Invoke(_selectedMapId);
        }

        private GameObject CreateScreen(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(_root, false);
            UiFactory.Stretch(rt, Vector2.zero, Vector2.one);
            go.GetComponent<Image>().color = UiStyle.PanelDark;
            go.SetActive(false);
            return go;
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
