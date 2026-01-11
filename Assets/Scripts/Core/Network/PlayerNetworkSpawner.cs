using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Core.Network
{
    public class PlayerNetworkSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<Transform> spawnPoints;
        private int _currentSpawnIndex = 0;

        private void Awake()
        {
            // ลงทะเบียน Service เพื่อให้คลาสอื่นเรียกใช้ได้ (ถ้าต้องการ)
            // Core.Managers.ServiceLocator.Register<CustomNetworkManager>(this);
        }

        private void Start()
        {
            if (NetworkManager.Singleton == null) return;

            // เปลี่ยนมาใช้ OnServerStarted เพื่อความชัวร์ในฝั่ง Host
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            }
        }

        private void HandleServerStarted()
        {
            // ถ้าเราเป็น Host (Server + Client)
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Host Started - Local Player will be spawned via OnClientConnected");
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            // Logic นี้จะรันเฉพาะบน Server เท่านั้น
            if (!NetworkManager.Singleton.IsServer) return;

            Debug.Log($"Client Connected: {clientId}. Spawning player...");
            SpawnPlayer(clientId);
        }

        private void SpawnPlayer(ulong clientId)
        {
            if (playerPrefab == null) return;

            // ค้นหา SpawnPoints อัตโนมัติหากใน Inspector ลืมลากใส่
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                var points = GameObject.FindGameObjectsWithTag("Respawn"); // หรือใช้ชื่อ Object
                foreach(var p in points) spawnPoints.Add(p.transform);
            }

            Transform spawnPoint = spawnPoints[_currentSpawnIndex % spawnPoints.Count];
            _currentSpawnIndex++;

            GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            var netObj = playerInstance.GetComponent<NetworkObject>();
            
            // สำคัญ: ต้องใช้ SpawnAsPlayerObject เพื่อให้ NGO จัดการ Ownership
            netObj.SpawnAsPlayerObject(clientId, true);
        }
    }
}