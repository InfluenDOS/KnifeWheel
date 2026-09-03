using System;
using UnityEngine;

namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Tunable motorcycle arcade-drive parameters. Kept as a plain serializable type
    /// so EditMode tests can construct settings without a MonoBehaviour.
    /// </summary>
    [Serializable]
    public sealed class MotorcycleDriveSettings
    {
        [Min(0f)] public float Acceleration = 25f;
        [Min(0.01f)] public float MaxSpeed = 20f;
        [Min(0f)] public float BrakeDeceleration = 35f;
        [Min(0f)] public float CoastDeceleration = 4f;
        [Min(0f)] public float SteerYawAcceleration = 8f;
        [Min(0f)] public float MaxSteerYawRate = 2.2f;
        [Min(0f)] public float MinSpeedForSteer = 0.35f;
        [Min(0.01f)] public float FullSteerSpeed = 4f;
        [Min(0f)] public float StabilityAssist = 40f;
        [Min(0f)] public float AngularDampingAssist = 2f;

        public void ValidateOrThrow()
        {
            if (Acceleration < 0f)
                throw new InvalidOperationException("Acceleration must be >= 0.");
            if (MaxSpeed <= 0f)
                throw new InvalidOperationException("MaxSpeed must be > 0.");
            if (BrakeDeceleration < 0f)
                throw new InvalidOperationException("BrakeDeceleration must be >= 0.");
            if (FullSteerSpeed <= 0f)
                throw new InvalidOperationException("FullSteerSpeed must be > 0.");
            if (MinSpeedForSteer < 0f)
                throw new InvalidOperationException("MinSpeedForSteer must be >= 0.");
            if (MinSpeedForSteer > FullSteerSpeed)
                throw new InvalidOperationException("MinSpeedForSteer must be <= FullSteerSpeed.");
        }
    }
}
