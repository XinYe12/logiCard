using System.Collections.Generic;
using UnityEngine;

namespace LogiCard.Audio
{
    /// <summary>
    /// Dead-code Wave 1 stub (D11 brief): synthesizes a tiny, distinct placeholder clip per
    /// <see cref="FoleyId"/> at runtime so the project ships no binary audio assets yet. Core wires
    /// callers in Wave 2 — nothing in this file calls into Boot/UI/Board/Sim.
    /// </summary>
    public sealed class FoleyPlayer : MonoBehaviour, IFoleyPlayer
    {
        private const int SampleRate = 44100;

        private AudioSource _source;
        private readonly Dictionary<FoleyId, AudioClip> _clips = new();

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }

            _source.playOnAwake = false;
        }

        public void Play(FoleyId id)
        {
            if (_source == null)
            {
                return;
            }

            AudioClip clip = GetOrBuildClip(id);
            if (clip != null)
            {
                _source.PlayOneShot(clip);
            }
        }

        private AudioClip GetOrBuildClip(FoleyId id)
        {
            if (_clips.TryGetValue(id, out AudioClip existing))
            {
                return existing;
            }

            AudioClip clip = BuildClip(id);
            _clips[id] = clip;
            return clip;
        }

        // Tactile-foley placeholders (ART_DIRECTION audio floor): clay-on-board footstep,
        // cap-gun/heavy-stapler shot, paper Time Card shuffle, Lock In switch snap. Each id gets a
        // distinct tone/noise/envelope recipe so they read apart even as raw synth tones.
        private static AudioClip BuildClip(FoleyId id)
        {
            FoleyProfile profile = ProfileFor(id);
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(profile.DurationSeconds * SampleRate));
            float[] data = new float[sampleCount];

            var rng = new System.Random((int)id * 7919 + 17);
            float attackSamples = Mathf.Max(1f, profile.AttackSeconds * SampleRate);
            float decayTau = Mathf.Max(0.005f, profile.DurationSeconds / 3f);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;

                float envelope = i < attackSamples
                    ? i / attackSamples
                    : Mathf.Exp(-(t - profile.AttackSeconds) / decayTau);

                float tone = profile.ToneHz > 0f
                    ? Mathf.Sin(2f * Mathf.PI * profile.ToneHz * t)
                    : 0f;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float texture = profile.FlutterHz > 0f
                    ? 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * profile.FlutterHz * t)
                    : 1f;

                float sample = Mathf.Lerp(tone, noise, profile.NoiseMix) * envelope * texture;
                data[i] = Mathf.Clamp(sample, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create($"Foley_{id}", sampleCount, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static FoleyProfile ProfileFor(FoleyId id)
        {
            return id switch
            {
                // Dull heavy thud — clay on plywood.
                FoleyId.Footstep => new FoleyProfile(duration: 0.16f, toneHz: 85f, noiseMix: 0.55f, attack: 0.004f, flutterHz: 0f),
                // Punchy, muffled — cap gun / heavy stapler, not cinematic.
                FoleyId.Shot => new FoleyProfile(duration: 0.08f, toneHz: 200f, noiseMix: 0.75f, attack: 0.001f, flutterHz: 0f),
                // Paper shuffle — noise-led with a flutter to fake a crinkle texture.
                FoleyId.TimeCard => new FoleyProfile(duration: 0.22f, toneHz: 0f, noiseMix: 1f, attack: 0.01f, flutterHz: 90f),
                // Physical switch snap — near-instant, high, sharp.
                FoleyId.LockIn => new FoleyProfile(duration: 0.05f, toneHz: 1400f, noiseMix: 0.3f, attack: 0.0005f, flutterHz: 0f),
                _ => new FoleyProfile(duration: 0.1f, toneHz: 440f, noiseMix: 0.5f, attack: 0.005f, flutterHz: 0f),
            };
        }

        private readonly struct FoleyProfile
        {
            public readonly float DurationSeconds;
            public readonly float ToneHz;
            public readonly float NoiseMix;
            public readonly float AttackSeconds;
            public readonly float FlutterHz;

            public FoleyProfile(float duration, float toneHz, float noiseMix, float attack, float flutterHz)
            {
                DurationSeconds = duration;
                ToneHz = toneHz;
                NoiseMix = noiseMix;
                AttackSeconds = attack;
                FlutterHz = flutterHz;
            }
        }
    }
}
