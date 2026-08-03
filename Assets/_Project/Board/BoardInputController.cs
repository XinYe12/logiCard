using System;
using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using LogiCard.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogiCard.Board
{
    /// <summary>
    /// Program input for one pawn: raycasts board tiles, drafts a multi-waypoint path (each tap
    /// appends a waypoint) at a directly picked stance, and commits Move/Shoot into a
    /// <see cref="PawnProgram"/>. Cost is automatic — no time-allotment step (C21, amended
    /// 2026-08-03). Origin and budget refresh each round from carried state + Time Card (C33).
    /// </summary>
    public sealed class BoardInputController : MonoBehaviour
    {
        private PawnView _pawn;
        private RoundPhaseController _phase;
        private GridCoordinate _origin;
        private float _baseSecondsPerTile;
        private float _budgetSeconds;
        private GridBoard _board;
        private BoardView _boardView;
        private bool _locked;
        private StanceType _preferredStance = StanceType.Walk;
        private ShootMode _preferredShootMode = ShootMode.SnapShot;
        private PathPreviewView _pathPreview;

        public ActionVerb Mode { get; set; } = ActionVerb.Move;

        public PawnProgram Program { get; private set; }

        public GridCoordinate Origin => _origin;

        public StanceType PreferredStance
        {
            get => _preferredStance;
            set
            {
                _preferredStance = value;
                if (Program != null)
                {
                    Program.SetPreferredStance(value);
                    RefreshPreview();
                    QueueChanged?.Invoke(Program);
                }
            }
        }

        public ShootMode PreferredShootMode
        {
            get => _preferredShootMode;
            set
            {
                _preferredShootMode = value == ShootMode.None ? ShootMode.SnapShot : value;
                if (Program != null)
                {
                    Program.SetShootMode(_preferredShootMode);
                    QueueChanged?.Invoke(Program);
                }
            }
        }

        public event Action<PawnProgram> QueueChanged;

        public void Init(
            PawnView pawn,
            RoundPhaseController phase,
            GridCoordinate origin,
            float baseSecondsPerTile,
            float budgetSeconds,
            BoardView boardView = null)
        {
            _pawn = pawn;
            _phase = phase;
            _origin = origin;
            _baseSecondsPerTile = baseSecondsPerTile;
            _budgetSeconds = budgetSeconds;
            _boardView = boardView;
            _board = boardView != null ? boardView.Model : null;

            if (_pathPreview == null && boardView != null)
            {
                var previewGo = new GameObject("PathPreview");
                previewGo.transform.SetParent(transform, false);
                _pathPreview = previewGo.AddComponent<PathPreviewView>();
                _pathPreview.Init(boardView);
            }

            _phase.PhaseChanged += OnPhaseChanged;
            ResetProgram();
        }

        /// <summary>Updates the carried start tile and round budget before Program rebuilds (C33).</summary>
        public void PrepareRound(GridCoordinate origin, float budgetSeconds)
        {
            _origin = origin;
            _budgetSeconds = budgetSeconds < 0f ? 0f : budgetSeconds;
        }

        private void OnDestroy()
        {
            if (_phase != null)
            {
                _phase.PhaseChanged -= OnPhaseChanged;
            }
        }

        /// <summary>
        /// Stops further scheduling. The pawn's playback path now comes from the resolved ReplayTape
        /// rather than from this preview, so nothing is written to the view here.
        /// </summary>
        public void CommitToPlayback()
        {
            TryCommitDraftPath();
            _locked = true;
            _pathPreview?.Clear();
        }

        /// <summary>Commits a pending draft path at its allotted stance, if any.</summary>
        public bool TryCommitDraftPath()
        {
            if (_locked || Program == null || !Program.HasDraft)
            {
                return false;
            }

            if (!Program.TryCommitDraft(out string reason))
            {
                Debug.Log($"[logiCard] Commit draft rejected: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        public bool TrySetDraftStance(StanceType stance)
        {
            if (_locked || Program == null)
            {
                return false;
            }

            _preferredStance = stance;
            if (!Program.HasDraft)
            {
                Program.SetPreferredStance(stance);
                QueueChanged?.Invoke(Program);
                return true;
            }

            if (!Program.TrySetDraftStance(stance, out string reason))
            {
                Debug.Log($"[logiCard] Set draft stance rejected: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (phase == RoundPhase.Program)
            {
                ResetProgram();
            }
        }

        private void ResetProgram()
        {
            Program = new PawnProgram(_origin, _baseSecondsPerTile, _budgetSeconds, _preferredStance, _board);
            Program.SetShootMode(_preferredShootMode);
            _locked = false;
            RefreshPreview();
            QueueChanged?.Invoke(Program);
        }

        private void Update()
        {
            if (_locked || _phase.Phase != RoundPhase.Program)
            {
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            TryHandleClick();
        }

        /// <summary>
        /// Queues the current <see cref="Mode"/> against an already-resolved tile and refreshes the
        /// preview. Split out from the raycast so the schedule path can be driven without a real
        /// pointer (mouse state cannot be synthesised through legacy <see cref="Input"/>).
        /// </summary>
        public bool TryTapTile(GridCoordinate coordinate)
        {
            if (_locked || _phase.Phase != RoundPhase.Program)
            {
                return false;
            }

            if (Mode == ActionVerb.Shoot)
            {
                TryCommitDraftPath();
            }

            string reason;
            bool queued = Mode == ActionVerb.Move
                ? Program.TryAddWaypoint(coordinate, out reason)
                : Program.TryQueueShoot(coordinate, out reason, _preferredShootMode);

            if (!queued)
            {
                Debug.Log($"[logiCard] {Mode} rejected at {coordinate}: {reason}");
                return false;
            }

            RefreshPreview();
            QueueChanged?.Invoke(Program);
            return true;
        }

        private void RefreshPreview()
        {
            if (_pawn == null || Program == null)
            {
                return;
            }

            _pawn.SetPath(Program.BuildMovePreviewPath(_origin));
            float previewTime = Program.UsedSeconds;
            if (Program.HasDraft)
            {
                previewTime += Program.DraftAllottedSeconds;
            }

            _pawn.ApplyTime(previewTime);
            RefreshPathBeads();
        }

        private void RefreshPathBeads()
        {
            if (_pathPreview == null || Program == null)
            {
                return;
            }

            var points = new List<GridCoordinate> { _origin };
            foreach (ActionNode node in Program.Nodes)
            {
                if (node.Verb == ActionVerb.Move)
                {
                    points.Add(node.GridPosition);
                }
            }

            if (Program.HasDraft)
            {
                for (int i = 0; i < Program.DraftWaypoints.Count; i++)
                {
                    points.Add(Program.DraftWaypoints[i]);
                }

                _pathPreview.Show(points, isDraft: true);
                return;
            }

            if (points.Count > 1)
            {
                _pathPreview.Show(points, isDraft: false);
                return;
            }

            _pathPreview.Clear();
        }

        private void TryHandleClick()
        {
            Camera cam = Camera.main;
            if (cam == null || !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                return;
            }

            var marker = hit.collider.GetComponent<TileMarker>();
            if (marker == null)
            {
                return;
            }

            TryTapTile(marker.Coordinate);
        }
    }
}
