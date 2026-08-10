using System;
using UnityEngine;
using UnityEngine.UI;

namespace LogiCard.UI
{
    /// <summary>
    /// Generic confirmation / popup: title, body, 1–2 action buttons over a dimmed full-screen blocker.
    /// Reusable for Quit confirms, future matchmaking errors, etc. — not one-off per call site.
    /// </summary>
    public sealed class ModalDialog
    {
        private readonly GameObject _root;

        private ModalDialog(GameObject root)
        {
            _root = root;
        }

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Close()
        {
            if (_root == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(_root);
        }

        /// <summary>
        /// Builds and shows a modal under <paramref name="canvasRoot"/>. Primary is the accent
        /// confirm; optional secondary is a mid-tone cancel. Either action closes the dialog first.
        /// </summary>
        public static ModalDialog Show(
            UiFactory ui,
            RectTransform canvasRoot,
            string title,
            string body,
            string primaryLabel,
            Action onPrimary,
            string secondaryLabel = null,
            Action onSecondary = null)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (canvasRoot == null)
            {
                throw new ArgumentNullException(nameof(canvasRoot));
            }

            RectTransform dimmer = ui.CreatePanel(canvasRoot, "ModalDialog", UiStyle.ModalDimmer,
                Vector2.zero, Vector2.one);
            dimmer.SetAsLastSibling();

            ModalDialog dialog = null;
            dialog = new ModalDialog(dimmer.gameObject);

            // Centered ~40%×40% card — the old 56%×44% band stretched unreadably on ultrawide and
            // left title/body/buttons floating in a wide empty panel at 16:9.
            RectTransform card = ui.CreatePanel(dimmer, "DialogCard", UiStyle.Card,
                new Vector2(0.28f, 0.28f), new Vector2(0.72f, 0.72f), UiStyle.RoundSprite, Image.Type.Sliced);

            Text titleText = ui.CreateText(card, "Title", title ?? string.Empty, 34, TextAnchor.MiddleCenter, UiStyle.Ink,
                UiTextOverflow.SingleLine);
            titleText.fontStyle = FontStyle.Bold;
            UiFactory.Stretch(titleText.rectTransform, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.92f));

            Text bodyText = ui.CreateText(card, "Body", body ?? string.Empty, 22, TextAnchor.MiddleCenter, UiStyle.Ink);
            UiFactory.Stretch(bodyText.rectTransform, new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.68f));

            ui.CreatePanel(card, "Divider", UiStyle.AccentDim, new Vector2(0.12f, 0.29f), new Vector2(0.88f, 0.31f), UiStyle.RoundSprite, Image.Type.Sliced);

            bool hasSecondary = !string.IsNullOrEmpty(secondaryLabel);
            if (hasSecondary)
            {
                Button secondary = ui.CreateButton(card, "ModalSecondary", secondaryLabel, UiStyle.SecondaryButton, UiStyle.Ink, 26,
                    () =>
                    {
                        dialog.Close();
                        onSecondary?.Invoke();
                    },
                    UiStyle.RoundSprite, Image.Type.Sliced);
                UiFactory.Stretch(secondary.GetComponent<RectTransform>(),
                    new Vector2(0.08f, 0.10f), new Vector2(0.46f, 0.30f));

                Button primary = ui.CreateButton(card, "ModalPrimary", primaryLabel ?? "OK", UiStyle.PrimaryButton, UiStyle.PrimaryButtonText, 26,
                    () =>
                    {
                        dialog.Close();
                        onPrimary?.Invoke();
                    },
                    UiStyle.RoundSprite, Image.Type.Sliced);
                UiFactory.Stretch(primary.GetComponent<RectTransform>(),
                    new Vector2(0.54f, 0.10f), new Vector2(0.92f, 0.30f));
            }
            else
            {
                Button primary = ui.CreateButton(card, "ModalPrimary", primaryLabel ?? "OK", UiStyle.PrimaryButton, UiStyle.PrimaryButtonText, 28,
                    () =>
                    {
                        dialog.Close();
                        onPrimary?.Invoke();
                    },
                    UiStyle.RoundSprite, Image.Type.Sliced);
                UiFactory.Stretch(primary.GetComponent<RectTransform>(),
                    new Vector2(0.28f, 0.10f), new Vector2(0.72f, 0.30f));
            }

            return dialog;
        }
    }
}
