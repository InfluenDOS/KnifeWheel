using UnityEngine;

namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Legacy Input Manager mapping: Vertical = throttle/brake, Horizontal = steer.
    /// Positive Vertical is throttle; negative Vertical is brake (no dedicated reverse for S2).
    /// </summary>
    public sealed class LegacyVehicleInputSource : IVehicleInputSource
    {
        public const string HorizontalAxis = "Horizontal";
        public const string VerticalAxis = "Vertical";

        public VehicleControlInput ReadInput()
        {
            float vertical = Input.GetAxisRaw(VerticalAxis);
            float horizontal = Input.GetAxisRaw(HorizontalAxis);

            float throttle = Mathf.Clamp01(vertical);
            float brake = Mathf.Clamp01(-vertical);
            float steer = Mathf.Clamp(horizontal, -1f, 1f);

            return new VehicleControlInput(throttle, brake, steer);
        }
    }
}
