using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Retail;

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

        [Test]
        public void OrdersHasStableNameAndDependsOnlyOnCoreCatalogAndInventoryDomains()
        {
            string[] references = typeof(OrdersAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(OrdersAssembly).Assembly.GetName().Name, Is.EqualTo(OrdersAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            Assert.That(references, Does.Contain("PSE.Inventory"));
            AssertNoUnityReferences(references);
        }

        [Test]
        public void RetailHasStableNameAndDependsOnlyOnCoreCatalogAndInventoryDomains()
        {
            string[] references = typeof(RetailAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(RetailAssembly).Assembly.GetName().Name, Is.EqualTo(RetailAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            Assert.That(references, Does.Contain("PSE.Inventory"));
            Assert.That(references, Does.Not.Contain("PSE.Orders"));
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
