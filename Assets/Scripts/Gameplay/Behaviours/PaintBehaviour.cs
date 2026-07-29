using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. A future stage can override UpdateProgress for a distinct painting gesture.
    /// </summary>
    public sealed class PaintBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Paint;
    }
}
