using System.Collections;
using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Smoke: WeatherPocket builds without throw and exposes CloudAtlas bank + rim mist (not a
    /// full-board fog slab). Atmosphere stylized pass 2026-08-12.
    /// </summary>
    [TestFixture]
    public sealed class BoardWeatherPocketPlayModeTests : SliceSceneFixture
    {
        [UnityTest]
        public IEnumerator WeatherPocketBuildsCloudBankAndRimMistWithoutThrow()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null, "Bootstrap built no BoardWeatherPocket.");

            Transform cloudBank = weather.transform.Find("CloudBank");
            Assert.That(cloudBank, Is.Not.Null, "Expected CloudBank child.");
            Assert.That(cloudBank.childCount, Is.GreaterThanOrEqualTo(6),
                "CloudAtlas bank should place multiple puffs.");

            Transform rimMist = weather.transform.Find("RimMist");
            Assert.That(rimMist, Is.Not.Null, "Expected RimMist child (rim-only Kenney mist).");
            Assert.That(rimMist.childCount, Is.GreaterThanOrEqualTo(4),
                "Rim mist should place edge pockets.");

            // No full-board FogGround / RainMist volumes from the prior pack wire.
            Assert.That(weather.transform.Find("FogGround"), Is.Null);
            Assert.That(weather.transform.Find("RainMist"), Is.Null);

            Texture2D atlas = Resources.Load<Texture2D>("Weather/CloudAtlas");
            Assert.That(atlas, Is.Not.Null, "CloudAtlas must live under Resources/Weather.");

            // Cloud puff renderers must use textured materials (not default white squares).
            var renderers = cloudBank.GetComponentsInChildren<ParticleSystemRenderer>(true);
            Assert.That(renderers.Length, Is.GreaterThan(0));
            for (int i = 0; i < renderers.Length; i++)
            {
                Material mat = renderers[i].sharedMaterial;
                Assert.That(mat, Is.Not.Null, $"Cloud puff {renderers[i].name} missing material.");
                Texture tex = mat.mainTexture
                    ?? (mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null);
                Assert.That(tex, Is.Not.Null,
                    $"Cloud puff {renderers[i].name} material has no CloudAtlas texture.");
            }

            yield return null;
        }
    }
}
