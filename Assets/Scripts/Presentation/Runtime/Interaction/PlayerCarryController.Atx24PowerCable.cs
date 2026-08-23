using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        [SerializeField] private Atx24PowerCableRouteProjection atx24PowerCableRoute;
        [SerializeField] private Atx24PowerCableAssemblyItemBinding atx24PowerCableBinding;

        public bool IsAtx24PowerCableRouteMode { get; private set; }

        public Atx24PowerCableRouteStatus CurrentAtx24PowerCableRouteStatus
        {
            get;
            private set;
        } = Atx24PowerCableRouteStatus.ContextMissing;

        public void ConfigureAtx24PowerCableRoute(
            Atx24PowerCableRouteProjection routeProjection,
            Atx24PowerCableAssemblyItemBinding assemblyBinding)
        {
            if (routeProjection == null)
            {
                throw new ArgumentNullException(nameof(routeProjection));
            }

            if (assemblyBinding == null)
            {
                throw new ArgumentNullException(nameof(assemblyBinding));
            }

            if (assemblyBinding.Route != routeProjection)
            {
                throw new ArgumentException(
                    "The ATX24 binding must own the configured route.",
                    nameof(assemblyBinding));
            }

            atx24PowerCableRoute = routeProjection;
            atx24PowerCableBinding = assemblyBinding;
            atx24PowerCableBinding.SyncProjectionToAuthority();
        }

        public bool MatchesAtx24PowerCableConfiguration(
            Atx24PowerCableRouteProjection routeProjection,
            Atx24PowerCableAssemblyItemBinding assemblyBinding)
        {
            return routeProjection != null &&
                   assemblyBinding != null &&
                   atx24PowerCableRoute == routeProjection &&
                   atx24PowerCableBinding == assemblyBinding &&
                   assemblyBinding.Route == routeProjection;
        }

        public OperationResult TrySetAtx24PowerCableRouteMode(bool enabled)
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.paused")));
            }

            SetAtx24PowerCableRouteMode(enabled);
            if (enabled)
            {
                UpdateAtx24PowerCableRoutePreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateAtx24PowerCableConnectorPreview()
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.nothing-held")));
            }

            if (!IsAtx24PowerCableRouteMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateAtx24PowerCableRoutePreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmAtx24PowerCableRoute()
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.nothing-held")));
            }

            if (!IsAtx24PowerCableRouteMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.mode-inactive")));
            }

            return TryConfirmAtx24PowerCableRoute(
                binding,
                EvaluateAtx24PowerCableRoute(binding));
        }

        private OperationResult TryConfirmAtx24PowerCableRoute(
            Atx24PowerCableAssemblyItemBinding binding,
            Atx24PowerCableRouteEvaluation evaluation)
        {
            ApplyAtx24PowerCableRouteEvaluation(evaluation);
            if (!evaluation.CanRoute)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult route = binding.TryRouteAt(
                evaluation.Pose,
                evaluation.Orientation,
                carryAnchor,
                heldItemLayer);
            if (route.IsSuccess)
            {
                CompleteHeldItemRelease();
                binding.SyncProjectionToAuthority();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(route);
        }

        private bool ProcessHeldAtx24PowerCableInput()
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TrySetAtx24PowerCableRouteMode(
                    !IsAtx24PowerCableRouteMode);
                return true;
            }

            if (IsAtx24PowerCableRouteMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateAtx24PowerCableConnectorPreview();
                return true;
            }

            if (!IsAtx24PowerCableRouteMode)
            {
                atx24PowerCableRoute?.SetRouteModeActive(active: false);
                if (input.TryConsumeDropPressThisFrame())
                {
                    input.TryConsumePrimaryActionPressThisFrame();
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    TryDrop();
                }

                return true;
            }

            Atx24PowerCableRouteEvaluation evaluation =
                EvaluateAtx24PowerCableRoute(binding);
            ApplyAtx24PowerCableRouteEvaluation(evaluation);
            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                TryConfirmAtx24PowerCableRoute(binding, evaluation);
            }

            return true;
        }

        private void SetAtx24PowerCableRouteMode(bool enabled)
        {
            IsAtx24PowerCableRouteMode = enabled &&
                                         HeldItem != null &&
                                         GetAtx24PowerCableBinding(HeldItem) != null;
            IsPlacementMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentAtx24PowerCableRouteStatus =
                Atx24PowerCableRouteStatus.ContextMissing;
            LastFailureCode = string.Empty;
            atx24PowerCableRoute?.SetRouteModeActive(
                IsAtx24PowerCableRouteMode);
            placementPreview?.Hide();
            if (!IsAtx24PowerCableRouteMode)
            {
                _placementRotationQuarterTurns = 0;
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdateAtx24PowerCableRoutePreview(
            Atx24PowerCableAssemblyItemBinding binding)
        {
            if (!IsAtx24PowerCableRouteMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentAtx24PowerCableRouteStatus =
                    Atx24PowerCableRouteStatus.ContextMissing;
                atx24PowerCableRoute?.SetRouteModeActive(active: false);
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyAtx24PowerCableRouteEvaluation(
                EvaluateAtx24PowerCableRoute(binding));
        }

        private Atx24PowerCableRouteEvaluation EvaluateAtx24PowerCableRoute(
            Atx24PowerCableAssemblyItemBinding binding)
        {
            Atx24PowerCableRouteProjection routeProjection =
                binding?.Route ?? atx24PowerCableRoute;
            if (routeProjection == null || binding?.Session == null)
            {
                return new Atx24PowerCableRouteEvaluation(
                    Atx24PowerCableRouteStatus.ContextMissing,
                    default,
                    false,
                    default);
            }

            GarageStockFlowSession session = binding.Session;
            PowerCableKeyOrientation orientation =
                (_placementRotationQuarterTurns % 2) == 0
                    ? PowerCableKeyOrientation.Keyed
                    : PowerCableKeyOrientation.Reversed;
            return routeProjection.EvaluateRoute(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                motor == null || motor.IsPaused,
                binding.IsAuthorityInHands && !binding.IsRouted,
                session.AssemblyBuild.MotherboardSeatState ==
                    AssemblySeatState.SeatedSecured,
                session.AssemblyBuild.PowerSupplyBayState ==
                    PowerSupplyBayState.PowerSupplyRetained,
                orientation);
        }

        private void ApplyAtx24PowerCableRouteEvaluation(
            Atx24PowerCableRouteEvaluation evaluation)
        {
            CurrentAtx24PowerCableRouteStatus = evaluation.Status;
            PlacementValid = evaluation.CanRoute;
            CurrentPlacementStatus = evaluation.CanRoute
                ? PlacementStatus.Valid
                : PlacementStatus.Blocked;
            CurrentStackSupport = null;
            LastFailureCode = evaluation.CanRoute
                ? string.Empty
                : evaluation.FailureCode;
            placementPreview?.Hide();
            SetCarryHandsState(blocked: !evaluation.CanRoute);
        }

        private bool ProcessAtx24PowerCableWorldInput()
        {
            if (resolver == null ||
                atx24PowerCableRoute == null ||
                atx24PowerCableBinding == null ||
                (!atx24PowerCableBinding.IsAuthorityLooseWorld &&
                 !atx24PowerCableBinding.IsRouted))
            {
                return false;
            }

            PhysicalItemProjection cable =
                atx24PowerCableBinding.PhysicalItem;
            if (atx24PowerCableBinding.IsRouted)
            {
                Atx24PowerCableRouteStatus routedFocus =
                    atx24PowerCableRoute.EvaluateRoutedFocus(
                        resolver.Origin,
                        transform,
                        cable,
                        obstructionMask,
                        motor != null && motor.IsPaused);
                if (routedFocus != Atx24PowerCableRouteStatus.ValidRoute)
                {
                    return false;
                }
            }
            else
            {
                OperationResult<PhysicalItemProjection> target =
                    resolver.Resolve();
                if (target.IsFailure || target.Value != cable)
                {
                    return false;
                }
            }

            FocusedCart = null;
            FocusedItem = cable;
            SetHandsState(VisibleHandsState.TargetFocused);
            if (input.TryConsumeInteractPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryPickup(FocusedItem);
            }

            return true;
        }

        private OperationResult TryPickupAtx24PowerCable(
            PhysicalItemProjection item,
            Atx24PowerCableAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.paused")));
            }

            bool wasRouted = binding.IsRouted;
            OperationResult physicalPickup = item.BeginCarry(
                carryAnchor,
                heldItemLayer);
            if (physicalPickup.IsFailure)
            {
                return Remember(physicalPickup);
            }

            OperationResult authority = wasRouted
                ? binding.TryCommitRoutedUnroute()
                : binding.TryCommitLoosePickup();
            if (authority.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                if (rollback.IsFailure)
                {
                    Debug.LogError(
                        $"ATX24_POWER_CABLE_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
                }

                binding.SyncProjectionToAuthority();
                return Remember(authority);
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor?.ApplyCarryProfile(item.CarryProfile);
            binding.SyncProjectionToAuthority();
            SetCarryHandsState(blocked: false);
            return physicalPickup;
        }

        private string GetHeldAtx24PowerCablePrompt(
            Atx24PowerCableAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsAtx24PowerCableRouteMode)
            {
                return $"{primary}: ATX24 rota önizlemesi • " +
                       $"{drop}: güvenli bırak • PSU 18+10 → ANAKART 24";
            }

            string state = GetAtx24PowerCableStatusLabel(
                CurrentAtx24PowerCableRouteStatus);
            string orientation = (_placementRotationQuarterTurns % 2) == 0
                ? "ANAHTAR HİZALI"
                : "ANAHTAR TERS";
            return PlacementValid
                ? $"[OK] ATX24 ROTA AÇIK • {orientation} • " +
                  $"{drop}: yönlendir • {rotate}: konektörü çevir • {primary}: çık"
                : $"[X] {state} • {rotate}: konektörü çevir • {primary}: çık";
        }

        private string GetFocusedAtx24PowerCablePrompt(
            Atx24PowerCableAssemblyItemBinding binding)
        {
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            return binding.IsRouted
                ? $"[ROUTE] PSU 18+10 → KANAL → ANAKART 24 • {interact}: çöz"
                : $"{interact}: {binding.PhysicalItem.DisplayName} al • 3 KONEKTÖR";
        }

        private static string GetAtx24PowerCableStatusLabel(
            Atx24PowerCableRouteStatus status)
        {
            return status switch
            {
                Atx24PowerCableRouteStatus.ValidRoute => "ROTA HAZIR",
                Atx24PowerCableRouteStatus.HostMotherboardUnsecured =>
                    "ANAKART SABİT DEĞİL",
                Atx24PowerCableRouteStatus.HostPowerSupplyUnretained =>
                    "PSU 4 VİDA İLE SABİT DEĞİL",
                Atx24PowerCableRouteStatus.OutOfRange => "YAKLAŞ",
                Atx24PowerCableRouteStatus.NotFocused =>
                    "24-PIN ANAKART GİRİŞİNİ HEDEFLE",
                Atx24PowerCableRouteStatus.LineOfSightBlocked => "GÖRÜŞÜ AÇ",
                Atx24PowerCableRouteStatus.OrientationInvalid =>
                    "KONEKTÖR ANAHTARI TERS",
                Atx24PowerCableRouteStatus.RouteObstructed =>
                    "KABLO KANALI ENGELLİ",
                Atx24PowerCableRouteStatus.QuerySaturated =>
                    "ROTA GÜVENLE DOĞRULANAMADI",
                Atx24PowerCableRouteStatus.Paused => "DURAKLATILDI",
                Atx24PowerCableRouteStatus.AuthorityBlocked =>
                    "KABLO ELDE DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private void ResetAtx24PowerCableState()
        {
            IsAtx24PowerCableRouteMode = false;
            CurrentAtx24PowerCableRouteStatus =
                Atx24PowerCableRouteStatus.ContextMissing;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            atx24PowerCableRoute?.ResetFeedback();
        }

        private static Atx24PowerCableAssemblyItemBinding
            GetAtx24PowerCableBinding(PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<Atx24PowerCableAssemblyItemBinding>()
                : null;
        }
    }
}
