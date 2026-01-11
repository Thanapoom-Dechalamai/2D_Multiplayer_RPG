using Unity.Netcode;
using UnityEngine;

namespace Core.Network
{
    public struct InputPayload : INetworkSerializable
    {
        public uint Tick;
        public Vector2 InputDirection;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref InputDirection);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public uint Tick;
        public Vector2 Position;
        public Vector2 Velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Velocity);
        }
    }
}