using UnityEngine;

namespace Game.Character
{
    public interface ICharacterContext
    {
        float MoveSpeed { get; }
        IMoveIntent MoveIntent { get; }

        void ApplyMovement(Vector2 velocity);
    }

    public interface IMoveIntent
    {
        Vector2 GetMoveIntent();
    }
}
