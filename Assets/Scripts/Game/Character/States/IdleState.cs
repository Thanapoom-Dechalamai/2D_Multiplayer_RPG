using UnityEngine;

namespace Game.Character.States
{
    public class IdleState : CharacterState
    {
        public IdleState(ICharacterContext character, CharacterStateMachine stateMachine) : base(character, stateMachine){ }

        public override void Enter()
        {
            Character.ApplyMovement(Vector2.zero);
        }

        public override void Update()
        {
            Vector2 intent = Character.MoveIntent.GetMoveIntent();
            
            if (intent.sqrMagnitude > 0.01f)
            {
                StateMachine.ChangeState<MoveState>();
            }

        }
    }
}

