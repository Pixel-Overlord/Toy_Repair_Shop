using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. A future stage can override UpdateProgress for a distinct circular-buffing gesture.
    /// </summary>
    public sealed class PolishBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Polish;
    }
}
