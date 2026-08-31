using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string CustomPcPackagingSmokeSuccessMarker =
            "GARAGE_CUSTOM_PC_PACKAGING_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted quality-release=current " +
            "review=keyboard seal=gamepad package=one physical=large-box " +
            "source-projections=10 hidden-after-seal=true pickup=ok " +
            "cart=hands-cart-hands dispatch=staged custody=append-only " +
            "custody-receipts=4 replay=ok upstream=unchanged invariants=ok";

        private IEnumerator RunCustomPcPackagingSmoke()
        {
            return RunCustomPcPackagingSmokeGuarded(
                RunCustomPcPackagingSmokeCore());
        }

        private IEnumerator RunCustomPcPackagingSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedQualityReleaseSmokeFailureCode = null;
            _suppressQualityReleaseSmokeSuccessMarker = true;
            try
            {
                yield return RunQualityReleaseSmoke();
            }
            finally
            {
                _suppressQualityReleaseSmokeSuccessMarker = false;
            }

            string prerequisiteFailure = _nestedQualityReleaseSmokeFailureCode;
            _nestedQualityReleaseSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogCustomPcPackagingSmokeFailure(
                    "smoke.quality-release-prerequisite-" + suffix);
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            OperationResult<CustomPcQualityReleaseReceipt> currentQuality =
                session != null && session.TryGetQualityRelease(
                    out CustomPcQualityReleaseAuthority qualityAuthority)
                    ? qualityAuthority.EvaluateCurrentRelease()
                    : OperationResult<CustomPcQualityReleaseReceipt>.Fail(
                        CustomPcPackageFailures.QualityReleaseInvalid);
            if (session == null || playerMotor == null || playerInput == null ||
                playerCarry == null || transportCart == null ||
                customPcPackagingStation == null ||
                customPcPackageDispatch == null ||
                customPcPackageBinding == null || customPcPackage == null ||
                !HasCustomPcPackagingR68Runtime || currentQuality.IsFailure ||
                session.TryGetCustomPcPackageAuthority(out _) ||
                session.TryGetPrototypeCustomPcPackage(out _) ||
                customPcPackage.gameObject.activeSelf ||
                customPcPackageBinding.ValidateContract().IsFailure ||
                customPcPackageBinding.ValidateSealProjection().IsFailure ||
                customPcPackageBinding.SourceProjections.Count !=
                    CustomPcPackagePhysicalBinding.RequiredSourceProjectionCount ||
                customPcPackageBinding.SourceProjections.Any(source =>
                    source == null || !source.gameObject.activeSelf))
            {
                LogCustomPcPackagingSmokeFailure("smoke.context-mismatch");
                yield break;
            }

            CustomPcQualityReleaseReceipt qualityReceipt = currentQuality.Value;
            long inventoryRevision = session.Inventory.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long powerRevision = session.PowerState.Revision;
            int powerReceiptCount = session.PowerState.ReceiptCount;
            long validationRevision = session.Validation.Revision;
            int validationReceiptCount = session.Validation.ReceiptCount;

            Keyboard smokeKeyboard = null;
            Gamepad smokeGamepad = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeGamepad = InputSystem.AddDevice<Gamepad>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                InputSystem.Update();

                MovePlayerToCustomPcPackagingStation(1.35f);
                if (!customPcPackagingStation.PromptText.Contains(
                        "EXACT KALİTE DOSYASINI İNCELE"))
                {
                    LogCustomPcPackagingSmokeFailure(
                        "smoke.review-prompt-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;
                if (!customPcPackagingStation.IsReviewing ||
                    session.TryGetPrototypeCustomPcPackage(out _) ||
                    !customPcPackagingStation.PromptText.Contains(
                        "KOLİYİ MÜHÜRLE"))
                {
                    LogCustomPcPackagingSmokeFailure(
                        "smoke.review-state-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.South
                    });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                if (!session.TryGetPrototypeCustomPcPackage(
                        out CustomPcPackageReceipt packageReceipt) ||
                    !session.TryGetCustomPcPackageAuthority(
                        out CustomPcPackageAuthority packages) ||
                    packages.PackageCount != 1 || packages.Revision != 1L ||
                    packageReceipt.State != CustomPcPackageState.Sealed ||
                    !ReferenceEquals(
                        packageReceipt.SourceQualityReleaseReceipt,
                        qualityReceipt) ||
                    customPcPackageBinding.PackageReceipt != packageReceipt ||
                    !customPcPackage.gameObject.activeSelf ||
                    customPcPackage.CarryProfile !=
                        PhysicalCarryProfile.LargeBox ||
                    customPcPackageBinding.SourceProjections.Any(source =>
                        source.gameObject.activeSelf) ||
                    !playerInput.UsesGamepadPrompts)
                {
                    LogCustomPcPackagingSmokeFailure(
                        "smoke.seal-or-physical-projection-mismatch");
                    yield break;
                }

                OperationResult pickup = playerCarry.TryPickup(customPcPackage);
                OperationResult load = pickup.IsSuccess
                    ? playerCarry.TryLoadHeldItem(transportCart)
                    : pickup;
                OperationResult unload = load.IsSuccess
                    ? playerCarry.TryUnloadCart(transportCart)
                    : load;
                if (pickup.IsFailure || load.IsFailure || unload.IsFailure ||
                    playerCarry.HeldItem != customPcPackage ||
                    transportCart.Cargo != null)
                {
                    string code = pickup.IsFailure
                        ? pickup.Error.Code
                        : load.IsFailure
                            ? load.Error.Code
                            : unload.IsFailure
                                ? unload.Error.Code
                                : "smoke.cart-state-mismatch";
                    LogCustomPcPackagingSmokeFailure(
                        "smoke.physical-custody-" + code);
                    yield break;
                }

                MovePlayerToCustomPcPackageDispatch(1.35f);
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;

                OperationResult<CustomPcPackageReceipt> replay =
                    packages.TrySealPackage(
                        packageReceipt.PackageId,
                        packageReceipt.OperationId,
                        qualityReceipt,
                        packageReceipt.ExpectedRevision);
                bool custodyFound = packages.TryGetCurrentCustody(
                    packageReceipt,
                    out CustomPcPackageCustody custody);
                if (playerCarry.IsCarrying ||
                    !customPcPackageDispatch.IsStaged ||
                    !customPcPackage.IsStablePlacement ||
                    customPcPackage.Ownership != PhysicalItemOwnership.World ||
                    !custodyFound ||
                    custody != CustomPcPackageCustody.DispatchStaging ||
                    packages.Revision != 5L ||
                    packages.CustodyReceiptCount != 4 ||
                    replay.IsFailure ||
                    !ReferenceEquals(replay.Value, packageReceipt) ||
                    Vector3.Distance(
                        customPcPackage.transform.position,
                        customPcPackageBinding.DispatchPose.position) > 0.001f ||
                    !customPcPackageBinding.PackageLabel.text.Contains(
                        "DISPATCHSTAGING") ||
                    packages.ValidateReceiptHistory().IsFailure ||
                    session.Inventory.Revision != inventoryRevision ||
                    session.CustomPcWorkOrders.Revision != workOrderRevision ||
                    session.CustomPcBuildKit.Revision != buildKitRevision ||
                    session.AssemblyBuild.Revision != assemblyRevision ||
                    session.AssemblyBuild.ReceiptCount != assemblyReceiptCount ||
                    session.PowerState.Revision != powerRevision ||
                    session.PowerState.ReceiptCount != powerReceiptCount ||
                    session.Validation.Revision != validationRevision ||
                    session.Validation.ReceiptCount != validationReceiptCount ||
                    session.ValidateInvariants().IsFailure)
                {
                    LogCustomPcPackagingSmokeFailure(
                        "smoke.dispatch-replay-mutation-or-invariant-mismatch");
                    yield break;
                }
            }
            finally
            {
                if (smokeKeyboard != null && smokeKeyboard.added)
                {
                    InputSystem.RemoveDevice(smokeKeyboard);
                }

                if (smokeGamepad != null && smokeGamepad.added)
                {
                    InputSystem.RemoveDevice(smokeGamepad);
                }
            }

            Debug.Log(CustomPcPackagingSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private void MovePlayerToCustomPcPackagingStation(float distance)
        {
            MovePlayerToCustomPcPackageFocus(
                customPcPackagingStation?.transform,
                customPcPackagingStation?.InteractionCollider,
                distance);
        }

        private void MovePlayerToCustomPcPackageDispatch(float distance)
        {
            MovePlayerToCustomPcPackageFocus(
                customPcPackageDispatch?.transform,
                customPcPackageDispatch?.InteractionCollider,
                distance);
        }

        private void MovePlayerToCustomPcPackageFocus(
            Transform station,
            Collider focusCollider,
            float distance)
        {
            if (station == null || focusCollider == null || playerMotor == null)
            {
                return;
            }

            Vector3 target = focusCollider.bounds.center;
            Vector3 outward = target - station.position;
            outward.y = 0f;
            if (outward.sqrMagnitude <= 0.0001f)
            {
                outward = -focusCollider.transform.forward;
            }

            Vector3 playerPosition = target + (outward.normalized * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private IEnumerator RunCustomPcPackagingSmokeGuarded(IEnumerator root)
        {
            var routines = new Stack<IEnumerator>();
            routines.Push(root);
            try
            {
                while (routines.Count > 0)
                {
                    IEnumerator active = routines.Peek();
                    bool moved = false;
                    object yielded = null;
                    Exception failure = null;
                    try
                    {
                        moved = active.MoveNext();
                        if (moved)
                        {
                            yielded = active.Current;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                    }

                    if (failure != null)
                    {
                        Debug.LogException(failure);
                        LogCustomPcPackagingSmokeFailure(
                            "smoke.unhandled-exception");
                        yield break;
                    }

                    if (!moved)
                    {
                        routines.Pop();
                        (active as IDisposable)?.Dispose();
                        continue;
                    }

                    if (yielded is IEnumerator nested)
                    {
                        routines.Push(nested);
                        continue;
                    }

                    yield return yielded;
                }
            }
            finally
            {
                while (routines.Count > 0)
                {
                    (routines.Pop() as IDisposable)?.Dispose();
                }
            }
        }

        private static void LogCustomPcPackagingSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_CUSTOM_PC_PACKAGING_RUNTIME_SMOKE " +
                "packaging-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
