using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. Added so the Broken Robot's Tighten step (sample content) has a registered behaviour.
    /// </summary>
    public sealed class TightenBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Tighten;
    }
}
