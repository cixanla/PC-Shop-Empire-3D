using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private OperationResult TryPickupCustomPcPackage(
            PhysicalItemProjection item,
            CustomPcPackagePhysicalBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    CustomPcPackagingStationFailures.Paused));
            }

            OperationResult preflight = binding.PrepareCustodyTransition(
                CustomPcPackageCustody.ActorHands,
                out CustomPcPackageCustody source,
                out long expectedRevision);
            if (preflight.IsFailure)
            {
                return Remember(preflight);
            }

            OperationResult physical = item.BeginCarry(
                carryAnchor,
                heldItemLayer);
            if (physical.IsFailure)
            {
                return Remember(physical);
            }

            OperationResult custody = binding.CommitCustodyTransition(
                source,
                CustomPcPackageCustody.ActorHands,
                expectedRevision);
            if (custody.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                return Remember(rollback.IsSuccess
                    ? custody
                    : OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.PhysicalRollbackFailed));
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor?.ApplyCarryProfile(item.CarryProfile);
            SetCarryHandsState(blocked: false);
            return OperationResult.Success();
        }

        private OperationResult TryLoadHeldCustomPcPackage(
            TransportCartProjection cart,
            CustomPcPackagePhysicalBinding binding)
        {
            OperationResult preflight = binding.PrepareCustodyTransition(
                CustomPcPackageCustody.TransportCart,
                out CustomPcPackageCustody source,
                out long expectedRevision);
            if (preflight.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(preflight);
            }

            PhysicalItemProjection item = HeldItem;
            OperationResult physical = cart.TryLoad(item, heldItemLayer);
            if (physical.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(physical);
            }

            OperationResult custody = binding.CommitCustodyTransition(
                source,
                CustomPcPackageCustody.TransportCart,
                expectedRevision);
            if (custody.IsFailure)
            {
                OperationResult<PhysicalItemProjection> rollback =
                    cart.TryUnload(carryAnchor, heldItemLayer);
                return Remember(rollback.IsSuccess
                    ? custody
                    : OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.PhysicalRollbackFailed));
            }

            HeldItem = null;
            _heldItemId = string.Empty;
            FocusedItem = null;
            FocusedCart = cart;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.TargetFocused);
            return OperationResult.Success();
        }

        private OperationResult TryUnloadCustomPcPackage(
            TransportCartProjection cart,
            CustomPcPackagePhysicalBinding binding)
        {
            OperationResult preflight = binding.PrepareCustodyTransition(
                CustomPcPackageCustody.ActorHands,
                out CustomPcPackageCustody source,
                out long expectedRevision);
            if (preflight.IsFailure)
            {
                return Remember(preflight);
            }

            OperationResult<PhysicalItemProjection> physical = cart.TryUnload(
                carryAnchor,
                heldItemLayer);
            if (physical.IsFailure)
            {
                return Remember(OperationResult.Fail(physical.Error));
            }

            OperationResult custody = binding.CommitCustodyTransition(
                source,
                CustomPcPackageCustody.ActorHands,
                expectedRevision);
            if (custody.IsFailure)
            {
                OperationResult rollback = cart.TryLoad(
                    physical.Value,
                    heldItemLayer);
                return Remember(rollback.IsSuccess
                    ? custody
                    : OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.PhysicalRollbackFailed));
            }

            HeldItem = physical.Value;
            _heldItemId = HeldItem.ItemIdValue;
            FocusedItem = null;
            FocusedCart = null;
            ResetPlacementState();
            motor?.ApplyCarryProfile(HeldItem.CarryProfile);
            LastFailureCode = string.Empty;
            SetCarryHandsState(blocked: false);
            return OperationResult.Success();
        }

        private OperationResult TryDropCustomPcPackage(
            Pose pose,
            CustomPcPackagePhysicalBinding binding)
        {
            OperationResult preflight = binding.PrepareCustodyTransition(
                CustomPcPackageCustody.WorldFloor,
                out CustomPcPackageCustody source,
                out long expectedRevision);
            if (preflight.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(preflight);
            }

            PhysicalItemProjection item = HeldItem;
            OperationResult physical = item.ReleaseTo(pose);
            if (physical.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(physical);
            }

            OperationResult custody = binding.CommitCustodyTransition(
                source,
                CustomPcPackageCustody.WorldFloor,
                expectedRevision);
            if (custody.IsFailure)
            {
                OperationResult rollback = item.RecoverToCarryAfterAuthority(
                    carryAnchor,
                    heldItemLayer);
                SetCarryHandsState(blocked: true);
                return Remember(rollback.IsSuccess
                    ? custody
                    : OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.PhysicalRollbackFailed));
            }

            Physics.SyncTransforms();
            CompleteHeldItemRelease();
            return Remember(OperationResult.Success());
        }

        private OperationResult TryRecoverHeldCustomPcPackage(
            PhysicalItemProjection item,
            CustomPcPackagePhysicalBinding binding)
        {
            OperationResult preflight = binding.PrepareCustodyTransition(
                CustomPcPackageCustody.WorldFloor,
                out CustomPcPackageCustody source,
                out long expectedRevision);
            if (preflight.IsFailure)
            {
                return Remember(preflight);
            }

            OperationResult physical = item.RecoverToLastSafePose();
            if (physical.IsFailure)
            {
                return Remember(physical);
            }

            OperationResult custody = binding.CommitCustodyTransition(
                source,
                CustomPcPackageCustody.WorldFloor,
                expectedRevision);
            if (custody.IsFailure)
            {
                OperationResult rollback = item.RecoverToCarryAfterAuthority(
                    carryAnchor,
                    heldItemLayer);
                return Remember(rollback.IsSuccess
                    ? custody
                    : OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.PhysicalRollbackFailed));
            }

            HeldItem = null;
            _heldItemId = string.Empty;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
            return Remember(OperationResult.Success());
        }

        public OperationResult TryStageHeldCustomPcPackage(
            CustomPcPackagePhysicalBinding binding,
            Pose dispatchPose)
        {
            if (binding == null || HeldItem == null ||
                HeldItem != binding.PackageItem)
            {
                return Remember(OperationResult.Fail(
                    CustomPcPackageDispatchFailures.PackageNotHeld));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    CustomPcPackageDispatchFailures.Paused));
            }

            OperationResult preflight = binding.PrepareCustodyTransition(
                CustomPcPackageCustody.DispatchStaging,
                out CustomPcPackageCustody source,
                out long expectedRevision);
            if (preflight.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(preflight);
            }

            PhysicalItemProjection item = HeldItem;
            OperationResult physical = item.PlaceAt(dispatchPose);
            if (physical.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(physical);
            }

            OperationResult custody = binding.CommitCustodyTransition(
                source,
                CustomPcPackageCustody.DispatchStaging,
                expectedRevision);
            if (custody.IsFailure)
            {
                OperationResult rollback = item.RecoverToCarryAfterAuthority(
                    carryAnchor,
                    heldItemLayer);
                SetCarryHandsState(blocked: true);
                return Remember(rollback.IsSuccess
                    ? custody
                    : OperationResult.Fail(
                        CustomPcPackagePhysicalFailures.PhysicalRollbackFailed));
            }

            Physics.SyncTransforms();
            CompleteHeldItemRelease();
            return Remember(OperationResult.Success());
        }

        private static CustomPcPackagePhysicalBinding GetCustomPcPackageBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<CustomPcPackagePhysicalBinding>()
                : null;
        }
    }
}
