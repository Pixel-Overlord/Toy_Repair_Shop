using System.Collections.Generic;
using ToyRepairShop.Data.ScriptableObjects;
using ToyRepairShop.Gameplay.Models;
using UnityEngine;

namespace ToyRepairShop.Gameplay.Controllers
{
    /// <summary>
    /// Ordered queue of runtime RepairStep instances for a single toy's
    /// repair session. Owns nothing about tools, behaviours, or state -
    /// only "what step are we on, what's next, how many remain".
    /// </summary>
    public sealed class RepairStepQueue
    {
        private readonly List<RepairStep> _steps = new List<RepairStep>();
        private int _index;

        public RepairStep Current => _index < _steps.Count ? _steps[_index] : null;
        public RepairStep Next => _index + 1 < _steps.Count ? _steps[_index + 1] : null;
        public int RemainingCount => Mathf.Max(0, _steps.Count - _index);
        public bool IsEmpty => _steps.Count == 0;

        /// <summary>Replaces the queue's contents with fresh runtime steps built from the given step data, in order.</summary>
        public void Load(IEnumerable<RepairStepData> stepDataList)
        {
            _steps.Clear();
            _index = 0;

            foreach (RepairStepData data in stepDataList)
            {
                _steps.Add(new RepairStep(data));
            }
        }

        /// <summary>Advances past the current step. Returns true if a next step is now current, false if the queue is exhausted.</summary>
        public bool Advance()
        {
            _index++;
            return _index < _steps.Count;
        }
    }
}
