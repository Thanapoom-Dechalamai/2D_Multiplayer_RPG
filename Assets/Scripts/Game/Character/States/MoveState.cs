using UnityEngine;


namespace Game.Character.States
{
    public class MoveState : CharacterState
    {
        public MoveState(ICharacterContext character, CharacterStateMachine stateMachine) : base(character, stateMachine) { }

        public override void Update()
        {
            Vector2 intent = Character.MoveIntent.GetMoveIntent();

            if (intent.sqrMagnitude <= 0.01f)
            {
                StateMachine.ChangeState<IdleState>();
            }

            Vector2 velocity = intent * Character.MoveSpeed;

            Character.ApplyMovement(velocity);
        }
    }

}
