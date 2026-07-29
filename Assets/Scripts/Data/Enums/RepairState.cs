namespace ToyRepairShop.Data.Enums
{
    /// <summary>
    /// State of a single toy's repair session, owned by RepairStateMachine.
    /// </summary>
    public enum RepairState
    {
        Idle,
        ToolSelection,
        Repairing,
        StepCompleted,
        ToyCompleted
    }
}
