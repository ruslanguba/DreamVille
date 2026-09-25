using System;
using System.Collections.Generic;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons.Pillow
{
    /// <summary>
    /// The F/R rules over a fixed set of slots, of which the first <see cref="Available"/> may be used.
    /// F: ignored while a pillow returns; recalls the flying pillow; else throws the first stowed available slot;
    /// else recalls the most recently thrown one (LIFO). R: freezes the flying pillow. "Flying" means thrown and
    /// neither frozen nor recalled — a thrown pillow that has come to rest is still the one F recalls.
    /// </summary>
    public sealed class PillowRack
    {
        private readonly PillowSlot[] _slots;
        private readonly PillowConfig _config;
        private readonly List<PillowSlot> _thrown = new List<PillowSlot>();

        private PillowSlot _flying;
        private PillowSlot _returning;

        /// <exception cref="ArgumentException">A body is missing or the config would stall the recall loop.</exception>
        public PillowRack(IReadOnlyList<IPillowBody> bodies, PillowConfig config)
        {
            if (bodies == null)
                throw new ArgumentNullException(nameof(bodies));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!config.IsValid(out string error))
                throw new ArgumentException(error, nameof(config));

            _config = config;
            _slots = new PillowSlot[bodies.Count];

            for (int i = 0; i < bodies.Count; i++)
            {
                // A destroyed or unassigned component is fake-null: the interface reference alone does not see it.
                if (bodies[i] == null || (bodies[i] is UnityEngine.Object unityObject && unityObject == null))
                    throw new ArgumentException($"Pillow body {i} is missing.", nameof(bodies));

                _slots[i] = new PillowSlot(bodies[i]);
            }
        }

        /// <summary>Slots on the prefab, used or not.</summary>
        public int Capacity => _slots.Length;

        /// <summary>How many slots, counted from the first, the player may use. Starts at 0.</summary>
        public int Available { get; private set; }

        /// <summary>Stowed pillows among the available slots.</summary>
        public int InHand
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Available; i++)
                {
                    if (_slots[i].State == PillowState.Stowed)
                        count++;
                }

                return count;
            }
        }

        /// <summary>True while a recalled pillow is on its way back.</summary>
        public bool IsReturning => _returning != null;

        /// <summary>Clamped to [<see cref="Available"/>, <see cref="Capacity"/>]: availability only grows at runtime.</summary>
        public void SetAvailable(int count)
        {
            Available = Mathf.Clamp(count, Available, Capacity);
        }

        /// <summary>One press of the throw key. The throw impulse points along <paramref name="launch"/>'s forward.</summary>
        public PillowCommand ThrowOrRecall(Pose launch, Vector3 returnPoint)
        {
            if (IsReturning)
                return PillowCommand.None;

            if (_flying != null)
                return BeginReturn(_flying, returnPoint);

            for (int i = 0; i < Available; i++)
            {
                PillowSlot slot = _slots[i];
                if (slot.TryThrow(launch, launch.forward * _config.ThrowImpulse))
                {
                    _thrown.Add(slot);
                    _flying = slot;
                    return PillowCommand.Thrown;
                }
            }

            // Every available pillow is out: take back the most recent one.
            if (_thrown.Count > 0)
                return BeginReturn(_thrown[_thrown.Count - 1], returnPoint);

            return PillowCommand.None;
        }

        /// <summary>Freezes the flying pillow into a platform. False when nothing is flying.</summary>
        public bool FreezeFlying()
        {
            if (_flying == null || !_flying.TryFreeze())
                return false;

            _flying = null;
            return true;
        }

        /// <summary>Moves the returning pillow by one frame of <see cref="PillowConfig.RecallSpeed"/>.</summary>
        /// <returns>True when no pillow is returning any more (it arrived now, or none was returning): the recall loop can stop.</returns>
        public bool StepReturn(Vector3 returnPoint, float deltaTime)
        {
            if (_returning == null)
                return true;

            if (!_returning.StepReturn(returnPoint, _config.RecallSpeed * deltaTime, _config.ArrivalThreshold))
                return false;

            _returning = null;
            return true;
        }

        /// <summary>Stows the returning pillow at once (cancellation path). False when none was returning.</summary>
        public bool CompleteReturn()
        {
            if (_returning == null)
                return false;

            _returning.CompleteReturn();
            _returning = null;
            return true;
        }

        private PillowCommand BeginReturn(PillowSlot slot, Vector3 returnPoint)
        {
            if (!slot.TryBeginReturn(returnPoint, _config.RecallSnapDistance))
                return PillowCommand.None;

            _thrown.Remove(slot);
            if (_flying == slot)
                _flying = null;

            _returning = slot;
            return PillowCommand.ReturnStarted;
        }
    }
}
