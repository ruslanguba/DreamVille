using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Configs.Enemies;
using DreamVille.Core.Players;
using DreamVille.Gameplay.Enemies.Combat;
using DreamVille.Gameplay.Enemies.Presentation;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Gameplay.Enemies.Steering;
using DreamVille.Gameplay.Player;

using Random = UnityEngine.Random;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// The one enemy component. It owns the per-instance seams (steering, animation, audio, attack),
    /// forwards the Unity lifecycle to an <see cref="EnemyStateMachine"/> and exposes the world to
    /// its states through <see cref="IEnemyContext"/>. Behaviour and tuning come from the
    /// <see cref="EnemyConfig"/> archetype; only the placement below stays on the instance.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(EnemyHealth), typeof(EnemyRotationHandler))]
    public sealed class Enemy : MonoBehaviour, IEnemyContext
    {
        /// <summary>Arguments: previous state, next state.</summary>
        public event Action<EnemyStateId, EnemyStateId> StateChanged;

        /// <summary>Raised from OnDestroy so hideouts can drop their reference to this enemy.</summary>
        public event Action<Enemy> Destroyed;

        [SerializeField] private EnemyConfig _config;

        // Where this enemy walks and throws from; every number comes from the archetype in _config.
        [Header("Placement")]
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private Transform _attackSpawn;

        private IPlayerMarker _player;
        private EnemyProjectile.Pool _projectiles;

        private Rigidbody _body;
        private EnemyHealth _health;
        private EnemyRotationHandler _rotation;
        private ActorAudio _actorAudio;
        private AnimationEventListener _animationEvents;

        private EnemyStateMachine _machine;
        private ISteering _steering;
        private IEnemyAnimator _animator;
        private IEnemyAudio _audio;
        private IEnemyAttack _attack;

        private Transform[] _route = Array.Empty<Transform>();
        private Transform _fleeTarget;
        private IEnemyHideout _fleeHideout;
        private CancellationTokenSource _immunityCts;

        /// <summary>Valid from Awake onwards.</summary>
        public EnemyStateId CurrentState => _machine != null && _machine.IsRunning
            ? _machine.CurrentState.Id
            : EnemyStateId.Idle;

        [Inject]
        public void Construct(IPlayerMarker player, EnemyProjectile.Pool projectiles)
        {
            _player = player;
            _projectiles = projectiles;
        }

        /// <summary>
        /// Runs to <paramref name="destination"/>. An <see cref="IEnemyHideout"/> on the destination
        /// swallows the enemy on arrival. Safe to call before Awake: the machine then starts fleeing.
        /// </summary>
        public void FleeTo(Transform destination)
        {
            if (destination == null)
            {
                Debug.LogError($"[{nameof(Enemy)}] FleeTo needs a destination; the enemy would run nowhere forever.", this);
                return;
            }

            _fleeTarget = destination;
            _fleeHideout = destination == null ? null : destination.GetComponent<IEnemyHideout>();

            if (_machine != null && _machine.IsRunning)
                _machine.ChangeState(EnemyStateId.FleeToHideout);
        }

        // A destroyed component still compares non-null through an interface reference.
        private bool HasPlayer => _player is Component component ? component != null : _player != null;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _health = GetComponent<EnemyHealth>();
            _rotation = GetComponent<EnemyRotationHandler>();
            _actorAudio = GetComponent<ActorAudio>();
            _animationEvents = GetComponentInChildren<AnimationEventListener>();

            if (_config == null)
            {
                Debug.LogError($"[{nameof(Enemy)}] No {nameof(EnemyConfig)} assigned; this enemy stays inert.", this);
                enabled = false;
                return;
            }

            _rotation.Configure(_config.RotationSpeed);
            _route = PatrolRoute.WithoutEmptySlots(_patrolPoints);
            _steering = new RigidbodySteering(_body);
            _animator = new EnemyAnimatorDriver(GetComponentInChildren<Animator>());
            _audio = _actorAudio != null ? new ActorAudioAdapter(_actorAudio) : (IEnemyAudio)new NullEnemyAudio();
            _attack = CreateAttack();

            _machine = new EnemyStateMachine(machine => new IEnemyState[]
            {
                new EnemyIdleState(this, machine),
                new EnemyPatrolState(this, machine),
                new EnemyChaseState(this, machine),
                new EnemyAttackState(this, machine),
                new EnemyFleeToHideoutState(this, machine),
                new EnemyHiddenState(this, machine),
                new EnemyDeadState(this, machine)
            });

            _machine.StateChanged += OnStateChanged;
            _health.Killed += OnEnemyKilled;

            // A hideout releases its enemy on the same frame it activates it, so the flee target can
            // already be set by the time this first Awake runs.
            _machine.Start(_fleeTarget != null ? EnemyStateId.FleeToHideout : _config.InitialState);
        }

        private void OnEnable()
        {
            if (_animationEvents != null && _actorAudio != null)
                _animationEvents.JumpFrameReached += _actorAudio.PlayJump;

            if (_config == null)
                return;

            _health.SetVulnerable(_config.SpawnsVulnerable);

            if (_config.SpawnsVulnerable)
                return;

            _immunityCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            BecomeVulnerableAsync(_config.HitImmunityDuration, _immunityCts.Token).Forget();
        }

        private void OnDisable()
        {
            if (_animationEvents != null && _actorAudio != null)
                _animationEvents.JumpFrameReached -= _actorAudio.PlayJump;

            CancelImmunity();
        }

        private void Start()
        {
            if (_config != null)
                _animator.PlayLocomotion(Random.Range(0f, _config.MaxLocomotionStartOffset));
        }

        // Null only when Awake refused to build the machine; the component is disabled then.
        private void Update() => _machine?.Tick(Time.deltaTime);

        private void FixedUpdate() => _machine?.FixedTick(Time.fixedDeltaTime);

        private void OnDestroy()
        {
            if (_machine != null)
                _machine.StateChanged -= OnStateChanged;

            if (_health != null)
                _health.Killed -= OnEnemyKilled;

            Destroyed?.Invoke(this);
        }

        private IEnemyAttack CreateAttack()
        {
            if (_config.Attack != EnemyAttackKind.ThrowProjectile)
                return new NullEnemyAttack();

            if (_attackSpawn == null || _projectiles == null)
            {
                Debug.LogError($"[{nameof(Enemy)}] A throwing enemy needs an attack spawn point and a projectile pool.", this);
                return new NullEnemyAttack();
            }

            return new ProjectileAttack(_projectiles, _attackSpawn, _config);
        }

        private void OnStateChanged(EnemyStateId previous, EnemyStateId next) => StateChanged?.Invoke(previous, next);

        private void OnEnemyKilled(EnemyHealth health, int ricochetCount) => _machine.ChangeState(EnemyStateId.Dead);

        private async UniTaskVoid BecomeVulnerableAsync(float delay, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                _health.SetVulnerable(true);
            }
            catch (OperationCanceledException)
            {
                // Disabled or destroyed before the immunity window ended.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void CancelImmunity()
        {
            if (_immunityCts == null)
                return;

            _immunityCts.Cancel();
            _immunityCts.Dispose();
            _immunityCts = null;
        }

        // The view the states get. Explicit implementations: none of it belongs to the component's API.

        EnemyConfig IEnemyContext.Config => _config;

        Vector3 IEnemyContext.Position => transform.position;

        bool IEnemyContext.HasPlayer => HasPlayer;

        Vector3 IEnemyContext.PlayerPosition => _player.Transform.position;

        ISteering IEnemyContext.Steering => _steering;

        IEnemyAnimator IEnemyContext.Animator => _animator;

        IEnemyAudio IEnemyContext.Audio => _audio;

        IEnemyAttack IEnemyContext.Attack => _attack;

        int IEnemyContext.PatrolPointCount => _route.Length;

        bool IEnemyContext.HasFleeTarget => _fleeTarget != null;

        Vector3 IEnemyContext.FleeTargetPosition => _fleeTarget.position;

        bool IEnemyContext.FleeTargetIsHideout => _fleeHideout != null;

        // Empty slots were dropped in Awake, so every entry of the route is a live transform.
        Vector3 IEnemyContext.GetPatrolPoint(int index) => _route[index].position;

        void IEnemyContext.EnterHideout() => _fleeHideout?.Receive(this);

        void IEnemyContext.FaceTowards(Vector3 worldPoint) => _rotation.FaceTowards(worldPoint);

        void IEnemyContext.SnapFacing(Vector3 worldPoint) => _rotation.SnapTo(worldPoint);

        void IEnemyContext.SetAutoFacing(bool isEnabled) => _rotation.enabled = isEnabled;

        void IEnemyContext.SetVulnerable(bool isVulnerable) => _health.SetVulnerable(isVulnerable);

        void IEnemyContext.ReleaseBody()
        {
            _body.freezeRotation = false;
            _body.isKinematic = false;
            _body.useGravity = true;
        }

        void IEnemyContext.Despawn(float delay) => Destroy(gameObject, delay);
    }
}
