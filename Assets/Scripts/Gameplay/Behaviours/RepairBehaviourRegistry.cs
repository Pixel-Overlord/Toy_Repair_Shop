using System.Collections.Generic;
using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Default IRepairBehaviourRegistry: a fixed table of one
    /// IRepairBehaviour instance per RepairType. Adding a new repair type
    /// means adding one line here and one new behaviour class - nothing
    /// else in the repair system changes.
    /// </summary>
    public sealed class RepairBehaviourRegistry : IRepairBehaviourRegistry
    {
        private readonly Dictionary<RepairType, IRepairBehaviour> _behaviours;

        public RepairBehaviourRegistry()
        {
            _behaviours = new Dictionary<RepairType, IRepairBehaviour>
            {
                { RepairType.Wash, new WashBehaviour() },
                { RepairType.Dry, new DryBehaviour() },
                { RepairType.Sew, new SewBehaviour() },
                { RepairType.Glue, new GlueBehaviour() },
                { RepairType.Paint, new PaintBehaviour() },
                { RepairType.Polish, new PolishBehaviour() },
                { RepairType.ReplacePart, new ReplacePartBehaviour() },
                { RepairType.Clean, new CleanBehaviour() },
                { RepairType.Tighten, new TightenBehaviour() },
            };
        }

        public IRepairBehaviour GetBehaviour(RepairType repairType)
        {
            if (_behaviours.TryGetValue(repairType, out IRepairBehaviour behaviour))
            {
                return behaviour;
            }

            throw new KeyNotFoundException($"RepairBehaviourRegistry: no IRepairBehaviour registered for RepairType.{repairType}.");
        }
    }
}
