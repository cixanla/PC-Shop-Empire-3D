using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Core;

namespace PCShopEmpire3D.Tests.EditMode
{
    public sealed class CoreAssemblyBoundaryTests
    {
        [Test]
        public void CoreAssemblyHasStableName()
        {
            Assert.That(typeof(CoreAssembly).Assembly.GetName().Name, Is.EqualTo(CoreAssembly.Name));
        }

        [Test]
        public void CoreAssemblyDoesNotReferenceUnityPresentationAssemblies()
        {
            string[] referencedAssemblies = typeof(CoreAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(
                referencedAssemblies.Any(name =>
                    name.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                    name.StartsWith("UnityEditor", StringComparison.Ordinal)),
                Is.False,
                "PSE.Core must remain independent from Unity presentation/runtime assemblies.");
        }
    }
}
