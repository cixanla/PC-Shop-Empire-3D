namespace PCShopEmpire3D.Core.Time
{
    /// <summary>
    /// Read-only simulation clock exposed to domain systems.
    /// </summary>
    public interface ISimulationClock
    {
        SimulationTimestamp Current { get; }

        bool IsPaused { get; }
    }
}
