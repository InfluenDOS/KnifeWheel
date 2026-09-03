namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Scriptable input source for automated tests and debugging.
    /// </summary>
    public sealed class ScriptedVehicleInputSource : IVehicleInputSource
    {
        public VehicleControlInput Current { get; set; } = VehicleControlInput.None;

        public VehicleControlInput ReadInput() => Current;
    }
}
