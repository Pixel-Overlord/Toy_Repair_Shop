namespace ToyRepairShop.Data.Enums
{
    /// <summary>
    /// Runtime lifecycle state of a Toy instance. Added alongside the other
    /// enums so the Toy runtime model's "current state" field (Stage 3
    /// requirement) has a concrete, non-string-typed value.
    /// </summary>
    public enum ToyState
    {
        Locked,
        Available,
        InProgress,
        Repaired
    }
}
