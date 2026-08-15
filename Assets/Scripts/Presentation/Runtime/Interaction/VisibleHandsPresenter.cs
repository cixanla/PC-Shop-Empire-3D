using System;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum VisibleHandsState
    {
        Empty = 0,
        TargetFocused = 1,
        CarryingSmallItem = 2,
        DropBlocked = 3,
        Recovering = 4,
        CarryingLargeItem = 5,
        LargeDropBlocked = 6,
        DrivingTransportCart = 7
    }

    [DisallowMultipleComponent]
    public sealed class VisibleHandsPresenter : MonoBehaviour
    {
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        private Vector3 _leftIdlePosition;
        private Vector3 _rightIdlePosition;
        private Quaternion _leftIdleRotation;
        private Quaternion _rightIdleRotation;
        private bool _configured;

        public VisibleHandsState State { get; private set; }

        public void Configure(Transform left, Transform right)
        {
            leftHand = left != null ? left : throw new ArgumentNullException(nameof(left));
            rightHand = right != null ? right : throw new ArgumentNullException(nameof(right));
            CaptureIdlePose();
            SetState(VisibleHandsState.Empty);
        }

        public void SetState(VisibleHandsState state)
        {
            EnsureConfigured();
            State = state;
            Vector3 leftOffset;
            Vector3 rightOffset;
            Vector3 leftEuler;
            Vector3 rightEuler;
            switch (state)
            {
                case VisibleHandsState.TargetFocused:
                    leftOffset = new Vector3(0.015f, 0.02f, 0.035f);
                    rightOffset = new Vector3(-0.015f, 0.02f, 0.035f);
                    leftEuler = new Vector3(-3f, 3f, 0f);
                    rightEuler = new Vector3(-3f, -3f, 0f);
                    break;
                case VisibleHandsState.CarryingSmallItem:
                case VisibleHandsState.DropBlocked:
                    leftOffset = new Vector3(0.07f, 0.09f, 0.28f);
                    rightOffset = new Vector3(-0.07f, 0.09f, 0.28f);
                    leftEuler = new Vector3(-18f, 24f, -8f);
                    rightEuler = new Vector3(-18f, -24f, 8f);
                    break;
                case VisibleHandsState.CarryingLargeItem:
                case VisibleHandsState.LargeDropBlocked:
                    leftOffset = new Vector3(-0.18f, 0.06f, 0.33f);
                    rightOffset = new Vector3(0.18f, 0.06f, 0.33f);
                    leftEuler = new Vector3(-12f, -28f, 10f);
                    rightEuler = new Vector3(-12f, 28f, -10f);
                    break;
                case VisibleHandsState.DrivingTransportCart:
                    leftOffset = new Vector3(-0.08f, 0.12f, 0.40f);
                    rightOffset = new Vector3(0.08f, 0.12f, 0.40f);
                    leftEuler = new Vector3(-24f, -12f, 8f);
                    rightEuler = new Vector3(-24f, 12f, -8f);
                    break;
                case VisibleHandsState.Recovering:
                    leftOffset = new Vector3(0f, -0.05f, -0.04f);
                    rightOffset = new Vector3(0f, -0.05f, -0.04f);
                    leftEuler = Vector3.zero;
                    rightEuler = Vector3.zero;
                    break;
                default:
                    leftOffset = Vector3.zero;
                    rightOffset = Vector3.zero;
                    leftEuler = Vector3.zero;
                    rightEuler = Vector3.zero;
                    break;
            }

            leftHand.localPosition = _leftIdlePosition + leftOffset;
            rightHand.localPosition = _rightIdlePosition + rightOffset;
            leftHand.localRotation = _leftIdleRotation * Quaternion.Euler(leftEuler);
            rightHand.localRotation = _rightIdleRotation * Quaternion.Euler(rightEuler);
        }

        private void Awake()
        {
            EnsureConfigured();
        }

        private void CaptureIdlePose()
        {
            _leftIdlePosition = leftHand.localPosition;
            _rightIdlePosition = rightHand.localPosition;
            _leftIdleRotation = leftHand.localRotation;
            _rightIdleRotation = rightHand.localRotation;
            _configured = true;
        }

        private void EnsureConfigured()
        {
            if (_configured)
            {
                return;
            }

            if (leftHand == null || rightHand == null)
            {
                return;
            }

            CaptureIdlePose();
        }
    }
}
