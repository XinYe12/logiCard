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
        private Text _mapDetail;
        private Text _lobbyStatus;
        private Text _roundResultLabel;
        private Text _matchEndLabel;
        private CharacterSelectView _characterSelectView;
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
            _root = _ui.CreatePanel(canvasRoot, "AppFlow", UiStyle.ShellVoid, Vector2.zero, Vector2.one);
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
            GameObject screen = CreateScreen("BootTitle", UiStyle.ShellGlowDefault);
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = _ui.CreateHeadline(rt, "Title", "logiCard", 104, UiStyle.ShellTitleInk,
                UiTextOverflow.SingleLine, shadowDistance: 6f);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.80f));

            _ui.CreateRule(rt, "TitleRule", new Vector2(0.44f, 0.552f), new Vector2(0.56f, 0.562f));

            Text tag = _ui.CreateText(rt, "Tag", "LANDSCAPE DESKTOP TACTICS  ·  PROGRAMMED MOVEMENT", 22,
                TextAnchor.MiddleCenter, UiStyle.ShellBodyInk, UiTextOverflow.SingleLine);
            UiFactory.Stretch(tag.rectTransform, new Vector2(0.1f, 0.47f), new Vector2(0.9f, 0.535f));

            Button play = _ui.CreateShellButton(rt, "TitlePlayButton", "PLAY", ShellButtonTone.Primary, 36,
                () => Show(Screen.CharacterSelect), riser: 10f);
            UiFactory.Stretch(play.GetComponent<RectTransform>(), new Vector2(0.39f, 0.235f), new Vector2(0.61f, 0.35f));

            Text footer = _ui.CreateText(rt, "BootFooter", "PROTOTYPE BUILD", 16, TextAnchor.MiddleCenter,
                UiStyle.ShellMutedInk, UiTextOverflow.SingleLine);
            UiFactory.Stretch(footer.rectTransform, new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.11f));
            return screen;
        }

        private GameObject BuildCharacterSelect()
        {
            GameObject screen = CreateScreen("CharacterSelect", UiStyle.CharSelectBgScout);
            RectTransform rt = screen.GetComponent<RectTransform>();

            _characterSelectView = CharacterSelectView.Build(_ui, rt, FindBackdropGlow(rt));
            _characterSelectView.SelectionChanged += OnCharacterSelectionChanged;

            Button confirm = _ui.CreateShellButton(rt, "ConfirmCharacter", "CONFIRM", ShellButtonTone.Primary, 30,
                () => Show(Screen.MapSelect));
            UiFactory.Stretch(confirm.GetComponent<RectTransform>(), new Vector2(0.37f, 0.035f), new Vector2(0.63f, 0.135f));

            OnCharacterSelectionChanged(_characterSelectView.SelectedId);
            return screen;
        }

        private GameObject BuildMapSelect()
        {
            GameObject screen = CreateScreen("MapSelect", UiStyle.ShellGlowDefault);
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = _ui.CreateHeadline(rt, "Title", "MAP SELECT", 52, UiStyle.ShellTitleInk);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.1f, 0.855f), new Vector2(0.9f, 0.955f));

            _ui.CreateRule(rt, "TitleRule", new Vector2(0.46f, 0.833f), new Vector2(0.54f, 0.842f));

            _mapGrid = SelectionGrid.Build(
                _ui,
                rt,
                new[]
                {
                    new SelectionOption(MapId.FreightYard.ToString(), "FREIGHT YARD"),
                    new SelectionOption(MapId.RailPlatform.ToString(), "RAIL PLATFORM"),
                    new SelectionOption(MapId.VaultComplex.ToString(), "VAULT COMPLEX"),
                },
                new Vector2(0.12f, 0.48f),
                new Vector2(0.88f, 0.78f),
                columns: 3,
                fontSize: 26);
            _mapGrid.SelectionChanged += OnMapSelectionChanged;

            RectTransform detailPlate = _ui.CreateShellPlate(rt, "MapDetailPlate",
                new Vector2(0.17f, 0.28f), new Vector2(0.83f, 0.42f));
            _mapDetail = _ui.CreateText(detailPlate, "MapDetail", string.Empty, 22, TextAnchor.MiddleCenter,
                UiStyle.ModalInk);
            UiFactory.Stretch(_mapDetail.rectTransform, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.92f));

            Button confirm = _ui.CreateShellButton(rt, "ConfirmMap", "CONFIRM", ShellButtonTone.Primary, 30,
                () =>
                {
                    MapSelected?.Invoke(_selectedMapId);
                    Show(Screen.Lobby);
                });
            UiFactory.Stretch(confirm.GetComponent<RectTransform>(), new Vector2(0.38f, 0.085f), new Vector2(0.62f, 0.195f));

            OnMapSelectionChanged(_mapGrid.SelectedId);
            return screen;
        }

        private GameObject BuildLobby()
        {
            GameObject screen = CreateScreen("LobbyFindMatch", UiStyle.ShellGlowLobby);
            RectTransform rt = screen.GetComponent<RectTransform>();

            Text title = _ui.CreateHeadline(rt, "Title", "LOBBY", 56, UiStyle.ShellTitleInk);
            UiFactory.Stretch(title.rectTransform, new Vector2(0.1f, 0.815f), new Vector2(0.9f, 0.93f));

            _ui.CreateRule(rt, "TitleRule", new Vector2(0.46f, 0.792f), new Vector2(0.54f, 0.801f));

            RectTransform plate = _ui.CreateShellPlate(rt, "LobbyPlate",
                new Vector2(0.23f, 0.51f), new Vector2(0.77f, 0.73f));

            _lobbyStatus = _ui.CreateText(plate, "LobbyStatus", "1v1 Lobby — Find Match or play Local.", 26,
                TextAnchor.MiddleCenter, UiStyle.ModalInk);
            UiFactory.Stretch(_lobbyStatus.rectTransform, new Vector2(0.06f, 0.45f), new Vector2(0.94f, 0.9f));

            _ui.CreatePanel(plate, "PlateDivider", UiStyle.ModalDivider,
                new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.415f), UiStyle.PillSprite, Image.Type.Sliced);

            Text roles = _ui.CreateText(plate, "Roles", "Attacker / Defender — spawn labels only", 20,
                TextAnchor.MiddleCenter, UiStyle.ModalInk);
            UiFactory.Stretch(roles.rectTransform, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.37f));

            Button find = _ui.CreateShellButton(rt, "FindMatchButton", "FIND MATCH", ShellButtonTone.Primary, 30,
                () => StartCoroutine(FindMatchStub()));
            UiFactory.Stretch(find.GetComponent<RectTransform>(), new Vector2(0.21f, 0.30f), new Vector2(0.475f, 0.42f));

            Button local = _ui.CreateShellButton(rt, "LocalPlayButton", "LOCAL PLAY", ShellButtonTone.Secondary, 30,
                () => EnterMatch(viaRelay: false));
            UiFactory.Stretch(local.GetComponent<RectTransform>(), new Vector2(0.525f, 0.30f), new Vector2(0.79f, 0.42f));

            Text note = _ui.CreateText(rt, "Note",
                "Find Match connects to a resolve relay on 127.0.0.1:7777 — start one manually for now " +
                "(real matchmaking is still open, C52). Local stays same-process for testing.", 18,
                TextAnchor.MiddleCenter, UiStyle.ShellMutedInk);
            UiFactory.Stretch(note.rectTransform, new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.25f));
            return screen;
        }

        private GameObject BuildRoundResult()
        {
            GameObject screen = CreateScreen("RoundResult", UiStyle.ShellGlowDefault);
            RectTransform rt = screen.GetComponent<RectTransform>();

            _roundResultLabel = _ui.CreateHeadline(rt, "RoundResultLabel", "ROUND COMPLETE", 52, UiStyle.ShellTitleInk,
                UiTextOverflow.Body);
            UiFactory.Stretch(_roundResultLabel.rectTransform, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.68f));

            _ui.CreateRule(rt, "TitleRule", new Vector2(0.455f, 0.455f), new Vector2(0.545f, 0.464f));

            Button cont = _ui.CreateShellButton(rt, "ContinueButton", "CONTINUE", ShellButtonTone.Primary, 32,
                () => Show(Screen.None));
            UiFactory.Stretch(cont.GetComponent<RectTransform>(), new Vector2(0.385f, 0.25f), new Vector2(0.615f, 0.36f));
            return screen;
        }

        private GameObject BuildMatchEnd()
        {
            GameObject screen = CreateScreen("MatchEnd", UiStyle.ShellGlowVerdict);
            RectTransform rt = screen.GetComponent<RectTransform>();

            _matchEndLabel = _ui.CreateHeadline(rt, "MatchEndLabel", "MATCH OVER", 64, UiStyle.ShellTitleInk,
                UiTextOverflow.Body, shadowDistance: 5f);
            UiFactory.Stretch(_matchEndLabel.rectTransform, new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.77f));

            _ui.CreateRule(rt, "TitleRule", new Vector2(0.45f, 0.525f), new Vector2(0.55f, 0.535f));

            Button rematch = _ui.CreateShellButton(rt, "RematchButton", "REMATCH", ShellButtonTone.Primary, 30,
                () =>
                {
                    ShowLobby();
                    RematchRequested?.Invoke();
                });
            UiFactory.Stretch(rematch.GetComponent<RectTransform>(), new Vector2(0.21f, 0.30f), new Vector2(0.475f, 0.42f));

            Button quit = _ui.CreateShellButton(rt, "QuitToTitleButton", "QUIT", ShellButtonTone.Quiet, 30,
                ConfirmQuitToTitle);
            UiFactory.Stretch(quit.GetComponent<RectTransform>(), new Vector2(0.525f, 0.30f), new Vector2(0.79f, 0.42f));
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
            GameObject screen = CreateScreen(name, UiStyle.ShellGlowDefault);
            RectTransform rt = screen.GetComponent<RectTransform>();
            Text label = _ui.CreateHeadline(rt, "Message", message, 58, UiStyle.ShellTitleInk);
            UiFactory.Stretch(label.rectTransform, new Vector2(0.1f, 0.44f), new Vector2(0.9f, 0.62f));
            _ui.CreateRule(rt, "MessageRule", new Vector2(0.455f, 0.415f), new Vector2(0.545f, 0.424f));
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

        /// <summary>
        /// A shell screen is a lit backdrop with objects on it, not a flat colour fill — the screen
        /// root's own Image is only the input blocker (fully transparent), and
        /// <see cref="UiFactory.CreateShellBackdrop"/> paints void + light pool + grain + vignette
        /// underneath everything the screen adds afterwards.
        /// </summary>
        private GameObject CreateScreen(string name, Color glowTint)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(_root, false);
            UiFactory.Stretch(rt, Vector2.zero, Vector2.one);
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            _ui.CreateShellBackdrop(rt, glowTint);
            go.SetActive(false);
            return go;
        }

        /// <summary>The re-tintable light pool <see cref="UiFactory.CreateShellBackdrop"/> put on a screen.</summary>
        private static Image FindBackdropGlow(RectTransform screenRoot)
        {
            Transform glow = screenRoot.Find("BackdropGlow");
            return glow != null ? glow.GetComponent<Image>() : null;
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
