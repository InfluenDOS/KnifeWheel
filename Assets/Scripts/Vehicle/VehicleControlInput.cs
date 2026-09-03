namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Normalized player control sample for one simulation step.
    /// Throttle/Brake are 0..1; Steer is -1..1.
    /// </summary>
    public struct VehicleControlInput
    {
        public float Throttle;
        public float Brake;
        public float Steer;

        public VehicleControlInput(float throttle, float brake, float steer)
        {
            Throttle = throttle;
            Brake = brake;
            Steer = steer;
        }

        public static VehicleControlInput None => new VehicleControlInput(0f, 0f, 0f);
    }
}
