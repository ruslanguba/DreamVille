using System;
using System.Collections.Generic;

using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>
    /// Drives a fixed set of <see cref="IEnemyState"/> instances, one per <see cref="EnemyStateId"/>.
    /// States are stored in an array indexed by id, so a transition never allocates.
    /// </summary>
    public sealed class EnemyStateMachine
    {
        private static readonly int StateSlotCount = Enum.GetValues(typeof(EnemyStateId)).Length;

        private readonly IEnemyState[] _states;

        private IEnemyState _current;

        /// <summary>Arguments: previous, next. Raised after Exit and before Enter.</summary>
        public event Action<EnemyStateId, EnemyStateId> StateChanged;

        public EnemyStateMachine(IEnumerable<IEnemyState> states) : this(_ => states) { }

        /// <summary>
        /// States hold their machine and the machine holds its states, so the factory receives the
        /// machine under construction; both sides can then keep their references readonly.
        /// </summary>
        public EnemyStateMachine(Func<EnemyStateMachine, IEnumerable<IEnemyState>> createStates)
        {
            if (createStates == null)
                throw new ArgumentNullException(nameof(createStates));

            _states = new IEnemyState[StateSlotCount];

            foreach (var state in createStates(this))
            {
                int index = (int)state.Id;
                if (_states[index] != null)
                    throw new ArgumentException($"Duplicate state registered for {state.Id}.", nameof(createStates));

                _states[index] = state;
            }
        }

        public IEnemyState CurrentState => _current;

        public bool IsRunning => _current != null;

        /// <summary>
        /// Enters the initial state. Raises <see cref="StateChanged"/> with previous == next so
        /// listeners can sync without a "no state" sentinel in <see cref="EnemyStateId"/>.
        /// </summary>
        public void Start(EnemyStateId id)
        {
            if (_current != null)
                throw new InvalidOperationException($"Already running in {_current.Id}; Start may be called once.");

            _current = GetState(id);
            StateChanged?.Invoke(id, id);
            _current.Enter();
        }

        /// <summary>Returns false when a guard refused the transition.</summary>
        public bool ChangeState(EnemyStateId id)
        {
            var next = GetState(id);

            if (_current != null && _current.Id == id)
                return false;

            if (_current != null && _current.Id == EnemyStateId.Dead)
                return false;

            var previous = _current;
            previous?.Exit();
            _current = next;
            StateChanged?.Invoke(previous != null ? previous.Id : id, id);
            _current.Enter();
            return true;
        }

        public void Tick(float deltaTime) => _current?.Tick(deltaTime);

        public void FixedTick(float fixedDeltaTime) => _current?.FixedTick(fixedDeltaTime);

        private IEnemyState GetState(EnemyStateId id)
        {
            int index = (int)id;
            if (index < 0 || index >= _states.Length || _states[index] == null)
                throw new ArgumentOutOfRangeException(nameof(id), id, "No state registered for this id.");

            return _states[index];
        }
    }
}
