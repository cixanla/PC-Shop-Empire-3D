using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        [SerializeField] private Eps12vPowerCableRouteProjection eps12vPowerCableRoute;
        [SerializeField] private Eps12vPowerCableAssemblyItemBinding eps12vPowerCableBinding;

        public bool IsEps12vPowerCableRouteMode { get; private set; }

        public Eps12vPowerCableRouteStatus CurrentEps12vPowerCableRouteStatus
        {
            get;
            private set;
        } = Eps12vPowerCableRouteStatus.ContextMissing;

        public void ConfigureEps12vPowerCableRoute(
            Eps12vPowerCableRouteProjection routeProjection,
            Eps12vPowerCableAssemblyItemBinding assemblyBinding)
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
                    "The EPS12V binding must own the configured route.",
                    nameof(assemblyBinding));
            }

            eps12vPowerCableRoute = routeProjection;
            eps12vPowerCableBinding = assemblyBinding;
            eps12vPowerCableBinding.SyncProjectionToAuthority();
        }

        public bool MatchesEps12vPowerCableConfiguration(
            Eps12vPowerCableRouteProjection routeProjection,
            Eps12vPowerCableAssemblyItemBinding assemblyBinding)
        {
            return routeProjection != null &&
                   assemblyBinding != null &&
                   eps12vPowerCableRoute == routeProjection &&
                   eps12vPowerCableBinding == assemblyBinding &&
                   assemblyBinding.Route == routeProjection;
        }

        public OperationResult TrySetEps12vPowerCableRouteMode(bool enabled)
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.paused")));
            }

            if (enabled && eps12vPowerCableBuildKit != null &&
                eps12vPowerCableBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-eps12v-power-cable-build-kit.authority-blocked")));
            }

            SetEps12vPowerCableRouteMode(enabled);
            if (enabled)
            {
                UpdateEps12vPowerCableRoutePreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateEps12vPowerCableConnectorPreview()
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.nothing-held")));
            }

            if (!IsEps12vPowerCableRouteMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateEps12vPowerCableRoutePreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmEps12vPowerCableRoute()
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.nothing-held")));
            }

            if (!IsEps12vPowerCableRouteMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.mode-inactive")));
            }

            return TryConfirmEps12vPowerCableRoute(
                binding,
                EvaluateEps12vPowerCableRoute(binding));
        }

        private OperationResult TryConfirmEps12vPowerCableRoute(
            Eps12vPowerCableAssemblyItemBinding binding,
            Eps12vPowerCableRouteEvaluation evaluation)
        {
            ApplyEps12vPowerCableRouteEvaluation(evaluation);
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

        private bool ProcessHeldEps12vPowerCableInput()
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            Eps12vPowerCableBuildKitEvaluation buildKitEvaluation =
                EvaluateEps12vPowerCableBuildKit(binding);
            bool buildKitOwnsPrimary =
                IsEps12vPowerCableBuildKitMode ||
                (eps12vPowerCableBuildKit != null &&
                 eps12vPowerCableBuildKit.HasPickupReceipt);

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (buildKitOwnsPrimary)
                {
                    TrySetEps12vPowerCableBuildKitMode(
                        !IsEps12vPowerCableBuildKitMode);
                }
                else
                {
                    TrySetEps12vPowerCableRouteMode(
                        !IsEps12vPowerCableRouteMode);
                }
                return true;
            }

            if (IsEps12vPowerCableBuildKitMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateEps12vPowerCableBuildKitPreviewClockwise();
                return true;
            }

            if (IsEps12vPowerCableRouteMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateEps12vPowerCableConnectorPreview();
                return true;
            }

            Eps12vPowerCableRouteEvaluation evaluation = default;
            if (IsEps12vPowerCableBuildKitMode)
            {
                ApplyEps12vPowerCableBuildKitEvaluation(buildKitEvaluation);
            }
            else if (!IsEps12vPowerCableRouteMode)
            {
                eps12vPowerCableRoute?.SetRouteModeActive(active: false);
            }
            else
            {
                evaluation = EvaluateEps12vPowerCableRoute(binding);
                ApplyEps12vPowerCableRouteEvaluation(evaluation);
            }

            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                if (IsEps12vPowerCableBuildKitMode)
                {
                    TryConfirmEps12vPowerCableBuildKit();
                }
                else if (IsEps12vPowerCableRouteMode)
                {
                    TryConfirmEps12vPowerCableRoute(binding, evaluation);
                }
                else
                {
                    TryDrop();
                }
            }

            return true;
        }

        private void SetEps12vPowerCableRouteMode(bool enabled)
        {
            ResetEps12vPowerCableBuildKitState();
            IsEps12vPowerCableRouteMode = enabled &&
                                         HeldItem != null &&
                                         GetEps12vPowerCableBinding(HeldItem) != null;
            IsAtx24PowerCableRouteMode = false;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            IsPcieGpuPowerCableRouteMode = false;
            pcieGpuPowerCableRoute?.SetRouteModeActive(active: false);
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
            CurrentEps12vPowerCableRouteStatus =
                Eps12vPowerCableRouteStatus.ContextMissing;
            LastFailureCode = string.Empty;
            eps12vPowerCableRoute?.SetRouteModeActive(
                IsEps12vPowerCableRouteMode);
            placementPreview?.Hide();
            if (!IsEps12vPowerCableRouteMode)
            {
                _placementRotationQuarterTurns = 0;
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdateEps12vPowerCableRoutePreview(
            Eps12vPowerCableAssemblyItemBinding binding)
        {
            if (!IsEps12vPowerCableRouteMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentEps12vPowerCableRouteStatus =
                    Eps12vPowerCableRouteStatus.ContextMissing;
                eps12vPowerCableRoute?.SetRouteModeActive(active: false);
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyEps12vPowerCableRouteEvaluation(
                EvaluateEps12vPowerCableRoute(binding));
        }

        private Eps12vPowerCableRouteEvaluation EvaluateEps12vPowerCableRoute(
            Eps12vPowerCableAssemblyItemBinding binding)
        {
            Eps12vPowerCableRouteProjection routeProjection =
                binding?.Route ?? eps12vPowerCableRoute;
            if (routeProjection == null || binding?.Session == null)
            {
                return new Eps12vPowerCableRouteEvaluation(
                    Eps12vPowerCableRouteStatus.ContextMissing,
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
                session.AssemblyBuild.ProcessorSocketState ==
                    ProcessorSocketState.ProcessorRetained,
                orientation);
        }

        private void ApplyEps12vPowerCableRouteEvaluation(
            Eps12vPowerCableRouteEvaluation evaluation)
        {
            CurrentEps12vPowerCableRouteStatus = evaluation.Status;
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

        private bool ProcessEps12vPowerCableWorldInput()
        {
            if (resolver == null ||
                eps12vPowerCableRoute == null ||
                eps12vPowerCableBinding == null ||
                (!eps12vPowerCableBinding.IsAuthorityLooseWorld &&
                 !eps12vPowerCableBinding.IsRouted))
            {
                return false;
            }

            PhysicalItemProjection cable =
                eps12vPowerCableBinding.PhysicalItem;
            if (eps12vPowerCableBinding.IsRouted)
            {
                Eps12vPowerCableRouteStatus routedFocus =
                    eps12vPowerCableRoute.EvaluateRoutedFocus(
                        resolver.Origin,
                        transform,
                        cable,
                        obstructionMask,
                        motor != null && motor.IsPaused);
                if (routedFocus != Eps12vPowerCableRouteStatus.ValidRoute)
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

        private OperationResult TryPickupEps12vPowerCable(
            PhysicalItemProjection item,
            Eps12vPowerCableAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.paused")));
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
                        $"EPS12V_POWER_CABLE_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
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

        private string GetHeldEps12vPowerCablePrompt(
            Eps12vPowerCableAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsEps12vPowerCableRouteMode)
            {
                return $"{primary}: EPS12V rota önizlemesi • " +
                       $"{drop}: güvenli bırak • PSU CPU 8 → ANAKART CPU 8";
            }

            string state = GetEps12vPowerCableStatusLabel(
                CurrentEps12vPowerCableRouteStatus);
            string orientation = (_placementRotationQuarterTurns % 2) == 0
                ? "ANAHTAR HİZALI"
                : "ANAHTAR TERS";
            return PlacementValid
                ? $"[OK] EPS12V ROTA AÇIK • {orientation} • " +
                  $"{drop}: yönlendir • {rotate}: konektörü çevir • {primary}: çık"
                : $"[X] {state} • {rotate}: konektörü çevir • {primary}: çık";
        }

        private string GetFocusedEps12vPowerCablePrompt(
            Eps12vPowerCableAssemblyItemBinding binding)
        {
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            return binding.IsRouted
                ? $"[ROUTE] PSU CPU 8 → KANAL → ANAKART CPU 8 • {interact}: çöz"
                : $"{interact}: {binding.PhysicalItem.DisplayName} al • 2 ANAHTARLI 8-PIN";
        }

        private static string GetEps12vPowerCableStatusLabel(
            Eps12vPowerCableRouteStatus status)
        {
            return status switch
            {
                Eps12vPowerCableRouteStatus.ValidRoute => "ROTA HAZIR",
                Eps12vPowerCableRouteStatus.HostMotherboardUnsecured =>
                    "ANAKART SABİT DEĞİL",
                Eps12vPowerCableRouteStatus.HostPowerSupplyUnretained =>
                    "PSU 4 VİDA İLE SABİT DEĞİL",
                Eps12vPowerCableRouteStatus.HostProcessorUnretained =>
                    "İŞLEMCİ MANDALI KİLİTLİ DEĞİL",
                Eps12vPowerCableRouteStatus.OutOfRange => "YAKLAŞ",
                Eps12vPowerCableRouteStatus.NotFocused =>
                    "CPU 8-PIN ANAKART GİRİŞİNİ HEDEFLE",
                Eps12vPowerCableRouteStatus.LineOfSightBlocked => "GÖRÜŞÜ AÇ",
                Eps12vPowerCableRouteStatus.OrientationInvalid =>
                    "KONEKTÖR ANAHTARI TERS",
                Eps12vPowerCableRouteStatus.RouteObstructed =>
                    "KABLO KANALI ENGELLİ",
                Eps12vPowerCableRouteStatus.QuerySaturated =>
                    "ROTA GÜVENLE DOĞRULANAMADI",
                Eps12vPowerCableRouteStatus.Paused => "DURAKLATILDI",
                Eps12vPowerCableRouteStatus.AuthorityBlocked =>
                    "KABLO ELDE DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private void ResetEps12vPowerCableState()
        {
            IsEps12vPowerCableRouteMode = false;
            CurrentEps12vPowerCableRouteStatus =
                Eps12vPowerCableRouteStatus.ContextMissing;
            eps12vPowerCableRoute?.SetRouteModeActive(active: false);
            eps12vPowerCableRoute?.ResetFeedback();
        }

        private static Eps12vPowerCableAssemblyItemBinding
            GetEps12vPowerCableBinding(PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<Eps12vPowerCableAssemblyItemBinding>()
                : null;
        }
    }
}
