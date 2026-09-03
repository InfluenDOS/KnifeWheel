using System;
using KnifeWheel.Vehicle;
using NUnit.Framework;
using UnityEngine;

namespace KnifeWheel.Tests.EditMode
{
    public class MotorcycleDriveModelTests
    {
        private MotorcycleDriveSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new MotorcycleDriveSettings
            {
                Acceleration = 20f,
                MaxSpeed = 10f,
                BrakeDeceleration = 30f,
                CoastDeceleration = 2f,
                SteerYawAcceleration = 6f,
                MaxSteerYawRate = 2f,
                MinSpeedForSteer = 0.5f,
                FullSteerSpeed = 3f,
                StabilityAssist = 25f
            };
        }

        [Test]
        public void Throttle_ProducesPositiveForwardAcceleration_BelowMaxSpeed()
        {
            var input = new VehicleControlInput(1f, 0f, 0f);
            float accel = MotorcycleDriveModel.ComputeLongitudinalAcceleration(0f, in input, _settings);
            Assert.Greater(accel, 0f);
        }

        [Test]
        public void Brake_WhileMovingForward_ProducesNegativeAcceleration()
        {
            var input = new VehicleControlInput(0f, 1f, 0f);
            float accel = MotorcycleDriveModel.ComputeLongitudinalAcceleration(5f, in input, _settings);
            Assert.Less(accel, 0f);
        }

        [Test]
        public void Brake_AtStandstill_ProducesNoAcceleration()
        {
            var input = new VehicleControlInput(0f, 1f, 0f);
            float accel = MotorcycleDriveModel.ComputeLongitudinalAcceleration(0f, in input, _settings);
            Assert.AreEqual(0f, accel, 0.0001f);
        }

        [Test]
        public void Throttle_AtMaxSpeed_DoesNotAddForwardAcceleration()
        {
            var input = new VehicleControlInput(1f, 0f, 0f);
            float accel = MotorcycleDriveModel.ComputeLongitudinalAcceleration(_settings.MaxSpeed, in input, _settings);
            Assert.AreEqual(0f, accel, 0.0001f);
        }

        [Test]
        public void RepeatedThrottleNearMaxSpeed_DoesNotExceedSoftLimitAccelGrowth()
        {
            var input = new VehicleControlInput(1f, 0f, 0f);
            float speed = _settings.MaxSpeed - 0.001f;
            float a1 = MotorcycleDriveModel.ComputeLongitudinalAcceleration(speed, in input, _settings);
            float a2 = MotorcycleDriveModel.ComputeLongitudinalAcceleration(speed, in input, _settings);
            Assert.AreEqual(a1, a2, 0.0001f);
            Assert.LessOrEqual(a1, _settings.Acceleration);
        }

        [Test]
        public void Steer_AtStandstill_HasZeroEffectivenessAndYaw()
        {
            var input = new VehicleControlInput(0f, 0f, 1f);
            Assert.AreEqual(0f, MotorcycleDriveModel.ComputeSteerEffectiveness(0f, _settings));
            Assert.AreEqual(0f, MotorcycleDriveModel.ComputeYawAcceleration(0f, in input, _settings));
        }

        [Test]
        public void Steer_BelowMinSpeed_ProducesNoYaw()
        {
            var input = new VehicleControlInput(0f, 0f, -1f);
            float yaw = MotorcycleDriveModel.ComputeYawAcceleration(_settings.MinSpeedForSteer * 0.5f, in input, _settings);
            Assert.AreEqual(0f, yaw);
        }

        [Test]
        public void Steer_AtSpeed_ProducesYawInSteerDirection()
        {
            var input = new VehicleControlInput(0f, 0f, 1f);
            float yaw = MotorcycleDriveModel.ComputeYawAcceleration(5f, in input, _settings);
            Assert.Greater(yaw, 0f);
        }

        [Test]
        public void Steer_WhenReversing_InvertsYawDirection()
        {
            var input = new VehicleControlInput(0f, 0f, 1f);
            float forwardYaw = MotorcycleDriveModel.ComputeYawAcceleration(5f, in input, _settings);
            float reverseYaw = MotorcycleDriveModel.ComputeYawAcceleration(-5f, in input, _settings);
            Assert.Greater(forwardYaw, 0f);
            Assert.Less(reverseYaw, 0f);
            Assert.AreEqual(forwardYaw, -reverseYaw, 0.0001f);
        }

        [Test]
        public void DriveForce_AlignsWithForwardAxis()
        {
            var input = new VehicleControlInput(1f, 0f, 0f);
            Vector3 forward = Vector3.forward;
            Vector3 force = MotorcycleDriveModel.ComputeDriveForce(forward, 0f, 100f, in input, _settings);
            Assert.Greater(force.z, 0f);
            Assert.AreEqual(0f, force.x, 0.0001f);
            Assert.AreEqual(0f, force.y, 0.0001f);
        }

        [Test]
        public void ClampYawRate_LimitsExtremeValues()
        {
            Assert.AreEqual(_settings.MaxSteerYawRate, MotorcycleDriveModel.ClampYawRate(99f, _settings));
            Assert.AreEqual(-_settings.MaxSteerYawRate, MotorcycleDriveModel.ClampYawRate(-99f, _settings));
        }

        [Test]
        public void StabilityTorque_WhenUpright_IsZero()
        {
            Vector3 torque = MotorcycleDriveModel.ComputeStabilityTorque(Vector3.up, _settings);
            Assert.AreEqual(Vector3.zero, torque);
        }

        [Test]
        public void StabilityTorque_WhenLeaning_PullsTowardUpright()
        {
            Vector3 leaned = (Vector3.up + Vector3.right).normalized;
            Vector3 torque = MotorcycleDriveModel.ComputeStabilityTorque(leaned, _settings);
            Assert.Greater(torque.magnitude, 0f);
            // Cross(leaned, up) points roughly around +Z for lean to +X; assist should be non-zero on that axis.
            Assert.That(Mathf.Abs(torque.z), Is.GreaterThan(0.0001f));
        }

        [Test]
        public void InvalidSettings_ThrowOnValidate()
        {
            var bad = new MotorcycleDriveSettings { MaxSpeed = 0f };
            Assert.Throws<InvalidOperationException>(() => bad.ValidateOrThrow());
        }
    }
}
