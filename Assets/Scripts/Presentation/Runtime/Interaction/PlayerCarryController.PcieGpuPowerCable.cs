using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        [SerializeField] private PcieGpuPowerCableRouteProjection pcieGpuPowerCableRoute;
        [SerializeField] private PcieGpuPowerCableAssemblyItemBinding pcieGpuPowerCableBinding;

        public bool IsPcieGpuPowerCableRouteMode { get; private set; }

        public PcieGpuPowerCableRouteStatus CurrentPcieGpuPowerCableRouteStatus
        {
            get;
            private set;
        } = PcieGpuPowerCableRouteStatus.ContextMissing;

        public void ConfigurePcieGpuPowerCableRoute(
            PcieGpuPowerCableRouteProjection routeProjection,
            PcieGpuPowerCableAssemblyItemBinding assemblyBinding)
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
                    "The PCIe GPU binding must own the configured route.",
                    nameof(assemblyBinding));
            }

            pcieGpuPowerCableRoute = routeProjection;
            pcieGpuPowerCableBinding = assemblyBinding;
            pcieGpuPowerCableBinding.SyncProjectionToAuthority();
        }

        public bool MatchesPcieGpuPowerCableConfiguration(
            PcieGpuPowerCableRouteProjection routeProjection,
            PcieGpuPowerCableAssemblyItemBinding assemblyBinding)
        {
            return routeProjection != null &&
                   assemblyBinding != null &&
                   pcieGpuPowerCableRoute == routeProjection &&
                   pcieGpuPowerCableBinding == assemblyBinding &&
                   assemblyBinding.Route == routeProjection;
        }

        public OperationResult TrySetPcieGpuPowerCableRouteMode(bool enabled)
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.paused")));
            }

            if (enabled && pcieGpuPowerCableBuildKit != null &&
                pcieGpuPowerCableBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-pcie-gpu-power-cable-build-kit.authority-blocked")));
            }

            SetPcieGpuPowerCableRouteMode(enabled);
            if (enabled)
            {
                UpdatePcieGpuPowerCableRoutePreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotatePcieGpuPowerCableConnectorPreview()
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.nothing-held")));
            }

            if (!IsPcieGpuPowerCableRouteMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdatePcieGpuPowerCableRoutePreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmPcieGpuPowerCableRoute()
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.nothing-held")));
            }

            if (!IsPcieGpuPowerCableRouteMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.mode-inactive")));
            }

            return TryConfirmPcieGpuPowerCableRoute(
                binding,
                EvaluatePcieGpuPowerCableRoute(binding));
        }

        private OperationResult TryConfirmPcieGpuPowerCableRoute(
            PcieGpuPowerCableAssemblyItemBinding binding,
            PcieGpuPowerCableRouteEvaluation evaluation)
        {
            ApplyPcieGpuPowerCableRouteEvaluation(evaluation);
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

        private bool ProcessHeldPcieGpuPowerCableInput()
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            PcieGpuPowerCableBuildKitEvaluation buildKitEvaluation =
                EvaluatePcieGpuPowerCableBuildKit(binding);
            bool buildKitOwnsPrimary =
                IsPcieGpuPowerCableBuildKitMode ||
                (pcieGpuPowerCableBuildKit != null &&
                 pcieGpuPowerCableBuildKit.HasPickupReceipt);

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (buildKitOwnsPrimary)
                {
                    TrySetPcieGpuPowerCableBuildKitMode(
                        !IsPcieGpuPowerCableBuildKitMode);
                }
                else
                {
                    TrySetPcieGpuPowerCableRouteMode(
                        !IsPcieGpuPowerCableRouteMode);
                }
                return true;
            }

            if (IsPcieGpuPowerCableBuildKitMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotatePcieGpuPowerCableBuildKitPreviewClockwise();
                return true;
            }

            if (IsPcieGpuPowerCableRouteMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotatePcieGpuPowerCableConnectorPreview();
                return true;
            }

            PcieGpuPowerCableRouteEvaluation evaluation = default;
            if (IsPcieGpuPowerCableBuildKitMode)
            {
                ApplyPcieGpuPowerCableBuildKitEvaluation(buildKitEvaluation);
            }
            else if (!IsPcieGpuPowerCableRouteMode)
            {
                pcieGpuPowerCableRoute?.SetRouteModeActive(active: false);
            }
            else
            {
                evaluation = EvaluatePcieGpuPowerCableRoute(binding);
                ApplyPcieGpuPowerCableRouteEvaluation(evaluation);
            }

            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                if (IsPcieGpuPowerCableBuildKitMode)
                {
                    TryConfirmPcieGpuPowerCableBuildKit();
                }
                else if (IsPcieGpuPowerCableRouteMode)
                {
                    TryConfirmPcieGpuPowerCableRoute(binding, evaluation);
                }
                else
                {
                    TryDrop();
                }
            }

            return true;
        }

        private void SetPcieGpuPowerCableRouteMode(bool enabled)
        {
            ResetPcieGpuPowerCableBuildKitState();
            IsPcieGpuPowerCableRouteMode = enabled &&
                                         HeldItem != null &&
                                         GetPcieGpuPowerCableBinding(HeldItem) != null;
            IsAtx24PowerCableRouteMode = false;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            IsEps12vPowerCableRouteMode = false;
            eps12vPowerCableRoute?.SetRouteModeActive(active: false);
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
            CurrentPcieGpuPowerCableRouteStatus =
                PcieGpuPowerCableRouteStatus.ContextMissing;
            LastFailureCode = string.Empty;
            pcieGpuPowerCableRoute?.SetRouteModeActive(
                IsPcieGpuPowerCableRouteMode);
            placementPreview?.Hide();
            if (!IsPcieGpuPowerCableRouteMode)
            {
                _placementRotationQuarterTurns = 0;
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdatePcieGpuPowerCableRoutePreview(
            PcieGpuPowerCableAssemblyItemBinding binding)
        {
            if (!IsPcieGpuPowerCableRouteMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentPcieGpuPowerCableRouteStatus =
                    PcieGpuPowerCableRouteStatus.ContextMissing;
                pcieGpuPowerCableRoute?.SetRouteModeActive(active: false);
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyPcieGpuPowerCableRouteEvaluation(
                EvaluatePcieGpuPowerCableRoute(binding));
        }

        private PcieGpuPowerCableRouteEvaluation EvaluatePcieGpuPowerCableRoute(
            PcieGpuPowerCableAssemblyItemBinding binding)
        {
            PcieGpuPowerCableRouteProjection routeProjection =
                binding?.Route ?? pcieGpuPowerCableRoute;
            if (routeProjection == null || binding?.Session == null)
            {
                return new PcieGpuPowerCableRouteEvaluation(
                    PcieGpuPowerCableRouteStatus.ContextMissing,
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
                session.AssemblyBuild.GraphicsCardSlotState ==
                    GraphicsCardSlotState.GraphicsCardRetained,
                orientation);
        }

        private void ApplyPcieGpuPowerCableRouteEvaluation(
            PcieGpuPowerCableRouteEvaluation evaluation)
        {
            CurrentPcieGpuPowerCableRouteStatus = evaluation.Status;
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

        private bool ProcessPcieGpuPowerCableWorldInput()
        {
            if (resolver == null ||
                pcieGpuPowerCableRoute == null ||
                pcieGpuPowerCableBinding == null ||
                (!pcieGpuPowerCableBinding.IsAuthorityLooseWorld &&
                 !pcieGpuPowerCableBinding.IsRouted))
            {
                return false;
            }

            PhysicalItemProjection cable =
                pcieGpuPowerCableBinding.PhysicalItem;
            if (pcieGpuPowerCableBinding.IsRouted)
            {
                PcieGpuPowerCableRouteStatus routedFocus =
                    pcieGpuPowerCableRoute.EvaluateRoutedFocus(
                        resolver.Origin,
                        transform,
                        cable,
                        obstructionMask,
                        motor != null && motor.IsPaused);
                if (routedFocus != PcieGpuPowerCableRouteStatus.ValidRoute)
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

        private OperationResult TryPickupPcieGpuPowerCable(
            PhysicalItemProjection item,
            PcieGpuPowerCableAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.paused")));
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
                        $"PCIe GPU_POWER_CABLE_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
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

        private string GetHeldPcieGpuPowerCablePrompt(
            PcieGpuPowerCableAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsPcieGpuPowerCableRouteMode)
            {
                return $"{primary}: PCIe GPU rota önizlemesi • " +
                       $"{drop}: güvenli bırak • PSU PCIe 8 → GPU 6+2";
            }

            string state = GetPcieGpuPowerCableStatusLabel(
                CurrentPcieGpuPowerCableRouteStatus);
            string orientation = (_placementRotationQuarterTurns % 2) == 0
                ? "ANAHTAR HİZALI"
                : "ANAHTAR TERS";
            return PlacementValid
                ? $"[OK] PCIe GPU ROTA AÇIK • {orientation} • " +
                  $"{drop}: yönlendir • {rotate}: konektörü çevir • {primary}: çık"
                : $"[X] {state} • {rotate}: konektörü çevir • {primary}: çık";
        }

        private string GetFocusedPcieGpuPowerCablePrompt(
            PcieGpuPowerCableAssemblyItemBinding binding)
        {
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            return binding.IsRouted
                ? $"[ROUTE] PSU PCIe 8 → KANAL → GPU 6+2 • {interact}: çöz"
                : $"{interact}: {binding.PhysicalItem.DisplayName} al • ANAHTARLI 6+2-PIN";
        }

        private static string GetPcieGpuPowerCableStatusLabel(
            PcieGpuPowerCableRouteStatus status)
        {
            return status switch
            {
                PcieGpuPowerCableRouteStatus.ValidRoute => "ROTA HAZIR",
                PcieGpuPowerCableRouteStatus.HostMotherboardUnsecured =>
                    "ANAKART SABİT DEĞİL",
                PcieGpuPowerCableRouteStatus.HostPowerSupplyUnretained =>
                    "PSU 4 VİDA İLE SABİT DEĞİL",
                PcieGpuPowerCableRouteStatus.HostGraphicsCardUnretained =>
                    "EKRAN KARTI MANDAL VE BRAKETLE SABİT DEĞİL",
                PcieGpuPowerCableRouteStatus.OutOfRange => "YAKLAŞ",
                PcieGpuPowerCableRouteStatus.NotFocused =>
                    "GPU 6+2-PIN GÜÇ GİRİŞİNİ HEDEFLE",
                PcieGpuPowerCableRouteStatus.LineOfSightBlocked => "GÖRÜŞÜ AÇ",
                PcieGpuPowerCableRouteStatus.OrientationInvalid =>
                    "KONEKTÖR ANAHTARI TERS",
                PcieGpuPowerCableRouteStatus.RouteObstructed =>
                    "KABLO KANALI ENGELLİ",
                PcieGpuPowerCableRouteStatus.QuerySaturated =>
                    "ROTA GÜVENLE DOĞRULANAMADI",
                PcieGpuPowerCableRouteStatus.Paused => "DURAKLATILDI",
                PcieGpuPowerCableRouteStatus.AuthorityBlocked =>
                    "KABLO ELDE DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private void ResetPcieGpuPowerCableState()
        {
            IsPcieGpuPowerCableRouteMode = false;
            CurrentPcieGpuPowerCableRouteStatus =
                PcieGpuPowerCableRouteStatus.ContextMissing;
            pcieGpuPowerCableRoute?.SetRouteModeActive(active: false);
            pcieGpuPowerCableRoute?.ResetFeedback();
        }

        private static PcieGpuPowerCableAssemblyItemBinding
            GetPcieGpuPowerCableBinding(PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<PcieGpuPowerCableAssemblyItemBinding>()
                : null;
        }
    }
}
