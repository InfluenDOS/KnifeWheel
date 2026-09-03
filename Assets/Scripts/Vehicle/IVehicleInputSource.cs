namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Abstraction over whatever reads player controls so physics code stays testable.
    /// </summary>
    public interface IVehicleInputSource
    {
        VehicleControlInput ReadInput();
    }
}
