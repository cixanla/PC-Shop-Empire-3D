using System;
using System.Collections;
using System.IO;
using System.Linq;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string AssemblyWorkbenchHeroReadabilitySmokeSuccessMarker =
            "GARAGE_ASSEMBLY_WORKBENCH_HERO_READABILITY_RUNTIME_SMOKE " +
            "states=loose+preview+routed hero=ready " +
            "materials=wood+rubber+dark-metal+brushed-steel+concrete+pcb+" +
            "safety-accent+connector-polymer+psu-intake+gpu-hardware " +
            "connector-glare=bounded " +
            "light=focused total-renderers=493 lights=4 cameras=1 " +
            "screenshots=3 glare-pixels<=64 " +
            "ui=lookdev-suppressed human=false";

        private const string AssemblyWorkbenchCaptureDirectoryArgument =
            "-pse-assembly-workbench-capture-directory=";
        private const int AssemblyWorkbenchMaximumCentralGlarePixels = 64;
        private const byte AssemblyWorkbenchGlareChannelThreshold = 250;

        private IEnumerator RunAssemblyWorkbenchHeroReadabilitySmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode = null;
            _suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunPcieGpuPowerCableAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode;
            _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                    $"smoke.pcie-gpu-prerequisite-{prerequisiteFailure}");
                yield break;
            }

            Transform heroRoot = FindSceneTransform(
                "AssemblyWorkbenchHeroReadability");
            Light taskLight = FindSceneLight("WorkbenchTaskLight");
            Renderer esdMat = FindSceneRenderer("AssemblyWorkbenchEsdMat");
            Renderer splashback = FindSceneRenderer(
                "AssemblyWorkbenchSplashback");
            Renderer zoneAccent = FindSceneRenderer(
                "AssemblyWorkbenchZoneAccent");
            Renderer routeReference = FindSceneRenderer(
                "AssemblyCableRouteReferenceStrip");
            Renderer pcieSixPinHousing = FindSceneRenderer(
                "PcieGpuGraphicsCardGpu8ConnectorSixPinHousing");
            Renderer pcieTwoPinHousing = FindSceneRenderer(
                "PcieGpuGraphicsCardGpu8ConnectorTwoPinHousing");
            Renderer powerSupplyFloorIntake = FindSceneRenderer(
                "PowerSupplyFilteredFloorIntake");
            Renderer graphicsCardFanBlade = FindSceneRenderer(
                "GraphicsCardFan1Blade_6");
            Renderer graphicsCardRearBracket = FindSceneRenderer(
                "GraphicsCardRearBracketPlate");
            Renderer graphicsCardIoBracket = FindSceneRenderer(
                "GraphicsCardIoRearBracket");
            int meshRendererCount = FindObjectsByType<MeshRenderer>(
                FindObjectsSortMode.None).Length;
            int totalMeshRendererCount = gameObject.scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<MeshRenderer>(true))
                .Count();
            int lightCount = FindObjectsByType<Light>(
                FindObjectsSortMode.None).Length;
            int cameraCount = FindObjectsByType<Camera>(
                FindObjectsSortMode.None).Length;

            if (heroRoot == null ||
                heroRoot.GetComponentsInChildren<Renderer>(true).Length != 4 ||
                heroRoot.GetComponentsInChildren<Collider>(true).Length != 0 ||
                taskLight == null ||
                esdMat == null ||
                splashback == null ||
                zoneAccent == null ||
                routeReference == null ||
                pcieSixPinHousing == null ||
                pcieTwoPinHousing == null ||
                powerSupplyFloorIntake == null ||
                graphicsCardFanBlade == null ||
                graphicsCardRearBracket == null ||
                graphicsCardIoBracket == null ||
                !MaterialNameStartsWith(esdMat, "WorkshopRubber") ||
                !MaterialNameStartsWith(splashback, "Concrete") ||
                !MaterialNameStartsWith(zoneAccent, "SafetyAccent") ||
                !MaterialNameStartsWith(routeReference, "SafetyAccent") ||
                !MaterialNameStartsWith(
                    pcieSixPinHousing,
                    "CableConnectorPolymer") ||
                !MaterialNameStartsWith(
                    pcieTwoPinHousing,
                    "CableConnectorPolymer") ||
                !IsMatteConnectorPolymer(pcieSixPinHousing.sharedMaterial) ||
                !IsMatteConnectorPolymer(pcieTwoPinHousing.sharedMaterial) ||
                !MaterialNameStartsWith(
                    powerSupplyFloorIntake,
                    "CableConnectorPolymer") ||
                !IsMatteConnectorPolymer(
                    powerSupplyFloorIntake.sharedMaterial) ||
                !MaterialNameStartsWith(
                    graphicsCardFanBlade,
                    "CableConnectorPolymer") ||
                !IsMatteConnectorPolymer(
                    graphicsCardFanBlade.sharedMaterial) ||
                !MaterialNameStartsWith(
                    graphicsCardRearBracket,
                    "WorkshopMatteHardware") ||
                !MaterialNameStartsWith(
                    graphicsCardIoBracket,
                    "WorkshopMatteHardware") ||
                !IsGlareSafeHardware(
                    graphicsCardRearBracket.sharedMaterial) ||
                !IsGlareSafeHardware(
                    graphicsCardIoBracket.sharedMaterial) ||
                !Mathf.Approximately(taskLight.intensity, 0.4f) ||
                !Mathf.Approximately(taskLight.range, 2.8f) ||
                !Mathf.Approximately(taskLight.spotAngle, 62f) ||
                totalMeshRendererCount != 493 ||
                lightCount != 4 ||
                cameraCount != 1)
            {
                LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                    "smoke.hero-contract-or-budget-mismatch-" +
                    $"active-renderers-{meshRendererCount}-" +
                    $"total-renderers-{totalMeshRendererCount}-" +
                    $"lights-{lightCount}-cameras-{cameraCount}");
                yield break;
            }

            if (playerMotor == null ||
                playerCarry == null ||
                pcieGpuPowerCableBinding == null ||
                pcieGpuPowerCableGeometry == null ||
                pcieGpuPowerCableRoute == null ||
                playerCarry.HeldItem != pcieGpuPowerCable ||
                !pcieGpuPowerCableBinding.IsAuthorityInHands ||
                pcieGpuPowerCableBinding.IsRouted ||
                pcieGpuPowerCableGeometry.RoutedTrunk.enabled ||
                !pcieGpuPowerCableGeometry.LooseCoil.enabled ||
                stockFlow.Session.AssemblyBuild.PcieGpuPowerCableState !=
                    PcieGpuPowerCableState.Loose)
            {
                LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                    "smoke.loose-state-mismatch");
                yield break;
            }

            string captureDirectory;
            try
            {
                captureDirectory = ResolveAssemblyWorkbenchCaptureDirectory();
                Directory.CreateDirectory(captureDirectory);
            }
            catch (Exception exception)
            {
                LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                    $"smoke.capture-directory-failed-{exception.GetType().Name}");
                yield break;
            }

            SuppressLookdevCaptureUi();
            SetAssemblyWorkbenchHeroCapturePose();
            LogAssemblyWorkbenchHeroCaptureComposition();
            yield return CaptureAssemblyWorkbenchHeroFrame(
                captureDirectory,
                "assembly-workbench-hero-loose-r55.png");

            MovePlayerToPcieGpuPowerCableRoute();
            OperationResult preview = playerCarry
                .TrySetPcieGpuPowerCableRouteMode(true);
            if (preview.IsFailure ||
                !playerCarry.IsPcieGpuPowerCableRouteMode ||
                !pcieGpuPowerCableRoute.PreviewLine.enabled)
            {
                LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                    "smoke.preview-state-mismatch");
                yield break;
            }

            SetAssemblyWorkbenchHeroCapturePose();
            yield return CaptureAssemblyWorkbenchHeroFrame(
                captureDirectory,
                "assembly-workbench-hero-preview-r55.png");

            MovePlayerToPcieGpuPowerCableRoute();
            OperationResult routed = playerCarry
                .TryConfirmPcieGpuPowerCableRoute();
            if (routed.IsFailure ||
                playerCarry.HeldItem != null ||
                playerCarry.IsPcieGpuPowerCableRouteMode ||
                !pcieGpuPowerCableBinding.IsRouted ||
                !pcieGpuPowerCableGeometry.RoutedTrunk.enabled ||
                pcieGpuPowerCableGeometry.LooseCoil.enabled ||
                stockFlow.Session.AssemblyBuild.PcieGpuPowerCableState !=
                    PcieGpuPowerCableState.Routed)
            {
                LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                    "smoke.routed-state-mismatch");
                yield break;
            }

            SetAssemblyWorkbenchHeroCapturePose();
            yield return CaptureAssemblyWorkbenchHeroFrame(
                captureDirectory,
                "assembly-workbench-hero-routed-r55.png");

            string[] expectedScreenshots =
            {
                "assembly-workbench-hero-loose-r55.png",
                "assembly-workbench-hero-preview-r55.png",
                "assembly-workbench-hero-routed-r55.png"
            };
            int maximumCentralGlarePixels = 0;
            foreach (string screenshot in expectedScreenshots)
            {
                string path = Path.Combine(captureDirectory, screenshot);
                if (!File.Exists(path) || new FileInfo(path).Length <= 0)
                {
                    LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                        $"smoke.capture-missing-{screenshot}");
                    yield break;
                }

                if (!TryCountAssemblyWorkbenchCentralGlarePixels(
                        path,
                        out int centralGlarePixels,
                        out string glareReadFailure))
                {
                    LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                        $"smoke.capture-glare-read-failed-{screenshot}-" +
                        glareReadFailure);
                    yield break;
                }

                maximumCentralGlarePixels = Mathf.Max(
                    maximumCentralGlarePixels,
                    centralGlarePixels);
                if (centralGlarePixels >
                    AssemblyWorkbenchMaximumCentralGlarePixels)
                {
                    LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
                        $"smoke.capture-glare-budget-exceeded-{screenshot}-" +
                        $"pixels-{centralGlarePixels}-maximum-" +
                        AssemblyWorkbenchMaximumCentralGlarePixels);
                    yield break;
                }
            }

            Debug.Log(
                $"{AssemblyWorkbenchHeroReadabilitySmokeSuccessMarker} " +
                $"active-renderers={meshRendererCount} " +
                $"max-central-glare-pixels={maximumCentralGlarePixels} " +
                $"capture-directory={captureDirectory}");
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private void SetAssemblyWorkbenchHeroCapturePose()
        {
            Vector3 playerPosition = new Vector3(-2.60f, 0.35f, 3.00f);
            Vector3 target = new Vector3(-0.68f, 1.30f, 4.28f);
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera camera = playerMotor.GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                camera.fieldOfView = 58f;
                camera.transform.rotation = Quaternion.LookRotation(
                    target - camera.transform.position,
                    Vector3.up);
            }

            playerMotor.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Physics.SyncTransforms();
        }

        private static void SuppressLookdevCaptureUi()
        {
            GaragePrototypeHud hud = FindFirstObjectByType<GaragePrototypeHud>();
            if (hud != null)
            {
                hud.enabled = false;
            }

            foreach (TextMesh text in FindObjectsByType<TextMesh>(
                         FindObjectsSortMode.None))
            {
                Renderer renderer = text.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        private static IEnumerator CaptureAssemblyWorkbenchHeroFrame(
            string directory,
            string fileName)
        {
            string path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path, 1);
            yield return new WaitForSecondsRealtime(0.45f);
        }

        private static bool TryCountAssemblyWorkbenchCentralGlarePixels(
            string path,
            out int glarePixels,
            out string failure)
        {
            glarePixels = 0;
            failure = null;
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(
                    2,
                    2,
                    TextureFormat.RGBA32,
                    false,
                    false);
                if (!ImageConversion.LoadImage(
                        texture,
                        File.ReadAllBytes(path),
                        false))
                {
                    failure = "decode-failed";
                    return false;
                }

                Color32[] pixels = texture.GetPixels32();
                int xMinimum = Mathf.Clamp(
                    Mathf.FloorToInt(texture.width * (400f / 1280f)),
                    0,
                    texture.width - 1);
                int xMaximum = Mathf.Clamp(
                    Mathf.CeilToInt(texture.width * (850f / 1280f)),
                    xMinimum + 1,
                    texture.width);
                int yMinimum = Mathf.Clamp(
                    Mathf.FloorToInt(texture.height * (120f / 720f)),
                    0,
                    texture.height - 1);
                int yMaximum = Mathf.Clamp(
                    Mathf.CeilToInt(texture.height * (470f / 720f)),
                    yMinimum + 1,
                    texture.height);

                for (int y = yMinimum; y < yMaximum; y++)
                {
                    int rowOffset = y * texture.width;
                    for (int x = xMinimum; x < xMaximum; x++)
                    {
                        Color32 pixel = pixels[rowOffset + x];
                        if (pixel.r > AssemblyWorkbenchGlareChannelThreshold &&
                            pixel.g > AssemblyWorkbenchGlareChannelThreshold &&
                            pixel.b > AssemblyWorkbenchGlareChannelThreshold)
                        {
                            glarePixels++;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                failure = exception.GetType().Name;
                return false;
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
        }

        private void LogAssemblyWorkbenchHeroCaptureComposition()
        {
            Camera camera = playerMotor != null
                ? playerMotor.GetComponentInChildren<Camera>(true)
                : null;
            if (camera == null)
            {
                return;
            }

            string composition = string.Join(
                " | ",
                FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                    .Where(renderer => renderer.enabled)
                    .Select(renderer => new
                    {
                        Renderer = renderer,
                        Area = ProjectedViewportArea(camera, renderer.bounds)
                    })
                    .Where(entry => entry.Area > 0.01f)
                    .OrderByDescending(entry => entry.Area)
                    .Take(12)
                    .Select(entry =>
                        $"{entry.Renderer.name}:" +
                        $"{entry.Renderer.sharedMaterial?.name ?? "none"}:" +
                        $"{entry.Area:F3}"));
            Debug.Log(
                "GARAGE_ASSEMBLY_WORKBENCH_HERO_CAPTURE_COMPOSITION " +
                composition);
        }

        private static float ProjectedViewportArea(
            Camera camera,
            Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            bool hasPointInFront = false;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 viewport = camera.WorldToViewportPoint(
                            center + Vector3.Scale(
                                extents,
                                new Vector3(x, y, z)));
                        if (viewport.z <= 0f)
                        {
                            continue;
                        }

                        hasPointInFront = true;
                        minX = Mathf.Min(minX, viewport.x);
                        minY = Mathf.Min(minY, viewport.y);
                        maxX = Mathf.Max(maxX, viewport.x);
                        maxY = Mathf.Max(maxY, viewport.y);
                    }
                }
            }

            if (!hasPointInFront)
            {
                return 0f;
            }

            minX = Mathf.Clamp01(minX);
            minY = Mathf.Clamp01(minY);
            maxX = Mathf.Clamp01(maxX);
            maxY = Mathf.Clamp01(maxY);
            return Mathf.Max(0f, maxX - minX) *
                   Mathf.Max(0f, maxY - minY);
        }

        private static string ResolveAssemblyWorkbenchCaptureDirectory()
        {
            foreach (string argument in Environment.GetCommandLineArgs())
            {
                if (argument.StartsWith(
                        AssemblyWorkbenchCaptureDirectoryArgument,
                        StringComparison.Ordinal))
                {
                    string requested = argument.Substring(
                        AssemblyWorkbenchCaptureDirectoryArgument.Length);
                    if (string.IsNullOrWhiteSpace(requested))
                    {
                        throw new InvalidOperationException(
                            "The assembly workbench capture directory is empty.");
                    }

                    return Path.GetFullPath(requested);
                }
            }

            return Path.Combine(
                Application.persistentDataPath,
                "AssemblyWorkbenchHeroEvidence");
        }

        private static bool IsMatteConnectorPolymer(Material material)
        {
            return material != null &&
                   material.shader != null &&
                   material.shader.name ==
                   "Universal Render Pipeline/Unlit" &&
                   material.HasProperty("_BaseColor") &&
                   material.GetColor("_BaseColor").maxColorComponent <= 0.031f &&
                   !material.IsKeywordEnabled("_EMISSION");
        }

        private static bool IsGlareSafeHardware(Material material)
        {
            return material != null &&
                   material.shader != null &&
                   material.shader.name ==
                   "Universal Render Pipeline/Unlit" &&
                   material.HasProperty("_BaseColor") &&
                   material.GetColor("_BaseColor").maxColorComponent <= 0.201f &&
                   !material.IsKeywordEnabled("_EMISSION");
        }

        private static Transform FindSceneTransform(string name)
        {
            foreach (Transform candidate in FindObjectsByType<Transform>(
                         FindObjectsSortMode.None))
            {
                if (candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Renderer FindSceneRenderer(string name)
        {
            Transform transform = FindSceneTransform(name);
            return transform != null ? transform.GetComponent<Renderer>() : null;
        }

        private static Light FindSceneLight(string name)
        {
            Transform transform = FindSceneTransform(name);
            return transform != null ? transform.GetComponent<Light>() : null;
        }

        private static bool MaterialNameStartsWith(
            Renderer renderer,
            string expectedPrefix)
        {
            return renderer != null &&
                   renderer.sharedMaterial != null &&
                   renderer.sharedMaterial.name.StartsWith(
                       expectedPrefix,
                       StringComparison.Ordinal);
        }

        private void LogAssemblyWorkbenchHeroReadabilitySmokeFailure(
            string code)
        {
            Debug.LogError(
                "GARAGE_ASSEMBLY_WORKBENCH_HERO_READABILITY_RUNTIME_SMOKE " +
                $"hero-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
