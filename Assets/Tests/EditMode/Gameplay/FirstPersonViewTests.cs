using NUnit.Framework;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay
{
    public sealed class FirstPersonViewTests
    {
        [Test]
        public void ViewSettingsClampToAccessibleSupportedRanges()
        {
            var settings = new FirstPersonViewSettings();

            settings.Set(-100f, -2f, -5f, true, false);
            Assert.That(settings.FieldOfView, Is.EqualTo(FirstPersonViewSettings.MinimumFieldOfView));
            Assert.That(settings.MouseSensitivity, Is.EqualTo(FirstPersonViewSettings.MinimumMouseSensitivity));
            Assert.That(settings.GamepadLookSpeed, Is.EqualTo(FirstPersonViewSettings.MinimumGamepadLookSpeed));
            Assert.That(settings.InvertY, Is.True);
            Assert.That(settings.MotionReduced, Is.False);

            settings.Set(1000f, 10f, 5000f, false, true);
            Assert.That(settings.FieldOfView, Is.EqualTo(FirstPersonViewSettings.MaximumFieldOfView));
            Assert.That(settings.MouseSensitivity, Is.EqualTo(FirstPersonViewSettings.MaximumMouseSensitivity));
            Assert.That(settings.GamepadLookSpeed, Is.EqualTo(FirstPersonViewSettings.MaximumGamepadLookSpeed));
            Assert.That(settings.InvertY, Is.False);
            Assert.That(settings.MotionReduced, Is.True);
        }

        [Test]
        public void MouseDeltaAndGamepadRateUseDifferentTimeContracts()
        {
            var settings = new FirstPersonViewSettings();
            settings.Set(72f, 0.08f, 160f, false, true);

            Vector2 mouse = FirstPersonMath.CalculateLookDegrees(
                new Vector2(10f, 5f),
                true,
                0.5f,
                settings);
            Vector2 gamepad = FirstPersonMath.CalculateLookDegrees(
                Vector2.one,
                false,
                0.5f,
                settings);

            Assert.That(mouse, Is.EqualTo(new Vector2(0.8f, 0.4f)).Using(Vector2ComparerWithEqualsOperator.Instance));
            Assert.That(gamepad, Is.EqualTo(new Vector2(80f, 80f)).Using(Vector2ComparerWithEqualsOperator.Instance));
        }

        [Test]
        public void InvertPitchClampAndDiagonalMoveAreExplicit()
        {
            var settings = new FirstPersonViewSettings();
            settings.Set(72f, 0.1f, 100f, true, true);

            Vector2 look = FirstPersonMath.CalculateLookDegrees(Vector2.one, false, 1f, settings);
            Vector2 move = FirstPersonMath.ClampMoveInput(Vector2.one);

            Assert.That(look, Is.EqualTo(new Vector2(100f, -100f)).Using(Vector2ComparerWithEqualsOperator.Instance));
            Assert.That(FirstPersonMath.ClampPitch(500f), Is.EqualTo(FirstPersonMath.MaximumPitch));
            Assert.That(FirstPersonMath.ClampPitch(-500f), Is.EqualTo(FirstPersonMath.MinimumPitch));
            Assert.That(move.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void LargeCarrySuppressesSprintAndMotionReductionDisablesLensPenalty()
        {
            GameObject rig = new GameObject("MotorTestRig");
            try
            {
                CharacterController controller = rig.AddComponent<CharacterController>();
                PlayerInputAdapter input = rig.AddComponent<PlayerInputAdapter>();
                Transform pivot = new GameObject("CameraPivot").transform;
                pivot.SetParent(rig.transform, false);
                Camera camera = new GameObject("Camera").AddComponent<Camera>();
                camera.transform.SetParent(pivot, false);
                FirstPersonMotor motor = rig.AddComponent<FirstPersonMotor>();
                motor.Configure(controller, input, pivot, camera);
                motor.ViewSettings.Set(72f, 0.08f, 160f, false, false);
                motor.ApplyViewSettings();

                motor.ApplyCarryProfile(PhysicalCarryProfile.LargeBox);

                Assert.That(motor.ActiveCarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));
                Assert.That(motor.CarryAllowsSprint, Is.False);
                Assert.That(motor.CarryMovementSpeedMultiplier, Is.EqualTo(0.65f).Within(0.001f));
                Assert.That(motor.ResolveHorizontalSpeed(false), Is.EqualTo(2.275f).Within(0.001f));
                Assert.That(motor.ResolveHorizontalSpeed(true), Is.EqualTo(2.275f).Within(0.001f));
                Assert.That(motor.RequestedCarryFieldOfViewPenalty, Is.EqualTo(6f).Within(0.001f));
                Assert.That(motor.AppliedCarryFieldOfViewPenalty, Is.EqualTo(6f).Within(0.001f));
                Assert.That(motor.TargetFieldOfView, Is.EqualTo(66f).Within(0.001f));

                motor.ViewSettings.Set(72f, 0.08f, 160f, false, true);
                motor.ApplyViewSettings();
                Assert.That(motor.AppliedCarryFieldOfViewPenalty, Is.Zero);
                Assert.That(motor.TargetFieldOfView, Is.EqualTo(72f).Within(0.001f));
                Assert.That(camera.fieldOfView, Is.EqualTo(72f).Within(0.001f));

                motor.ClearCarryProfile();
                Assert.That(motor.ActiveCarryProfile, Is.Null);
                Assert.That(motor.ResolveHorizontalSpeed(true), Is.EqualTo(5.2f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(rig);
            }
        }
    }
}
