using UnityEngine;

namespace ScriptableObjects.Network
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "NetworkConfig")]
    public class NetworkConfig : ScriptableObject
    {
        public float TICK_RATE = 64f;
        public float TICK_RATE_IN_SECONDS => 1f / TICK_RATE;

        public int BUFFER_SIZE = 1024;
    }
}
