using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicalItemProjection))]
    public sealed class InventoryItemWorldBinding : MonoBehaviour
    {
        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection projection;
        [SerializeField] private string inventoryItemId = GarageStockFlowSession.ItemInstanceIdValue;

        private bool _hasPreparedTransfer;
        private StableId<ContainerIdScope> _preparedSource;
        private StableId<ContainerIdScope> _preparedTarget;
        private StableId<ContainerIdScope> _lastWorldContainer;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection Projection => projection;

        public StableId<ItemInstanceIdScope> InventoryItemId =>
            StableId<ItemInstanceIdScope>.Parse(inventoryItemId);

        public bool HasPreparedTransfer => _hasPreparedTransfer;

        public bool RequiresAcceptance
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.Order.Status == PurchaseOrderStatus.Arrived &&
                       !session.TryGetItem(out _);
            }
        }

        public string LocationLabel
        {
            get
            {
                GarageStockFlowSession session = Session;
                if (session == null)
                {
                    return "AUTHORITY EKSİK";
                }

                if (!session.TryGetItem(out InventoryItemRecord item))
                {
                    return session.Order.Status == PurchaseOrderStatus.Arrived
                        ? "KABUL BEKLİYOR • STOK 0"
                        : "STOK KAYDI YOK";
                }

                return session.Inventory.TryGetContainer(
                    item.ContainerId,
                    out InventoryContainerDefinition container)
                    ? ContainerLabel(container.Kind)
                    : "KONUM HATASI";
            }
        }

        private GarageStockFlowSession Session => runtime != null
            ? runtime.EnsureInitialized()
            : null;

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            string stableInventoryItemId)
        {
            runtime = stockFlowRuntime != null
                ? stockFlowRuntime
                : throw new System.ArgumentNullException(nameof(stockFlowRuntime));
            projection = itemProjection != null
                ? itemProjection
                : throw new System.ArgumentNullException(nameof(itemProjection));
            inventoryItemId = StableId<ItemInstanceIdScope>.Parse(stableInventoryItemId).Value;
        }

        public OperationResult TryAcceptDelivery()
        {
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            GarageStockFlowSession session = Session;
            if (!RequiresAcceptance)
            {
                return OperationResult.Fail(StockProjectionFailures.AcceptanceUnavailable);
            }

            OperationResult result = session.AcceptArrivedDelivery();
            if (result.IsSuccess)
            {
                _lastWorldContainer = session.ReceivingContainerId;
                runtime.RefreshPresentation();
            }

            return result;
        }

        public OperationResult TryPreparePickupTransfer()
        {
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            GarageStockFlowSession session = Session;
            if (!session.TryGetItem(out InventoryItemRecord item))
            {
                return OperationResult.Fail(StockProjectionFailures.ItemNotAccepted);
            }

            _lastWorldContainer = item.ContainerId;
            return TryPrepareTransfer(session.HandsContainerId);
        }

        public OperationResult TryPreparePlacementTransfer(PlacementSurface surface)
        {
            if (surface == null)
            {
                return OperationResult.Fail(StockProjectionFailures.PlacementZoneMissing);
            }

            InventoryPlacementZone zone = surface.GetComponent<InventoryPlacementZone>();
            if (zone == null || zone.ContainerKind != InventoryContainerKind.Shelf)
            {
                return OperationResult.Fail(StockProjectionFailures.PlacementZoneMissing);
            }

            return TryPrepareTransfer(zone.ContainerId);
        }

        public OperationResult TryPrepareDropTransfer()
        {
            GarageStockFlowSession session = Session;
            return session == null
                ? OperationResult.Fail(StockProjectionFailures.RuntimeMissing)
                : TryPrepareTransfer(session.WorldFloorContainerId);
        }

        public OperationResult TryPrepareRecoveryTransfer()
        {
            if (_lastWorldContainer.IsEmpty)
            {
                return OperationResult.Fail(StockProjectionFailures.RecoveryContainerMissing);
            }

            return TryPrepareTransfer(_lastWorldContainer);
        }

        public OperationResult CommitPreparedTransfer(bool targetIsWorld)
        {
            if (!_hasPreparedTransfer)
            {
                return OperationResult.Fail(StockProjectionFailures.TransactionMissing);
            }

            if (targetIsWorld)
            {
                _lastWorldContainer = _preparedTarget;
            }

            ClearPreparedTransfer();
            runtime.RefreshPresentation();
            return OperationResult.Success();
        }

        public OperationResult RollbackPreparedTransfer()
        {
            if (!_hasPreparedTransfer)
            {
                return OperationResult.Fail(StockProjectionFailures.TransactionMissing);
            }

            GarageStockFlowSession session = Session;
            OperationResult rollback = session.TransferItem(_preparedSource);
            if (rollback.IsFailure)
            {
                return OperationResult.Fail(StockProjectionFailures.RollbackFailed);
            }

            ClearPreparedTransfer();
            runtime.RefreshPresentation();
            return OperationResult.Success();
        }

        private OperationResult TryPrepareTransfer(StableId<ContainerIdScope> targetContainer)
        {
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            if (_hasPreparedTransfer)
            {
                return OperationResult.Fail(StockProjectionFailures.TransactionPending);
            }

            GarageStockFlowSession session = Session;
            if (!session.TryGetItem(out InventoryItemRecord item))
            {
                return OperationResult.Fail(StockProjectionFailures.ItemNotAccepted);
            }

            OperationResult transfer = session.TransferItem(targetContainer);
            if (transfer.IsFailure)
            {
                return transfer;
            }

            _preparedSource = item.ContainerId;
            _preparedTarget = targetContainer;
            _hasPreparedTransfer = true;
            return OperationResult.Success();
        }

        private OperationResult ValidateContract()
        {
            if (runtime == null || Session == null)
            {
                return OperationResult.Fail(StockProjectionFailures.RuntimeMissing);
            }

            projection ??= GetComponent<PhysicalItemProjection>();
            if (projection == null)
            {
                return OperationResult.Fail(StockProjectionFailures.ProjectionMissing);
            }

            return projection.ItemIdValue == InventoryItemId.Value &&
                   InventoryItemId == Session.ItemId
                ? OperationResult.Success()
                : OperationResult.Fail(StockProjectionFailures.IdentityMismatch);
        }

        private void ClearPreparedTransfer()
        {
            _hasPreparedTransfer = false;
            _preparedSource = default;
            _preparedTarget = default;
        }

        private void Awake()
        {
            projection ??= GetComponent<PhysicalItemProjection>();
            _ = InventoryItemId;
        }

        private static string ContainerLabel(InventoryContainerKind kind)
        {
            return kind switch
            {
                InventoryContainerKind.Receiving => "KABUL ALANI • STOK 1",
                InventoryContainerKind.ActorHands => "OYUNCU ELİNDE • STOK 1",
                InventoryContainerKind.Shelf => "RAF A • STOK 1",
                InventoryContainerKind.WorldFloor => "GÜVENLİ ZEMİN • STOK 1",
                _ => $"{kind.ToString().ToUpperInvariant()} • STOK 1"
            };
        }
    }

    public static class StockProjectionFailures
    {
        public static readonly Failure RuntimeMissing = Failure.FromCode("stock-projection.runtime-missing");
        public static readonly Failure ProjectionMissing = Failure.FromCode("stock-projection.item-missing");
        public static readonly Failure IdentityMismatch = Failure.FromCode("stock-projection.identity-mismatch");
        public static readonly Failure ItemNotAccepted = Failure.FromCode("stock-projection.item-not-accepted");
        public static readonly Failure AcceptanceUnavailable = Failure.FromCode("stock-projection.acceptance-unavailable");
        public static readonly Failure PlacementZoneMissing = Failure.FromCode("stock-projection.placement-zone-missing");
        public static readonly Failure RecoveryContainerMissing = Failure.FromCode("stock-projection.recovery-container-missing");
        public static readonly Failure TransactionPending = Failure.FromCode("stock-projection.transaction-pending");
        public static readonly Failure TransactionMissing = Failure.FromCode("stock-projection.transaction-missing");
        public static readonly Failure RollbackFailed = Failure.FromCode("stock-projection.rollback-failed");
    }
}
