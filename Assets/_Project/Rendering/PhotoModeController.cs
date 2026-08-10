using UnityEngine;
using UnityEngine.Rendering;

namespace LogiCard.Rendering
{
    /// <summary>
    /// C54 photo-mode toggle: swaps the scene global Volume between the readability-first live
    /// grade and a more aggressive hero-shot profile (stronger bloom/vignette/grade + DoF).
    /// Hotkey only — not wired into gameplay input. Press <see cref="ToggleKey"/> to flip.
    /// </summary>
    public sealed class PhotoModeController : MonoBehaviour
    {
        public const KeyCode ToggleKey = KeyCode.F9;

        private const string LiveResource = "LogiCardVolumeProfile";
        private const string PhotoResource = "LogiCardVolumeProfile_Photo";

        private VolumeProfile _live;
        private VolumeProfile _photo;
        private Volume _volume;
        private bool _photoMode;

        /// <summary>True while the hero-shot profile is active.</summary>
        public bool IsPhotoMode => _photoMode;

        private void Awake()
        {
            _live = Resources.Load<VolumeProfile>(LiveResource);
            _photo = Resources.Load<VolumeProfile>(PhotoResource);
            if (_live == null || _photo == null)
            {
                Debug.LogWarning(
                    "[PhotoModeController] Missing Volume Profiles under Resources/ " +
                    $"({LiveResource} / {PhotoResource}). Run UrpPostProcessingBootstrap.");
            }
        }

        private void Start()
        {
            // GameBootstrap builds "Diorama Volume" in Awake; bind after that and replace the
            // runtime-created profile with the authored live asset so F9 has a stable swap target.
            _volume = FindDioramaVolume();
            if (_volume == null || _live == null)
            {
                return;
            }

            _volume.sharedProfile = _live;
            _photoMode = false;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(ToggleKey))
            {
                return;
            }

            Toggle();
        }

        public void Toggle()
        {
            if (_volume == null)
            {
                _volume = FindDioramaVolume();
            }

            if (_volume == null || _live == null || _photo == null)
            {
                return;
            }

            _photoMode = !_photoMode;
            _volume.sharedProfile = _photoMode ? _photo : _live;
            Debug.Log(_photoMode
                ? "[PhotoModeController] Photo mode ON (F9 to return to live grade)."
                : "[PhotoModeController] Photo mode OFF — live readability grade.");
        }

        private static Volume FindDioramaVolume()
        {
            Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                if (volumes[i] != null && volumes[i].name == "Diorama Volume")
                {
                    return volumes[i];
                }
            }

            return volumes.Length > 0 ? volumes[0] : null;
        }
    }
}
