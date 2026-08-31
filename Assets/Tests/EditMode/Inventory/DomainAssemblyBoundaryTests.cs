using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Economy;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;
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
        public void AssemblyHasStableNameAndDependsOnlyOnCoreCatalogAndInventoryDomains()
        {
            string[] references = typeof(AssemblyAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(AssemblyAssembly).Assembly.GetName().Name,
                Is.EqualTo(AssemblyAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            Assert.That(references, Does.Contain("PSE.Inventory"));
            Assert.That(references, Does.Not.Contain("PSE.Orders"));
            Assert.That(references, Does.Not.Contain("PSE.Retail"));
            Assert.That(references, Does.Not.Contain("PSE.Economy"));
            Assert.That(references, Does.Not.Contain("PSE.Presentation"));
            AssertNoUnityReferences(references);
        }

        [Test]
        public void OrdersHasStableNameAndDependsDownstreamOnRetailWithoutAssemblyCycle()
        {
            string[] references = typeof(OrdersAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(OrdersAssembly).Assembly.GetName().Name, Is.EqualTo(OrdersAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            Assert.That(references, Does.Contain("PSE.Inventory"));
            Assert.That(references, Does.Contain("PSE.Retail"));
            Assert.That(references, Does.Not.Contain("PSE.Assembly"));
            Assert.That(references, Does.Not.Contain("PSE.Presentation"));
            AssertNoUnityReferences(references);
        }

        [Test]
        public void QualityJoinsOrdersAndAssemblyWithoutCreatingAnUpstreamCycle()
        {
            string[] references = typeof(QualityAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(QualityAssembly).Assembly.GetName().Name,
                Is.EqualTo(QualityAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            Assert.That(references, Does.Contain("PSE.Inventory"));
            Assert.That(references, Does.Contain("PSE.Retail"));
            Assert.That(references, Does.Contain("PSE.Orders"));
            Assert.That(references, Does.Contain("PSE.Assembly"));
            Assert.That(references, Does.Not.Contain("PSE.Presentation"));
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
            Assert.That(references, Does.Not.Contain("PSE.Economy"));
            AssertNoUnityReferences(references);
        }

        [Test]
        public void EconomyHasStableNameAndDependsDownstreamOnCoreInventoryAndRetailDomains()
        {
            string[] references = typeof(EconomyAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(EconomyAssembly).Assembly.GetName().Name,
                Is.EqualTo(EconomyAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Inventory"));
            Assert.That(references, Does.Contain("PSE.Retail"));
            Assert.That(references, Does.Not.Contain("PSE.Orders"));
            Assert.That(references, Does.Not.Contain("PSE.Presentation"));
            AssertNoUnityReferences(references);
        }

        [Test]
        public void CheckoutCompletionIsNotAProductionPublicMutationPath()
        {
            Assert.That(
                typeof(RetailCheckoutAuthority).GetMethod(
                    "CompleteCheckout",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public),
                Is.Null);
            Assert.That(
                typeof(RetailCheckoutAuthority).GetMethod(
                    "CompleteCheckout",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic),
                Is.Not.Null);
        }

        [Test]
        public void CustomPcWorkOrderIssueRequiresOpaqueAccessAndHasNoPublicSessionBypass()
        {
            System.Reflection.MethodInfo publicIssue =
                typeof(CustomPcWorkOrderAuthority).GetMethod(
                    "Issue",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
            System.Reflection.MethodInfo issue =
                typeof(CustomPcWorkOrderAuthority).GetMethod(
                    "Issue",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);

            Assert.That(publicIssue, Is.Null);
            Assert.That(issue, Is.Not.Null);
            Assert.That(issue.GetParameters()[0].ParameterType,
                Is.EqualTo(typeof(CustomPcWorkOrderIssueAccess)));
            Assert.That(typeof(CustomPcWorkOrderIssueAccess).IsPublic, Is.False);
            Assert.That(typeof(CustomPcWorkOrderAuthorityCreation).IsPublic, Is.False);
            Assert.That(
                typeof(CustomPcWorkOrderIssueAccess).GetConstructors(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public),
                Is.Empty);
            Assert.That(
                typeof(CustomPcWorkOrderAuthority).GetMethod(
                    "Create",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public),
                Is.Null);
            Assert.That(
                typeof(GarageStockFlowSession).GetMethod(
                    "IssuePrototypeCustomPcWorkOrder",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public),
                Is.Null);
            Assert.That(
                typeof(CustomPcWorkOrderAuthority).GetMethod(
                    "GetIssueAccessForTests",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic),
                Is.Null,
                "Production assemblies must not expose the authority-owned issue capability.");
        }

        [Test]
        public void ActorsHasStableNameAndDependsOnlyOnCoreAndCatalogDomains()
        {
            string[] references = typeof(ActorsAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.That(typeof(ActorsAssembly).Assembly.GetName().Name, Is.EqualTo(ActorsAssembly.Name));
            Assert.That(references, Does.Contain("PSE.Core"));
            Assert.That(references, Does.Contain("PSE.Catalog"));
            Assert.That(references, Does.Not.Contain("PSE.Retail"));
            Assert.That(references, Does.Not.Contain("PSE.Inventory"));
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
