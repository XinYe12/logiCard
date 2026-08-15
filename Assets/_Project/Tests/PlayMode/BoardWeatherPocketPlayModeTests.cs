using System.Collections;
using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LogiCard.Tests.PlayMode
{
    /// <summary>
    /// WeatherPocket modular moods — Sunny is the bootstrap look pass; Storm stays swappable.
    /// </summary>
    [TestFixture]
    public sealed class BoardWeatherPocketPlayModeTests : SliceSceneFixture
    {
        [UnityTest]
        public IEnumerator WeatherPocketDefaultsToSunnyCloudless()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null, "Bootstrap built no BoardWeatherPocket.");
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Sunny),
                "Bootstrap should mount Sunny (Sunshine / 万里无云) for the current look pass.");

            Transform module = weather.transform.Find("Weather_Sunny");
            Assert.That(module, Is.Not.Null, "Expected Weather_Sunny module child (card-swappable).");
            Assert.That(module.Find("SunnySky"), Is.Not.Null, "Sunny module should expose SunnySky marker.");
            Assert.That(module.Find("CloudBank"), Is.Null, "Sunny must be cloudless — no CloudBank.");
            Assert.That(module.Find("LightningStorm"), Is.Null, "Sunny must not spawn LightningStorm.");
            Assert.That(module.Find("CloudEnergize"), Is.Null, "Sunny must not spawn CloudEnergize.");
            Assert.That(module.Find("RimMist"), Is.Null, "Sunny must not spawn RimMist.");

            // Mood lighting: clear-sky ambient lift (Atmosphere-owned override).
            Assert.That(RenderSettings.ambientLight.g, Is.GreaterThan(0.7f),
                "Sunny ambient should be punched open (not desk-lamp dim).");

            Transform sunnySky = module.Find("SunnySky");
            Assert.That(sunnySky, Is.Not.Null, "Sunny module should expose SunnySky rig.");
            Assert.That(sunnySky.Find("SunnySun"), Is.Not.Null, "Expected mood-owned SunnySun directional.");
            Assert.That(sunnySky.Find("SunnySkyFill"), Is.Not.Null, "Expected mood-owned SunnySkyFill.");
            var sun = sunnySky.Find("SunnySun").GetComponent<Light>();
            Assert.That(sun, Is.Not.Null);
            Assert.That(sun.enabled, Is.True);
            Assert.That(sun.intensity, Is.GreaterThan(2f), "SunnySun should punch harder than desk-lamp Key.");

            Assert.That(weather.transform.Find("WeatherToggleUi"), Is.Not.Null,
                "Look-pass Sunny/Storm toggle should live on the weather host.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator WeatherToggleFlipsSunnyAndStorm()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null);
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Sunny));

            weather.ToggleSunnyStorm();
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Storm));
            Assert.That(weather.transform.Find("Weather_Storm"), Is.Not.Null);
            Assert.That(weather.transform.Find("Weather_Storm/CloudBank"), Is.Not.Null);

            var drifts = weather.GetComponentsInChildren<MonoBehaviour>(true);
            int driftCount = 0;
            for (int i = 0; i < drifts.Length; i++)
            {
                if (drifts[i] != null && drifts[i].GetType().Name == "ClayCloudDrift")
                {
                    driftCount++;
                }
            }

            Assert.That(driftCount, Is.GreaterThanOrEqualTo(4),
                "Storm CloudBank masses should carry ClayCloudDrift (Phase A motion).");

            weather.ToggleSunnyStorm();
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Sunny));
            Assert.That(weather.transform.Find("Weather_Sunny"), Is.Not.Null);
            Assert.That(weather.transform.Find("WeatherToggleUi"), Is.Not.Null,
                "Toggle UI must survive ClearWeather module teardown.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator WeatherPocketStormModuleStillBuildsWhenApplied()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null, "Bootstrap built no BoardWeatherPocket.");

            weather.ApplyWeather(BoardWeatherMood.Storm);
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Storm));

            Transform module = weather.transform.Find("Weather_Storm");
            Assert.That(module, Is.Not.Null, "Expected Weather_Storm module child (card-swappable).");

            Transform cloudBank = module.Find("CloudBank");
            Assert.That(cloudBank, Is.Not.Null, "Expected CloudBank under Weather_Storm.");
            Assert.That(cloudBank.childCount, Is.GreaterThanOrEqualTo(4),
                "Clay cloud bank should place several coherent masses.");

            var meshRenderers = cloudBank.GetComponentsInChildren<MeshRenderer>(true);
            Assert.That(meshRenderers.Length, Is.GreaterThanOrEqualTo(12),
                "Expected multiple clay lobe meshes under CloudBank.");

            Transform rimMist = module.Find("RimMist");
            Assert.That(rimMist, Is.Not.Null, "Expected RimMist under Weather_Storm.");

            Transform lightningStorm = module.Find("LightningStorm");
            Assert.That(lightningStorm, Is.Not.Null,
                "Storm module should include LightningStorm (Yellow Zap).");
            Assert.That(lightningStorm.childCount, Is.GreaterThanOrEqualTo(1),
                "Expected at least one Zap rig under LightningStorm.");

            Transform cloudEnergize = module.Find("CloudEnergize");
            Assert.That(cloudEnergize, Is.Not.Null,
                "Storm module should wrap tiny yellow Zap arcs around the clay (CloudEnergize).");
            Assert.That(cloudEnergize.childCount, Is.GreaterThanOrEqualTo(6),
                "Expected rim-chained energize arcs under CloudEnergize.");

            Bounds cloudBounds = meshRenderers[0].bounds;
            for (int i = 1; i < meshRenderers.Length; i++)
            {
                cloudBounds.Encapsulate(meshRenderers[i].bounds);
            }

            const float footprintMargin = 1.5f;
            Transform zap = lightningStorm.GetChild(0);
            Assert.That(zap.position.x, Is.InRange(cloudBounds.min.x - footprintMargin, cloudBounds.max.x + footprintMargin));
            Assert.That(zap.position.z, Is.InRange(cloudBounds.min.z - footprintMargin, cloudBounds.max.z + footprintMargin));
            Assert.That(zap.position.y, Is.LessThan(cloudBounds.min.y));
            Assert.That(Vector3.Dot(zap.up, Vector3.up), Is.GreaterThan(0.9f));
            Assert.That(zap.localScale, Is.EqualTo(Vector3.one));

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
                Assert.That(shape.length, Is.EqualTo(cloudRise).Within(cloudBounds.size.y + 0.75f));
                Assert.That(shape.length, Is.Not.EqualTo(5f).Within(0.05f));
            }

            Assert.That(fitted, Is.GreaterThanOrEqualTo(1),
                $"{zap.name} should expose ConeVolume bolt layers fitted to cloud height.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator WeatherPocketFairModuleHasNoLightning()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null, "Bootstrap built no BoardWeatherPocket.");

            weather.ApplyWeather(BoardWeatherMood.Fair);
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Fair));

            Transform module = weather.transform.Find("Weather_Fair");
            Assert.That(module, Is.Not.Null, "Expected Weather_Fair module child (card-swappable).");
            Assert.That(module.Find("Lightning"), Is.Null,
                "Fair must not spawn a white Lightning rig — thunder/lightning is a Storm-only signal.");
            Assert.That(module.Find("LightningStorm"), Is.Null,
                "Fair must not spawn LightningStorm.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ApplyWeatherSameMoodKeepsCloudBankInstance()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null);

            weather.ApplyWeather(BoardWeatherMood.Storm);
            Transform cloudBank = weather.transform.Find("Weather_Storm/CloudBank");
            Assert.That(cloudBank, Is.Not.Null, "Expected CloudBank after first Storm apply.");

            weather.ApplyWeather(BoardWeatherMood.Storm);
            Transform again = weather.transform.Find("Weather_Storm/CloudBank");
            Assert.That(again, Is.Not.Null, "Same-mood ApplyWeather must leave the Storm module standing.");
            Assert.That(again == cloudBank, Is.True,
                "Same-mood ApplyWeather must early-out — CloudBank must not be destroyed/recreated.");
            Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Storm));

            yield return null;
        }

        [UnityTest]
        public IEnumerator FairStormLightingRoundTripsAcrossRepeatedCycles()
        {
            var weather = Object.FindAnyObjectByType<BoardWeatherPocket>();
            Assert.That(weather, Is.Not.Null);

            // Fair = desk-lamp baseline (no mood override). Capture Key + ambient as the restore target.
            weather.ApplyWeather(BoardWeatherMood.Fair);
            Light key = FindBootstrapKeyLight();
            Assert.That(key, Is.Not.Null, "Expected bootstrap Storm Key directional for lighting round-trip.");
            Assert.That(key.enabled, Is.True);

            float fairIntensity = key.intensity;
            Color fairColor = key.color;
            Color fairAmbient = RenderSettings.ambientLight;

            for (int cycle = 0; cycle < 3; cycle++)
            {
                weather.ApplyWeather(BoardWeatherMood.Storm);
                Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Storm));
                Assert.That(RenderSettings.ambientLight.r, Is.LessThan(fairAmbient.r - 0.05f),
                    $"Cycle {cycle}: Storm ambient should dim below Fair baseline.");
                Assert.That(key.intensity, Is.LessThan(fairIntensity * 0.95f),
                    $"Cycle {cycle}: Storm should dim the Key below Fair intensity.");

                weather.ApplyWeather(BoardWeatherMood.Fair);
                Assert.That(weather.ActiveMood, Is.EqualTo(BoardWeatherMood.Fair));
                Assert.That(key.enabled, Is.True, $"Cycle {cycle}: Key must be re-enabled after Fair restore.");
                Assert.That(key.intensity, Is.EqualTo(fairIntensity).Within(0.001f),
                    $"Cycle {cycle}: Key intensity must restore to Fair baseline after Storm.");
                Assert.That(key.color.r, Is.EqualTo(fairColor.r).Within(0.001f));
                Assert.That(key.color.g, Is.EqualTo(fairColor.g).Within(0.001f));
                Assert.That(key.color.b, Is.EqualTo(fairColor.b).Within(0.001f));
                Assert.That(RenderSettings.ambientLight.r, Is.EqualTo(fairAmbient.r).Within(0.001f));
                Assert.That(RenderSettings.ambientLight.g, Is.EqualTo(fairAmbient.g).Within(0.001f));
                Assert.That(RenderSettings.ambientLight.b, Is.EqualTo(fairAmbient.b).Within(0.001f));
            }

            yield return null;
        }

        private static Light FindBootstrapKeyLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light != null && light.type == LightType.Directional && light.name.Contains("Key"))
                {
                    return light;
                }
            }

            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light != null && light.type == LightType.Directional)
                {
                    return light;
                }
            }

            return null;
        }
    }
}
