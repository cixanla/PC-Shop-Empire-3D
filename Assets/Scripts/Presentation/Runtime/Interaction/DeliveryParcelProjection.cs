using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum DeliveryParcelState
    {
        Sealed = 0,
        Opened = 1
    }

    /// <summary>
    /// Visual-only state for the outer delivery parcel. The manifest fixes the contained item identity,
    /// and acceptance creates its Inventory record; opening only changes how that projection is presented.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicalItemProjection))]
    public sealed class DeliveryParcelProjection : MonoBehaviour
    {
        [SerializeField] private PhysicalItemProjection itemProjection;
        [SerializeField] private GameObject sealedVisualRoot;
        [SerializeField] private GameObject productVisualRoot;
        [SerializeField] private GameObject openedShellVisualRoot;
        [SerializeField] private DeliveryParcelState state = DeliveryParcelState.Sealed;

        public DeliveryParcelState State => state;

        public bool IsSealed => state == DeliveryParcelState.Sealed;

        public bool IsOpened => state == DeliveryParcelState.Opened;

        public int OpenTransitionCount { get; private set; }

        public PhysicalItemProjection ItemProjection => itemProjection;

        public GameObject SealedVisualRoot => sealedVisualRoot;

        public GameObject ProductVisualRoot => productVisualRoot;

        public GameObject OpenedShellVisualRoot => openedShellVisualRoot;

        public string StateLabel => IsOpened ? "AÇILDI • ÜRÜN HAZIR" : "KAPALI";

        public void Configure(
            PhysicalItemProjection projection,
            GameObject sealedVisual,
            GameObject productVisual,
            GameObject openedShellVisual)
        {
            itemProjection = projection != null
                ? projection
                : throw new System.ArgumentNullException(nameof(projection));
            sealedVisualRoot = sealedVisual != null
                ? sealedVisual
                : throw new System.ArgumentNullException(nameof(sealedVisual));
            productVisualRoot = productVisual != null
                ? productVisual
                : throw new System.ArgumentNullException(nameof(productVisual));
            openedShellVisualRoot = openedShellVisual != null
                ? openedShellVisual
                : throw new System.ArgumentNullException(nameof(openedShellVisual));
            state = DeliveryParcelState.Sealed;
            OpenTransitionCount = 0;
            ApplyVisualState();
        }

        public OperationResult TryOpen()
        {
            OperationResult contract = ValidateContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            if (IsOpened)
            {
                return OperationResult.Success();
            }

            state = DeliveryParcelState.Opened;
            OpenTransitionCount++;
            ApplyVisualState();
            itemProjection.RecordSafePose();
            Physics.SyncTransforms();
            return OperationResult.Success();
        }

        public OperationResult ValidateContract()
        {
            itemProjection ??= GetComponent<PhysicalItemProjection>();
            if (itemProjection == null)
            {
                return OperationResult.Fail(DeliveryParcelFailures.ItemProjectionMissing);
            }

            if (sealedVisualRoot == null || productVisualRoot == null || openedShellVisualRoot == null)
            {
                return OperationResult.Fail(DeliveryParcelFailures.VisualMissing);
            }

            if (!sealedVisualRoot.transform.IsChildOf(transform) ||
                !productVisualRoot.transform.IsChildOf(transform) ||
                openedShellVisualRoot.transform.IsChildOf(transform) ||
                sealedVisualRoot == productVisualRoot)
            {
                return OperationResult.Fail(DeliveryParcelFailures.InvalidVisualHierarchy);
            }

            return OperationResult.Success();
        }

        private void Awake()
        {
            itemProjection ??= GetComponent<PhysicalItemProjection>();
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (sealedVisualRoot != null)
            {
                sealedVisualRoot.SetActive(IsSealed);
            }

            if (productVisualRoot != null)
            {
                productVisualRoot.SetActive(IsOpened);
            }

            if (openedShellVisualRoot != null)
            {
                openedShellVisualRoot.SetActive(IsOpened);
            }
        }
    }

    public static class DeliveryParcelFailures
    {
        public static readonly Failure ItemProjectionMissing =
            Failure.FromCode("delivery-parcel.item-projection-missing");
        public static readonly Failure VisualMissing =
            Failure.FromCode("delivery-parcel.visual-missing");
        public static readonly Failure InvalidVisualHierarchy =
            Failure.FromCode("delivery-parcel.invalid-visual-hierarchy");
    }
}
