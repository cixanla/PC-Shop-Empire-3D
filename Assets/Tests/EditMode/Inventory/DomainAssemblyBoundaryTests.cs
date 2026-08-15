using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Inventory
{
    public sealed class DomainAssemblyBoundaryTests
    {
        [Test]
        public void CatalogHasStableNameAndOnlyDependsOnCoreDomain()
        {
            string[] references = typeof(CatalogAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(CatalogAssembly).Assembly.GetName().Name, Is.EqualTo(CatalogAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Not.Contain("PSE.Inventory"));
            AssertNoUnityReferences(references);
        }

        [Test]
        public void InventoryHasStableNameAndDependsOnlyOnCoreAndCatalogDomains()
        {
            string[] references = typeof(InventoryAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(InventoryAssembly).Assembly.GetName().Name, Is.EqualTo(InventoryAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            AssertNoUnityReferences(references);
        }

        private static void AssertNoUnityReferences(string[] references)
        {
            Assert.That(
                references.Any(name =>
                    name.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                    name.StartsWith("UnityEditor", StringComparison.Ordinal)),
                Is.False);
        }
    }
}
