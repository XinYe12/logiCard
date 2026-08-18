using System;
using System.Collections;
using UnityEngine;

namespace LogiCard.UI
{
    /// <summary>
    /// Tiny zero-dependency uGUI tween helper (no DOTween). Drives float / Color / Vector2
    /// over a duration with an EaseInOutCubic curve approximating CSS
    /// <c>cubic-bezier(0.4, 0, 0.2, 1)</c>. Callers host coroutines on any MonoBehaviour.
    /// </summary>
    public static class UiMotion
    {
        /// <summary>Default Character Select role crossfade (Reference 1).</summary>
        public const float DefaultDuration = 0.65f;

        /// <summary>
        /// EaseInOutCubic — close enough to <c>cubic-bezier(0.4, 0, 0.2, 1)</c> for UI crossfades.
        /// </summary>
        public static float EaseInOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow((-2f * t) + 2f, 3f) / 2f;
        }

        /// <summary>
        /// Steps <paramref name="onStep"/> with eased progress 0→1 over <paramref name="duration"/>
        /// seconds (unscaled). Invokes <paramref name="onComplete"/> once at the end.
        /// </summary>
        public static IEnumerator Animate(
            float duration,
            Action<float> onStep,
            Action onComplete = null)
        {
            if (onStep == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                onStep(1f);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                onStep(EaseInOutCubic(elapsed / duration));
                yield return null;
            }

            onStep(1f);
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateFloat(
            float from,
            float to,
            float duration,
            Action<float> apply,
            Action onComplete = null)
        {
            if (apply == null)
            {
                yield break;
            }

            yield return Animate(duration, t => apply(Mathf.LerpUnclamped(from, to, t)), onComplete);
        }

        public static IEnumerator AnimateColor(
            Color from,
            Color to,
            float duration,
            Action<Color> apply,
            Action onComplete = null)
        {
            if (apply == null)
            {
                yield break;
            }

            yield return Animate(duration, t => apply(Color.LerpUnclamped(from, to, t)), onComplete);
        }

        public static IEnumerator AnimateVector2(
            Vector2 from,
            Vector2 to,
            float duration,
            Action<Vector2> apply,
            Action onComplete = null)
        {
            if (apply == null)
            {
                yield break;
            }

            yield return Animate(duration, t => apply(Vector2.LerpUnclamped(from, to, t)), onComplete);
        }

        /// <summary>
        /// Runs several tween coroutines in parallel; completes when the longest finishes.
        /// </summary>
        public static IEnumerator Parallel(MonoBehaviour host, params IEnumerator[] routines)
        {
            if (host == null || routines == null || routines.Length == 0)
            {
                yield break;
            }

            int remaining = 0;
            for (int i = 0; i < routines.Length; i++)
            {
                if (routines[i] == null)
                {
                    continue;
                }

                remaining++;
                host.StartCoroutine(Wrap(routines[i], () => remaining--));
            }

            while (remaining > 0)
            {
                yield return null;
            }
        }

        private static IEnumerator Wrap(IEnumerator routine, Action onDone)
        {
            yield return routine;
            onDone?.Invoke();
        }
    }
}
