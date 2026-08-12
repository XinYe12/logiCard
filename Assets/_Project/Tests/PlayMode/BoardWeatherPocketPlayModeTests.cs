using System.Collections;
using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Smoke: WeatherPocket builds without throw — clay sphere CloudBank + sparse rim mist
    /// (not a full-board fog slab). Atmosphere pass 2026-08-12 / image copy 13.
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
            Assert.That(cloudBank.childCount, Is.GreaterThanOrEqualTo(4),
                "Clay cloud bank should place several coherent masses.");

            // Opaque sphere lobes (LA clay) — MeshRenderers, not camera-facing billboards.
            var meshRenderers = cloudBank.GetComponentsInChildren<MeshRenderer>(true);
            Assert.That(meshRenderers.Length, Is.GreaterThanOrEqualTo(12),
                "Expected multiple clay lobe meshes under CloudBank.");
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                Assert.That(meshRenderers[i].sharedMaterial, Is.Not.Null,
                    $"Clay lobe {meshRenderers[i].name} missing material.");
            }

            Transform rimMist = weather.transform.Find("RimMist");
            Assert.That(rimMist, Is.Not.Null, "Expected RimMist child (sparse apron mist).");
            Assert.That(rimMist.childCount, Is.GreaterThanOrEqualTo(2),
                "Rim mist should place at least corner apron haze.");

            // No full-board FogGround / RainMist volumes from the prior pack wire.
            Assert.That(weather.transform.Find("FogGround"), Is.Null);
            Assert.That(weather.transform.Find("RainMist"), Is.Null);

            yield return null;
        }
    }
}
