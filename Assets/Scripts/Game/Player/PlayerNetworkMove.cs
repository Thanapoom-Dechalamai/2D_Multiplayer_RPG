using UnityEngine;
using Unity.Netcode;
using Core.Inputs;
using Core.Network;
using Game.Character;
namespace Game.Player
{
    public class PlayerNetworkMove : NetworkBehaviour, IMoveIntent
    {
        [SerializeField] private InputReader _inputReader;
        
        private readonly InputPayload[] _inputBuffer = new InputPayload[BUFFER_SIZE];
        private readonly StatePayload[] _stateBuffer = new StatePayload[BUFFER_SIZE];
        
        private PlayerController _player;
        private Vector2 _moveIntent;
        
        private static int BUFFER_SIZE = 1024;
        private uint _localTick;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            BUFFER_SIZE = ServerSimulation.Instance.NetworkConfig.BUFFER_SIZE;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _inputReader.MoveEvent += OnMove;
            }

            if (IsServer)
            {
                ServerSimulation.Instance?.RegisterPlayer(OwnerClientId, this);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _inputReader.MoveEvent -= OnMove;
            }

            if (IsServer)
            {
                ServerSimulation.Instance?.UnregisterPlayer(OwnerClientId);
            }
        } 

        // ========== Public APIs ========== 

        public void ServerApplyInput(uint serverTick, Vector2 inputDirection)
        {
            _moveIntent = inputDirection;
            _player.UpdateStateMachine();
        }

        public StatePayload ServerCollectState(uint serverTick)
        {
            return new StatePayload
            {
                Tick = serverTick,
                Position = _player.Rigidbody.position,
                Velocity = _player.Rigidbody.linearVelocity
            };
        }

        [ClientRpc]
        public void UpdateStateClientRpc(StatePayload serverState)
        {
            if (!IsOwner)
            {
                _player.ApplyServerState(serverState.Position, serverState.Velocity);
                return;
            }
            
            Reconcile(serverState);
        }

        public Vector2 GetMoveIntent()
        {
            return _moveIntent;
        } 

        // ========== Internal ========== 

        private void OnMove(Vector2 dir)
        {
            InputPayload input = new()
            {
                Tick = _localTick,
                InputDirection = dir
            };
            
            _inputBuffer[_localTick % BUFFER_SIZE] = input;
            
            _moveIntent = dir;
            
            //_player.UpdateStateMachine();
            
            SendInputServerRpc(input);
            
            _localTick++;
        }

        private void Reconcile(StatePayload serverState)
        {
            uint index = serverState.Tick % (uint)BUFFER_SIZE;
            
            StatePayload predicted = _stateBuffer[index];
            
            if (Vector2.Distance(predicted.Position, serverState.Position) < 0.05f) return;
            
            _player.Rigidbody.position = serverState.Position;

            _player.Rigidbody.linearVelocity = serverState.Velocity;

            uint tick = serverState.Tick + 1;
            
            while (tick < _localTick)
            {
                InputPayload input = _inputBuffer[tick % BUFFER_SIZE];
                
                _moveIntent = input.InputDirection;
                
                //_player.UpdateStateMachine();
                
                _player.ApplyMovementPhysics(Time.fixedDeltaTime);

                _stateBuffer[tick % BUFFER_SIZE] = new StatePayload
                {
                    Tick = tick,
                    Position = _player.Rigidbody.position,
                    Velocity = _player.Rigidbody.linearVelocity
                };

                tick++;
            }
        }

        [ServerRpc]
        private void SendInputServerRpc(InputPayload payload)
        {
            ServerSimulation.Instance?.ReceiveClientInput(OwnerClientId, payload);
        }
    }
}
