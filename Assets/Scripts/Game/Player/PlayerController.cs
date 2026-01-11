using Game.Character;
using Game.Character.States;
using Unity.Netcode;
using UnityEngine;
namespace Game.Player
{
    [SelectionBase]
    public class PlayerController : NetworkBehaviour, ICharacterContext
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _externalDecay = 15f;

        [Header("Network Interpolation (clients)")]
        [SerializeField] private float _positionLerpRate = 15f;
        public float MoveSpeed => _moveSpeed;

        public Rigidbody2D Rigidbody { get; private set; }
        public PlayerVisualPresenter Visuals { get; private set; }
        public IMoveIntent MoveIntent { get; private set; }
        public CharacterStateMachine StateMachine { get; private set; }

        private Vector2 _movementVelocity;
        private Vector2 _externalVelocity;

        private Vector2 _serverTargetPosition;
        private Vector2 _serverTargetVelocity;
        private bool _hasServerState;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Visuals = GetComponentInChildren<PlayerVisualPresenter>();
            MoveIntent = GetComponent<PlayerNetworkMove>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                StateMachine = new CharacterStateMachine();
                StateMachine.RegisterState(new IdleState(this, StateMachine));
                StateMachine.RegisterState(new MoveState(this, StateMachine));
                StateMachine.Initialize<IdleState>();
            }
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                ApplyAuthoritativePhysics(Time.fixedDeltaTime);
            }
            else if (IsOwner)
            {
                ApplyPredictedPhysics(Time.fixedDeltaTime);
            }
            else
            {
                ApplyInterpolatedPhysics(Time.fixedDeltaTime);
            }
        }

        public void ApplyMovement(Vector2 velocity)
        {
            _movementVelocity = velocity;
        }

        public void AddExternalVelocity(Vector2 force)
        {
            _externalVelocity += force;
        }

        public void UpdateStateMachine()
        {
            if (!IsServer) return;
            StateMachine?.Update();
        }

        public void ApplyServerState(Vector2 position, Vector2 velocity)
        {
            _serverTargetPosition = position;
            _serverTargetVelocity = velocity;
            _hasServerState = true;
        }

        public void ApplyLocalKinematicStep(float deltaTime)
        {
            _externalVelocity = Vector2.Lerp(_externalVelocity, Vector2.zero, _externalDecay * deltaTime);
            
            Vector2 velocity = _movementVelocity + _externalVelocity;
            Vector2 newPos = Rigidbody.position + velocity * deltaTime;
            
            Rigidbody.position = newPos;
            Rigidbody.linearVelocity = velocity;
        } 

        // ========== Internal ========== 
        private void ApplyAuthoritativePhysics(float deltaTime)
        {
            _externalVelocity = Vector2.Lerp(_externalVelocity, Vector2.zero, _externalDecay * deltaTime);
            Rigidbody.linearVelocity = _movementVelocity + _externalVelocity;
        }

        private void ApplyPredictedPhysics(float deltaTime)
        {
            _externalVelocity = Vector2.Lerp(_externalVelocity, Vector2.zero, _externalDecay * deltaTime);
            
            Vector2 velocity = _movementVelocity + _externalVelocity;
            
            Rigidbody.position += velocity * deltaTime;
            Rigidbody.linearVelocity = velocity;
        }

        private void ApplyInterpolatedPhysics(float deltaTime)
        {
            if (!_hasServerState) return;

            float duration = 1f - Mathf.Exp(-_positionLerpRate * deltaTime);

            Rigidbody.MovePosition(Vector2.Lerp(Rigidbody.position, _serverTargetPosition, duration));
            Rigidbody.linearVelocity = Vector2.Lerp(Rigidbody.linearVelocity, _serverTargetVelocity, duration);
        }
    }
}