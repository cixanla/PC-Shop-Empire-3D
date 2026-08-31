using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class PowerTestPreflightInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator CustomPcPackagingKeyboardSealsCarriesCartsAndStagesExactPackage()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareCurrentQualityRelease(marker, keyboard, mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            CustomPcPackagingStationProjection station =
                marker.CustomPcPackagingStation;
            CustomPcPackagePhysicalBinding binding =
                marker.CustomPcPackageBinding;
            Assert.That(marker.HasCustomPcPackagingR68Runtime, Is.True);
            Assert.That(marker.CustomPcPackage.gameObject.activeSelf, Is.False);
            Assert.That(binding.SourceProjections.All(
                source => source.gameObject.activeSelf), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcPackage(out _), Is.False);
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            MovePlayerToFixedStation(
                marker,
                station.transform,
                station.InteractionCollider,
                1.45f);
            yield return null;
            Assert.That(station.IsFocused, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("KALİTE DOSYASINI İNCELE"));
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewing, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcPackage(out _), Is.False);
            Assert.That(station.PromptText,
                Does.Contain("KOLİYİ MÜHÜRLE"));
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetPrototypeCustomPcPackage(
                out CustomPcPackageReceipt package), Is.True);
            Assert.That(package.SourceQualityReleaseReceipt,
                Is.SameAs(session.QualityRelease.EvaluateCurrentRelease().Value));
            Assert.That(marker.CustomPcPackage.gameObject.activeSelf, Is.True);
            Assert.That(binding.SourceProjections.All(
                source => !source.gameObject.activeSelf), Is.True);
            Assert.That(binding.PackageReceipt, Is.SameAs(package));
            Assert.That(station.PromptText,
                Does.Contain("PAKET MÜHÜRLENDİ")
                    .And.Contain("SEVK ALANINA TAŞI"));
            Assert.That(binding.TryGetCurrentCustody(out var custody), Is.True);
            Assert.That(custody,
                Is.EqualTo(CustomPcPackageCustody.PackagingWorkbench));

            Require(marker.PlayerCarry.TryPickup(marker.CustomPcPackage));
            Assert.That(binding.TryGetCurrentCustody(out custody), Is.True);
            Assert.That(custody, Is.EqualTo(CustomPcPackageCustody.ActorHands));
            Require(marker.PlayerCarry.TryLoadHeldItem(marker.TransportCart));
            Assert.That(marker.TransportCart.Cargo,
                Is.SameAs(marker.CustomPcPackage));
            Assert.That(binding.TryGetCurrentCustody(out custody), Is.True);
            Assert.That(custody,
                Is.EqualTo(CustomPcPackageCustody.TransportCart));
            Require(marker.PlayerCarry.TryUnloadCart(marker.TransportCart));
            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(marker.CustomPcPackage));

            CustomPcPackageDispatchProjection dispatch =
                marker.CustomPcPackageDispatch;
            MovePlayerToFixedStation(
                marker,
                dispatch.transform,
                dispatch.InteractionCollider,
                1.45f);
            yield return null;
            Assert.That(dispatch.IsFocused, Is.True);
            Assert.That(dispatch.PromptText,
                Does.Contain("SEVK ALANINA BIRAK"));
            PressInteract(keyboard, dispatch.ProcessInputFrame);
            yield return null;

            Assert.That(marker.PlayerCarry.IsCarrying, Is.False);
            Assert.That(marker.CustomPcPackage.IsStablePlacement, Is.True);
            Assert.That(dispatch.IsStaged, Is.True);
            Assert.That(binding.TryGetCurrentCustody(out custody), Is.True);
            Assert.That(custody,
                Is.EqualTo(CustomPcPackageCustody.DispatchStaging));
            Assert.That(session.CustomPcPackages.PackageCount, Is.EqualTo(1));
            Assert.That(session.CustomPcPackages.CustodyReceiptCount,
                Is.EqualTo(4));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomPcPackagingDropRecoveryAndRedispatchKeepSameProjection()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareCurrentQualityRelease(marker, keyboard, mouse);
            yield return SealPackageForTests(marker);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            CustomPcPackagePhysicalBinding binding =
                marker.CustomPcPackageBinding;
            PhysicalItemProjection packageProjection = marker.CustomPcPackage;
            Require(marker.PlayerCarry.TryPickup(packageProjection));
            MovePlayerAndAim(
                marker,
                new Vector3(0f, 0.05f, -2.5f),
                new Vector3(0f, 0.75f, -1.5f));
            yield return null;
            Require(marker.PlayerCarry.TryDrop());
            Assert.That(binding.TryGetCurrentCustody(out var custody), Is.True);
            Assert.That(custody, Is.EqualTo(CustomPcPackageCustody.WorldFloor));
            Assert.That(packageProjection.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));

            Require(marker.PlayerCarry.TryPickup(packageProjection));
            Require(marker.PlayerCarry.TryRecoverHeldItem());
            Assert.That(binding.TryGetCurrentCustody(out custody), Is.True);
            Assert.That(custody, Is.EqualTo(CustomPcPackageCustody.WorldFloor));
            Assert.That(packageProjection.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));

            Require(marker.PlayerCarry.TryPickup(packageProjection));
            Require(marker.CustomPcPackageDispatch.TryStageForTests());
            Assert.That(binding.TryGetCurrentCustody(out custody), Is.True);
            Assert.That(custody,
                Is.EqualTo(CustomPcPackageCustody.DispatchStaging));
            Assert.That(marker.CustomPcPackage, Is.SameAs(packageProjection));
            Assert.That(session.CustomPcPackages.PackageCount, Is.EqualTo(1));
            Assert.That(session.CustomPcPackages.CustodyReceiptCount,
                Is.EqualTo(6));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomPcPackagingGamepadUsesSameTwoStepSealAuthority()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareCurrentQualityRelease(marker, keyboard, mouse);

            CustomPcPackagingStationProjection station =
                marker.CustomPcPackagingStation;
            MovePlayerToFixedStation(
                marker,
                station.transform,
                station.InteractionCollider,
                1.45f);
            yield return null;
            PressGamepadInteract(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.IsReviewing, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("A").And.Contain("KOLİYİ MÜHÜRLE"));
            PressGamepadInteract(gamepad, station.ProcessInputFrame);
            yield return null;

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.TryGetPrototypeCustomPcPackage(out _), Is.True);
            Assert.That(session.CustomPcPackages.PackageCount, Is.EqualTo(1));
            Assert.That(marker.CustomPcPackage.gameObject.activeSelf, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomPcPackagingContextLossAndBusyHandsFailWithoutPackage()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareCurrentQualityRelease(marker, keyboard, mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            CustomPcPackagingStationProjection station =
                marker.CustomPcPackagingStation;
            Assert.That(station.TryReviewOrSealForTests().IsSuccess, Is.True);
            yield return null;
            Assert.That(station.IsReviewing, Is.True);

            Assert.That(session.TryGetQualityRelease(
                out CustomPcQualityReleaseAuthority qualityRelease), Is.True);
            OperationResult<CustomPcQualityReleaseReceipt> currentRelease =
                qualityRelease.EvaluateCurrentRelease();
            Assert.That(currentRelease.IsSuccess, Is.True,
                currentRelease.Error.Code);
            PcValidationReceipt validation =
                currentRelease.Value.SourceValidationReceipt;
            OperationResult<PcPowerStateReceipt> nextPowerOn =
                session.PowerState.TryPowerOn(
                    session.CreatePrototypePowerStateOperationId(
                        PcPowerTransitionKind.PowerOn,
                        session.PowerState.Revision + 1L),
                    validation.PreflightReceipt,
                    session.PowerState.Revision);
            Assert.That(nextPowerOn.IsSuccess, Is.True, nextPowerOn.Error.Code);
            yield return null;

            Assert.That(station.TryReviewOrSealForTests().Error,
                Is.EqualTo(
                    CustomPcPackagingStationFailures.QualityReleaseMissing));
            Assert.That(session.TryGetPrototypeCustomPcPackage(out _), Is.False);
            Assert.That(marker.CustomPcPackage.gameObject.activeSelf, Is.False);
            Assert.That(marker.CustomPcPackageBinding.SourceProjections.All(
                source => source.gameObject.activeSelf), Is.True);

            PhysicalItemProjection largeBox = Object
                .FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .Single(item => item.CarryProfile == PhysicalCarryProfile.LargeBox);
            Require(marker.PlayerCarry.TryPickup(largeBox));
            MovePlayerToFixedStation(
                marker,
                station.transform,
                station.InteractionCollider,
                1.45f);
            yield return null;
            Assert.That(station.PromptText,
                Does.Contain("PAKETLEME ENGELLİ")
                    .And.Contain("ELLERİNİ BOŞALT"));
            Assert.That(station.TryReviewOrSealForTests().Error,
                Is.EqualTo(CustomPcPackagingStationFailures.HandsBusy));
            Assert.That(session.TryGetPrototypeCustomPcPackage(out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator PrepareCurrentQualityRelease(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse)
        {
            yield return PrepareSafeShutdownForQuality(marker, keyboard, mouse);
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.TryGetQualityRelease(
                out CustomPcQualityReleaseAuthority authority), Is.True);
            Assert.That(authority.EvaluateCurrentRelease().IsSuccess, Is.True);
        }

        private static IEnumerator SealPackageForTests(
            GaragePrototypeMarker marker)
        {
            CustomPcPackagingStationProjection station =
                marker.CustomPcPackagingStation;
            Assert.That(station.TryReviewOrSealForTests().IsSuccess, Is.True);
            yield return null;
            Assert.That(station.TryReviewOrSealForTests().IsSuccess, Is.True);
            yield return null;
            Assert.That(marker.StockFlow.EnsureInitialized()
                .TryGetPrototypeCustomPcPackage(out _), Is.True);
        }

        private static void MovePlayerToFixedStation(
            GaragePrototypeMarker marker,
            Transform stationRoot,
            Collider focusCollider,
            float distance)
        {
            Vector3 target = focusCollider.bounds.center;
            Vector3 outward = target - stationRoot.position;
            outward.y = 0f;
            if (outward.sqrMagnitude <= 0.0001f)
            {
                outward = -focusCollider.transform.forward;
            }

            MovePlayerAndAim(
                marker,
                target + (outward.normalized * distance),
                target);
        }
    }
}
