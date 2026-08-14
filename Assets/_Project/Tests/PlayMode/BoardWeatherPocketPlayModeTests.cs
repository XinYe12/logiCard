using System.Collections;
using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// Smoke: WeatherPocket binds + applies a modular weather child (Storm by bootstrap default)
    /// with centered clay CloudBank + rim mist — not a full-board fog slab.
    /// </summary>
    [TestFixture]
    public sealed class BoardWeatherPocketPlayModeTests : SliceSceneFixture
    {
        [UnityTest]
        public IEnumerator WeatherPocketBuildsCloudBankAndRimMistWithoutThrow()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null, "Bootstrap built no BoardWeatherPocket.");
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Storm),
                "Bootstrap should mount the Storm module for the current look pass.");

            Transform module = weather.transform.Find("Weather_Storm");
            Assert.That(module, Is.Not.Null, "Expected Weather_Storm module child (card-swappable).");

            Transform cloudBank = module.Find("CloudBank");
            Assert.That(cloudBank, Is.Not.Null, "Expected CloudBank under Weather_Storm.");
            Assert.That(cloudBank.childCount, Is.GreaterThanOrEqualTo(4),
                "Clay cloud bank should place several coherent masses.");

            var meshRenderers = cloudBank.GetComponentsInChildren<MeshRenderer>(true);
            Assert.That(meshRenderers.Length, Is.GreaterThanOrEqualTo(12),
                "Expected multiple clay lobe meshes under CloudBank.");
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                Assert.That(meshRenderers[i].sharedMaterial, Is.Not.Null,
                    $"Clay lobe {meshRenderers[i].name} missing material.");
            }

            Transform rimMist = module.Find("RimMist");
            Assert.That(rimMist, Is.Not.Null, "Expected RimMist under Weather_Storm.");
            Assert.That(rimMist.childCount, Is.GreaterThanOrEqualTo(2),
                "Rim mist should place at least corner apron haze.");

            Assert.That(weather.transform.Find("FogGround"), Is.Null);
            Transform lightningStorm = module.Find("LightningStorm");
            Assert.That(lightningStorm, Is.Not.Null,
                "Storm module should include LightningStorm (Yellow Zap).");
            Assert.That(lightningStorm.childCount, Is.GreaterThanOrEqualTo(1),
                "Expected at least one Zap rig under LightningStorm.");

            Transform cloudEnergize = module.Find("CloudEnergize");
            Assert.That(cloudEnergize, Is.Not.Null,
                "Storm module should wrap tiny yellow Zap arcs around the clay (CloudEnergize).");
            Assert.That(cloudEnergize.childCount, Is.GreaterThanOrEqualTo(6),
                "Expected rim-chained energize arcs under CloudEnergize (Layer-2 envelope wrap).");

            // Arcs should sit near the bank exterior, not deep in the volume core.
            Bounds cloudBounds = meshRenderers[0].bounds;
            for (int i = 1; i < meshRenderers.Length; i++)
            {
                cloudBounds.Encapsulate(meshRenderers[i].bounds);
            }

            int rimish = 0;
            for (int i = 0; i < cloudEnergize.childCount; i++)
            {
                Vector3 p = cloudEnergize.GetChild(i).position;
                Vector3 flat = p - cloudBounds.center;
                flat.y = 0f;
                float radial = new Vector2(flat.x / Mathf.Max(0.01f, cloudBounds.extents.x),
                    flat.z / Mathf.Max(0.01f, cloudBounds.extents.z)).magnitude;
                if (radial > 0.55f)
                {
                    rimish++;
                }
            }

            Assert.That(rimish, Is.GreaterThanOrEqualTo(cloudEnergize.childCount / 2),
                "Most energize arcs should lie on the exterior envelope rim, not the core.");

            // Bolt height tracks cloud shelf: ConeVolume length ≈ ground→cloud-center rise.
            // (reuse cloudBounds from above)

            const float footprintMargin = 1.5f;
            for (int i = 0; i < lightningStorm.childCount; i++)
            {
                Transform zap = lightningStorm.GetChild(i);
                Assert.That(zap.position.x, Is.InRange(cloudBounds.min.x - footprintMargin, cloudBounds.max.x + footprintMargin),
                    $"{zap.name} X should be under the cloud bank's footprint.");
                Assert.That(zap.position.z, Is.InRange(cloudBounds.min.z - footprintMargin, cloudBounds.max.z + footprintMargin),
                    $"{zap.name} Z should be under the cloud bank's footprint.");
                Assert.That(zap.position.y, Is.LessThan(cloudBounds.min.y),
                    $"{zap.name} should spawn near the ground.");
                Assert.That(Vector3.Dot(zap.up, Vector3.up), Is.GreaterThan(0.9f),
                    $"{zap.name} should stay upright.");
                Assert.That(zap.localScale, Is.EqualTo(Vector3.one),
                    $"{zap.name} should stay scale 1 — height comes from ConeVolume length, not transform scale.");

                float cloudRise = cloudBounds.center.y - zap.position.y;
                var systems = zap.GetComponentsInChildren<ParticleSystem>(true);
                int fitted = 0;
                for (int c = 0; c < systems.Length; c++)
                {
                    ParticleSystem.ShapeModule shape = systems[c].shape;
                    if (!shape.enabled || shape.shapeType != ParticleSystemShapeType.ConeVolume)
                    {
                        continue;
                    }

                    fitted++;
                    Assert.That(shape.length, Is.EqualTo(cloudRise).Within(cloudBounds.size.y + 0.75f),
                        $"{zap.name}/{systems[c].name} ConeVolume length {shape.length:F2} should track cloud rise ~{cloudRise:F2}.");
                    Assert.That(shape.length, Is.Not.EqualTo(5f).Within(0.05f),
                        $"{zap.name}/{systems[c].name} must not keep the prefab's fixed length 5.");
                }

                Assert.That(fitted, Is.GreaterThanOrEqualTo(1),
                    $"{zap.name} should expose ConeVolume bolt layers fitted to cloud height.");
            }

            yield return null;
        }
    }
}
