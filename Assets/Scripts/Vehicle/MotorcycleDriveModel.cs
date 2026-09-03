using UnityEngine;

namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Pure arcade motorcycle motion math. Operates on scalars/vectors so EditMode
    /// tests can validate forces without spinning up a Rigidbody scene.
    /// All accelerations are intended for FixedUpdate / physics timestep use.
    /// </summary>
    public static class MotorcycleDriveModel
    {
        public const float SpeedEpsilon = 0.0001f;

        /// <summary>
        /// Signed speed along the vehicle forward axis (positive = forward).
        /// </summary>
        public static float GetForwardSpeed(Vector3 velocity, Vector3 forward)
        {
            return Vector3.Dot(velocity, forward);
        }

        /// <summary>
        /// Longitudinal acceleration along forward (+throttle / -brake / coast).
        /// Clamps so throttle cannot push past MaxSpeed; brake may reduce below it.
        /// </summary>
        public static float ComputeLongitudinalAcceleration(
            float forwardSpeed,
            in VehicleControlInput input,
            MotorcycleDriveSettings settings)
        {
            settings.ValidateOrThrow();

            float throttle = Mathf.Clamp01(input.Throttle);
            float brake = Mathf.Clamp01(input.Brake);

            float accel = 0f;

            if (throttle > 0f && forwardSpeed < settings.MaxSpeed)
            {
                float headroom = settings.MaxSpeed - Mathf.Max(forwardSpeed, 0f);
                float throttleAccel = throttle * settings.Acceleration;
                // Soft-limit near max speed so high-frequency throttle cannot explode speed.
                float limitFactor = Mathf.Clamp01(headroom / Mathf.Max(settings.MaxSpeed * 0.05f, 0.01f));
                accel += throttleAccel * limitFactor;
            }

            if (brake > 0f)
            {
                // Brake always opposes current longitudinal motion; at rest it does nothing.
                if (Mathf.Abs(forwardSpeed) > SpeedEpsilon)
                {
                    float brakeAccel = brake * settings.BrakeDeceleration;
                    accel += -Mathf.Sign(forwardSpeed) * brakeAccel;
                }
            }
            else if (throttle <= 0f && Mathf.Abs(forwardSpeed) > SpeedEpsilon)
            {
                accel += -Mathf.Sign(forwardSpeed) * settings.CoastDeceleration;
            }

            return accel;
        }

        /// <summary>
        /// World-space force for Rigidbody.AddForce (mass-scaled by caller or here).
        /// Returns acceleration * mass so FixedUpdate can AddForce directly.
        /// </summary>
        public static Vector3 ComputeDriveForce(
            Vector3 forward,
            float forwardSpeed,
            float mass,
            in VehicleControlInput input,
            MotorcycleDriveSettings settings)
        {
            float accel = ComputeLongitudinalAcceleration(forwardSpeed, in input, settings);
            return forward.normalized * (accel * Mathf.Max(mass, 0.0001f));
        }

        /// <summary>
        /// Steer effectiveness 0..1 based on speed. Zero near standstill to avoid
        /// sideways spinning in place; ramps to full by FullSteerSpeed.
        /// </summary>
        public static float ComputeSteerEffectiveness(float forwardSpeed, MotorcycleDriveSettings settings)
        {
            settings.ValidateOrThrow();
            float absSpeed = Mathf.Abs(forwardSpeed);
            if (absSpeed < settings.MinSpeedForSteer)
                return 0f;

            float span = settings.FullSteerSpeed - settings.MinSpeedForSteer;
            if (span <= SpeedEpsilon)
                return 1f;

            return Mathf.Clamp01((absSpeed - settings.MinSpeedForSteer) / span);
        }

        /// <summary>
        /// Desired yaw angular acceleration (rad/s^2) around world/vehicle up.
        /// Reverse motion flips steer so low-speed reverse remains predictable.
        /// </summary>
        public static float ComputeYawAcceleration(
            float forwardSpeed,
            in VehicleControlInput input,
            MotorcycleDriveSettings settings)
        {
            settings.ValidateOrThrow();

            float steer = Mathf.Clamp(input.Steer, -1f, 1f);
            float effectiveness = ComputeSteerEffectiveness(forwardSpeed, settings);
            if (effectiveness <= 0f || Mathf.Abs(steer) <= SpeedEpsilon)
                return 0f;

            float direction = forwardSpeed < 0f ? -1f : 1f;
            return steer * direction * settings.SteerYawAcceleration * effectiveness;
        }

        /// <summary>
        /// Clamps a proposed yaw rate so steering cannot unbounded-spin the body.
        /// </summary>
        public static float ClampYawRate(float yawRate, MotorcycleDriveSettings settings)
        {
            settings.ValidateOrThrow();
            return Mathf.Clamp(yawRate, -settings.MaxSteerYawRate, settings.MaxSteerYawRate);
        }

        /// <summary>
        /// Upright assist torque axis magnitude: pulls local up toward world up.
        /// </summary>
        public static Vector3 ComputeStabilityTorque(Vector3 vehicleUp, MotorcycleDriveSettings settings)
        {
            settings.ValidateOrThrow();
            Vector3 axis = Vector3.Cross(vehicleUp, Vector3.up);
            float sinAngle = axis.magnitude;
            if (sinAngle <= SpeedEpsilon)
                return Vector3.zero;

            axis /= sinAngle;
            // Use cross magnitude (~sin) so small lean gets small correction.
            return axis * (sinAngle * settings.StabilityAssist);
        }
    }
}
