using Game.Player;

namespace Game.Character.States
{
    public abstract class CharacterState
    {
        protected ICharacterContext Character;
        protected CharacterStateMachine StateMachine;

        public CharacterState(ICharacterContext character, CharacterStateMachine stateMachine)
        {
            Character = character;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

}
