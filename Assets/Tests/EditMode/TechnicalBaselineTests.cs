using System.IO;
using NUnit.Framework;
using PCShopEmpire3D.Presentation;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace PCShopEmpire3D.Tests.EditMode
{
    public sealed class TechnicalBaselineTests
    {
        [Test]
        public void AssetSerializationUsesForceText()
        {
            Assert.That(EditorSettings.serializationMode, Is.EqualTo(SerializationMode.ForceText));
        }

        [Test]
        public void MetaFilesAreVisible()
        {
            Assert.That(VersionControlSettings.mode, Is.EqualTo("Visible Meta Files"));
        }

        [Test]
        public void UrpAssetIsConfigured()
        {
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.Not.Null);
        }

        [Test]
        public void PackageLockExists()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            Assert.That(File.Exists(Path.Combine(projectRoot, "Packages", "packages-lock.json")), Is.True);
        }

        [Test]
        public void WindowsD3d11RuntimeGateAcceptsOnlyWindowsPlayerAndDirect3d11()
        {
            Assert.That(
                GaragePrototypeMarker.IsRequiredWindowsD3D11Runtime(
                    RuntimePlatform.WindowsPlayer,
                    GraphicsDeviceType.Direct3D11),
                Is.True);
            Assert.That(
                GaragePrototypeMarker.IsRequiredWindowsD3D11Runtime(
                    RuntimePlatform.WindowsPlayer,
                    GraphicsDeviceType.Direct3D12),
                Is.False);
            Assert.That(
                GaragePrototypeMarker.IsRequiredWindowsD3D11Runtime(
                    RuntimePlatform.OSXPlayer,
                    GraphicsDeviceType.Direct3D11),
                Is.False);
        }
    }
}
