using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// One option in a <see cref="SelectionGrid"/> — id is stable for tests / save data; label is
    /// what the player sees. Detail text is owned by the host screen (Character Select detail line,
    /// future loadout blurb, etc.).
    /// </summary>
    public readonly struct SelectionOption
    {
        public string Id { get; }
        public string Label { get; }

        public SelectionOption(string id, string label)
        {
            Id = id;
            Label = label;
        }
    }

    /// <summary>
    /// Reusable selection grid/list. Character Select sits on top of this; a future loadout /
    /// cosmetic picker can reuse the same shape without hardcoding Scout/Juggernaut buttons.
    /// Button GameObject names are <c>Pick_{Id}</c> so existing PlayMode lookups keep working.
    /// </summary>
    public sealed class SelectionGrid
    {
        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>();
        private string _selectedId;

        public string SelectedId => _selectedId;

        public event Action<string> SelectionChanged;

        private SelectionGrid()
        {
        }

        public Button GetButton(string id)
        {
            return _buttons.TryGetValue(id, out Button button) ? button : null;
        }

        public void Select(string id)
        {
            if (string.IsNullOrEmpty(id) || !_buttons.ContainsKey(id))
            {
                return;
            }

            _selectedId = id;
            foreach (KeyValuePair<string, Button> pair in _buttons)
            {
                pair.Value.GetComponent<Image>().color =
                    pair.Key == _selectedId ? UiStyle.Accent : UiStyle.PanelMid;
            }

            SelectionChanged?.Invoke(_selectedId);
        }

        /// <summary>
        /// Builds a grid in <paramref name="area"/> (normalized anchors under <paramref name="parent"/>).
        /// Columns default to the option count when ≤ 3 so Character Select stays a clean 2-up row.
        /// </summary>
        public static SelectionGrid Build(
            UiFactory ui,
            RectTransform parent,
            IReadOnlyList<SelectionOption> options,
            Vector2 areaMin,
            Vector2 areaMax,
            int columns = 0,
            int fontSize = 32)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (options == null || options.Count == 0)
            {
                throw new ArgumentException("SelectionGrid needs at least one option.", nameof(options));
            }

            int cols = columns > 0 ? columns : Mathf.Min(options.Count, 3);
            int rows = Mathf.CeilToInt(options.Count / (float)cols);

            RectTransform root = ui.CreatePanel(parent, "SelectionGrid", new Color(0f, 0f, 0f, 0f), areaMin, areaMax);
            var grid = new SelectionGrid();

            for (int i = 0; i < options.Count; i++)
            {
                SelectionOption option = options[i];
                int col = i % cols;
                int row = i / cols;

                float x0 = col / (float)cols;
                float x1 = (col + 1) / (float)cols;
                float y1 = 1f - row / (float)rows;
                float y0 = 1f - (row + 1) / (float)rows;

                // Inset so neighboring cells don't share an edge (readable mouse targets).
                const float inset = 0.04f;
                Button button = ui.CreateButton(
                    root,
                    $"Pick_{option.Id}",
                    option.Label,
                    UiStyle.PanelMid,
                    UiStyle.Ink,
                    fontSize,
                    null);

                // Long labels (JUGGERNAUT) stay inside the cell at 720p instead of truncating mid-word.
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.resizeTextForBestFit = true;
                    label.resizeTextMinSize = Mathf.Max(16, fontSize - 14);
                    label.resizeTextMaxSize = fontSize;
                }

                string capturedId = option.Id;
                button.onClick.AddListener(() => grid.Select(capturedId));

                UiFactory.Stretch(
                    button.GetComponent<RectTransform>(),
                    new Vector2(x0 + inset, y0 + inset),
                    new Vector2(x1 - inset, y1 - inset));

                grid._buttons[option.Id] = button;
            }

            grid.Select(options[0].Id);
            return grid;
        }
    }
}
