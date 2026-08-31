using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct CustomPcPackagingBuildResult
        {
            public CustomPcPackagingBuildResult(
                CustomPcPackagingStationProjection packagingStation,
                CustomPcPackageDispatchProjection dispatchProjection,
                CustomPcPackagePhysicalBinding packageBinding,
                PhysicalItemProjection packageItem,
                Transform packagingAnchor,
                Transform dispatchAnchor,
                Collider packagingFocusCollider,
                Collider dispatchFocusCollider,
                TextMesh packagingStatusText,
                TextMesh dispatchStatusText,
                TextMesh packageLabel)
            {
                PackagingStation = packagingStation;
                DispatchProjection = dispatchProjection;
                PackageBinding = packageBinding;
                PackageItem = packageItem;
                PackagingAnchor = packagingAnchor;
                DispatchAnchor = dispatchAnchor;
                PackagingFocusCollider = packagingFocusCollider;
                DispatchFocusCollider = dispatchFocusCollider;
                PackagingStatusText = packagingStatusText;
                DispatchStatusText = dispatchStatusText;
                PackageLabel = packageLabel;
            }

            public CustomPcPackagingStationProjection PackagingStation { get; }
            public CustomPcPackageDispatchProjection DispatchProjection { get; }
            public CustomPcPackagePhysicalBinding PackageBinding { get; }
            public PhysicalItemProjection PackageItem { get; }
            public Transform PackagingAnchor { get; }
            public Transform DispatchAnchor { get; }
            public Collider PackagingFocusCollider { get; }
            public Collider DispatchFocusCollider { get; }
            public TextMesh PackagingStatusText { get; }
            public TextMesh DispatchStatusText { get; }
            public TextMesh PackageLabel { get; }
        }

        private static CustomPcPackagingBuildResult
            BuildCustomPcPackagingAndDispatch(
                Transform parent,
                Material metal,
                Material brushedSteel,
                Material accent,
                Material cardboard,
                Material rubber,
                Material labelPaper)
        {
            int interactable = RequireLayer(InteractableLayerName);
            int ignoreRaycast = RequireLayer("Ignore Raycast");

            Transform packaging = new GameObject(
                "CustomPcPackagingWorkbench").transform;
            packaging.SetParent(parent, false);
            packaging.localPosition = new Vector3(-3.28f, 0f, 0.50f);
            packaging.localRotation = Quaternion.Euler(0f, -90f, 0f);

            CreateBeveledCube(
                "PackagingWorkbenchBody",
                packaging,
                new Vector3(0f, 0.48f, 0f),
                new Vector3(1.28f, 0.92f, 1.02f),
                0.035f,
                metal);
            CreateBeveledCube(
                "PackagingWorkbenchTop",
                packaging,
                new Vector3(0f, 0.98f, 0f),
                new Vector3(1.42f, 0.10f, 1.16f),
                0.025f,
                brushedSteel);
            CreateDetailCube(
                "PackagingEsdMat",
                packaging,
                new Vector3(0f, 1.038f, 0f),
                new Vector3(1.18f, 0.018f, 0.88f),
                rubber);
            CreateDetailCube(
                "PackagingSafetyStripe",
                packaging,
                new Vector3(0f, 0.76f, -0.526f),
                new Vector3(1.02f, 0.11f, 0.018f),
                accent);

            Transform packagingAnchor = new GameObject(
                "CustomPcPackageWorkbenchAnchor").transform;
            packagingAnchor.SetParent(packaging, false);
            packagingAnchor.localPosition = new Vector3(0f, 1.42f, 0f);

            GameObject packagingFocus = new GameObject(
                "CustomPcPackagingFocusTarget");
            packagingFocus.transform.SetParent(packaging, false);
            packagingFocus.transform.localPosition =
                new Vector3(0f, 1.18f, -0.60f);
            packagingFocus.layer = interactable;
            BoxCollider packagingFocusCollider =
                packagingFocus.AddComponent<BoxCollider>();
            packagingFocusCollider.size = new Vector3(1.30f, 1.30f, 0.10f);
            packagingFocusCollider.isTrigger = true;

            TextMesh packagingStatus = new GameObject(
                "CustomPcPackagingStatusText").AddComponent<TextMesh>();
            packagingStatus.transform.SetParent(packaging, false);
            packagingStatus.transform.localPosition =
                new Vector3(0f, 1.75f, -0.592f);
            packagingStatus.anchor = TextAnchor.MiddleCenter;
            packagingStatus.alignment = TextAlignment.Center;
            packagingStatus.characterSize = 0.018f;
            packagingStatus.fontSize = 42;
            packagingStatus.color = new Color(1f, 0.76f, 0.42f);
            packagingStatus.text =
                "PAKETLEME İSTASYONU\nKALİTE ONAYI BEKLENİYOR";
            packagingStatus.gameObject.layer = ignoreRaycast;

            Transform dispatch = new GameObject(
                "CustomPcDispatchStaging").transform;
            dispatch.SetParent(parent, false);
            dispatch.localPosition = new Vector3(-2.35f, 0f, -4.12f);

            CreateBeveledCube(
                "DispatchStagingPlatform",
                dispatch,
                new Vector3(0f, 0.09f, 0f),
                new Vector3(1.48f, 0.18f, 1.12f),
                0.025f,
                brushedSteel);
            CreateDetailCube(
                "DispatchStagingMat",
                dispatch,
                new Vector3(0f, 0.192f, 0f),
                new Vector3(1.30f, 0.026f, 0.94f),
                rubber);
            CreateDetailCube(
                "DispatchBoundaryStripe",
                dispatch,
                new Vector3(0f, 0.205f, 0.47f),
                new Vector3(1.34f, 0.030f, 0.08f),
                accent);

            Transform dispatchAnchor = new GameObject(
                "CustomPcDispatchPackageAnchor").transform;
            dispatchAnchor.SetParent(dispatch, false);
            dispatchAnchor.localPosition = new Vector3(0f, 0.59f, 0f);
            dispatchAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject dispatchFocus = new GameObject(
                "CustomPcDispatchFocusTarget");
            dispatchFocus.transform.SetParent(dispatch, false);
            dispatchFocus.transform.localPosition = new Vector3(0f, 0.72f, 0.58f);
            dispatchFocus.layer = interactable;
            BoxCollider dispatchFocusCollider =
                dispatchFocus.AddComponent<BoxCollider>();
            dispatchFocusCollider.size = new Vector3(1.45f, 1.25f, 0.10f);
            dispatchFocusCollider.isTrigger = true;

            TextMesh dispatchStatus = new GameObject(
                "CustomPcDispatchStatusText").AddComponent<TextMesh>();
            dispatchStatus.transform.SetParent(dispatch, false);
            dispatchStatus.transform.localPosition =
                new Vector3(0f, 1.45f, 0.56f);
            dispatchStatus.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            dispatchStatus.anchor = TextAnchor.MiddleCenter;
            dispatchStatus.alignment = TextAlignment.Center;
            dispatchStatus.characterSize = 0.018f;
            dispatchStatus.fontSize = 42;
            dispatchStatus.color = new Color(0.95f, 0.78f, 0.48f);
            dispatchStatus.text =
                "SEVK SAHNESİ\nMÜHÜRLÜ CUSTOM PC BEKLENİYOR";
            dispatchStatus.gameObject.layer = ignoreRaycast;

            GameObject packageRoot = new GameObject("SealedCustomPcPackage");
            packageRoot.transform.SetParent(parent, false);
            packageRoot.transform.SetPositionAndRotation(
                packagingAnchor.position,
                packagingAnchor.rotation);
            packageRoot.layer = interactable;

            GameObject carton = CreateBeveledCube(
                "CustomPcPackageCarton",
                packageRoot.transform,
                Vector3.zero,
                new Vector3(1.00f, 0.72f, 0.66f),
                0.035f,
                cardboard);
            carton.layer = interactable;
            CreateDetailCube(
                "CustomPcPackageTape",
                packageRoot.transform,
                new Vector3(0f, 0.367f, 0f),
                new Vector3(0.17f, 0.014f, 0.62f),
                labelPaper);
            for (int side = -1; side <= 1; side += 2)
            {
                CreateDetailCube(
                    side < 0
                        ? "CustomPcPackageSealBandLeft"
                        : "CustomPcPackageSealBandRight",
                    packageRoot.transform,
                    new Vector3(side * 0.34f, 0f, -0.338f),
                    new Vector3(0.10f, 0.56f, 0.018f),
                    accent);
            }

            TextMesh packageLabel = new GameObject(
                "CustomPcPackageIdentityLabel").AddComponent<TextMesh>();
            packageLabel.transform.SetParent(packageRoot.transform, false);
            packageLabel.transform.localPosition = new Vector3(0f, 0f, -0.343f);
            packageLabel.anchor = TextAnchor.MiddleCenter;
            packageLabel.alignment = TextAlignment.Center;
            packageLabel.characterSize = 0.014f;
            packageLabel.fontSize = 40;
            packageLabel.color = new Color(0.10f, 0.075f, 0.045f);
            packageLabel.text = "CUSTOM PC\nKALİTE MÜHRÜ BEKLENİYOR";
            packageLabel.gameObject.layer = ignoreRaycast;

            Rigidbody packageBody = packageRoot.AddComponent<Rigidbody>();
            packageBody.mass = 12f;
            packageBody.useGravity = false;
            packageBody.isKinematic = true;
            packageBody.interpolation = RigidbodyInterpolation.Interpolate;
            packageBody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            PhysicalItemProjection packageItem =
                packageRoot.AddComponent<PhysicalItemProjection>();
            packageItem.Configure(
                GarageStockFlowSession.PrototypeCustomPcPackageIdValue,
                "Mühürlü Custom PC Paketi",
                packageBody,
                new Vector3(0.50f, 0.36f, 0.33f),
                new Vector3(0f, -0.28f, -0.32f),
                Vector3.zero,
                PhysicalCarryProfile.LargeBox);
            CustomPcPackagePhysicalBinding packageBinding =
                packageRoot.AddComponent<CustomPcPackagePhysicalBinding>();
            CustomPcPackagingStationProjection packagingStation =
                packaging.gameObject.AddComponent<
                    CustomPcPackagingStationProjection>();
            CustomPcPackageDispatchProjection dispatchProjection =
                dispatch.gameObject.AddComponent<
                    CustomPcPackageDispatchProjection>();

            SetLayerRecursively(packaging.gameObject, ignoreRaycast);
            packagingFocus.layer = interactable;
            SetLayerRecursively(dispatch.gameObject, ignoreRaycast);
            dispatchFocus.layer = interactable;
            packageRoot.SetActive(false);

            return new CustomPcPackagingBuildResult(
                packagingStation,
                dispatchProjection,
                packageBinding,
                packageItem,
                packagingAnchor,
                dispatchAnchor,
                packagingFocusCollider,
                dispatchFocusCollider,
                packagingStatus,
                dispatchStatus,
                packageLabel);
        }
    }
}
