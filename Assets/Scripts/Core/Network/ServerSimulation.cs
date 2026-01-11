using System.Collections.Generic;
using UnityEngine;
using Game.Player;
using Unity.Netcode;
using NetworkConfig = ScriptableObjects.Network.NetworkConfig;
using Utils;

namespace Core.Network
{
    [DefaultExecutionOrder(-100)]
    public class ServerSimulation : SceneSingleton<ServerSimulation>
    {
        [SerializeField] private NetworkConfig networkConfig; 
        public NetworkConfig NetworkConfig => networkConfig; 

        private float _tickRate => networkConfig.TICK_RATE_IN_SECONDS; 
        private float _fixedTickAccum; 
        private uint _serverTick; 
        
        private readonly Dictionary<ulong, PlayerNetworkMove> _players = new Dictionary<ulong,PlayerNetworkMove>();
        
        private readonly Dictionary<ulong, InputPayload> _latestInputs = new Dictionary<ulong, InputPayload>();
        
        private readonly Dictionary<uint, List<ulong>> _playersTouchedByTick = new Dictionary<uint,List<ulong>>(); 
        
        private readonly List<uint> _appliedTicks = new List<uint>(); 
        

        private void Update()
        {
            if (!(NetworkManager.Singleton?.IsServer ?? false)) return;
            
            if (_appliedTicks.Count == 0) return;
            
            foreach (uint tick in _appliedTicks)
            {
                if (!_playersTouchedByTick.TryGetValue(tick, out var playerList)) continue;

                foreach (ulong ownerId in playerList)
                {
                    if (!_players.TryGetValue(ownerId, out var playerMovement)) continue;

                    StatePayload payload = playerMovement.ServerCollectState(tick);
                    playerMovement.UpdateStateClientRpc(payload);

                }

                _playersTouchedByTick.Remove(tick);
            }

            _appliedTicks.Clear();
        }

        private void FixedUpdate()
        {
            if (!(NetworkManager.Singleton?.IsServer ?? false)) return;
            
            _fixedTickAccum += Time.fixedDeltaTime;
            
            float tickInterval = _tickRate;
            
            while (_fixedTickAccum >= tickInterval)
            {
                _fixedTickAccum -= tickInterval;
                ApplyServerTick(_serverTick);
                _appliedTicks.Add(_serverTick);
                _serverTick++;
            }
        }

        // ========== Public APIs ========== 

        public void RegisterPlayer(ulong ownerClientId, PlayerNetworkMove player)
        {
            _players[ownerClientId] = player;
        }

        public void UnregisterPlayer(ulong ownerClientId)
        {
            if (_players.ContainsKey(ownerClientId)) 
                _players.Remove(ownerClientId);

            if (_latestInputs.ContainsKey(ownerClientId)) 
                _latestInputs.Remove(ownerClientId);
        }

        public void ReceiveClientInput(ulong ownerClientId, InputPayload payload)
        {
            _latestInputs[ownerClientId] = payload;
        }

        // ========== Internal ==========

        private void ApplyServerTick(uint tick)
        {
            _playersTouchedByTick[tick] = new List<ulong>();
            
            foreach (var kv in _players)
            {
                ulong ownerId = kv.Key;
                PlayerNetworkMove playerMovement = kv.Value;
                InputPayload input = default;

                if (_latestInputs.TryGetValue(ownerId, out var latest))
                {
                    input = latest;
                }
                else
                {
                    input.Tick = tick;
                    input.InputDirection = Vector2.zero;
                }

                playerMovement.ServerApplyInput(tick, input.InputDirection);
                _playersTouchedByTick[tick].Add(ownerId);
            }
        }
    }
}
