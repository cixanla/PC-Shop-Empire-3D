using System;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlacementPreview : MonoBehaviour
    {
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        [SerializeField] private Renderer[] previewRenderers = Array.Empty<Renderer>();

        public bool IsVisible => gameObject.activeSelf;

        public bool IsShowingValidPose { get; private set; }

        public Pose CurrentPose => new Pose(transform.position, transform.rotation);

        public void Configure(Material valid, Material invalid)
        {
            validMaterial = valid != null ? valid : throw new ArgumentNullException(nameof(valid));
            invalidMaterial = invalid != null ? invalid : throw new ArgumentNullException(nameof(invalid));
            previewRenderers = GetComponentsInChildren<Renderer>(true);
            PrepareRenderers();
            Hide();
        }

        public void Show(PhysicalItemProjection source, PlacementEvaluation evaluation)
        {
            if (source == null || !evaluation.HasPose)
            {
                Hide();
                return;
            }

            transform.SetPositionAndRotation(evaluation.Pose.position, evaluation.Pose.rotation);
            transform.localScale = source.DropHalfExtents * 2f;
            IsShowingValidPose = evaluation.IsValid;
            Material material = evaluation.IsValid ? validMaterial : invalidMaterial;
            PrepareRenderers();
            foreach (Renderer previewRenderer in previewRenderers)
            {
                if (previewRenderer != null)
                {
                    previewRenderer.sharedMaterial = material;
                }
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            IsShowingValidPose = false;
            gameObject.SetActive(false);
        }

        private void PrepareRenderers()
        {
            if (previewRenderers == null || previewRenderers.Length == 0)
            {
                previewRenderers = GetComponentsInChildren<Renderer>(true);
            }

            foreach (Renderer previewRenderer in previewRenderers)
            {
                if (previewRenderer != null)
                {
                    previewRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    previewRenderer.receiveShadows = false;
                }
            }
        }
    }
}
