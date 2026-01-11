using System;
using System.Collections.Generic;

namespace Game.Character.States
{
    public class CharacterStateMachine
    {
        public CharacterState CurrentState { get; private set; }

        private Dictionary<Type, CharacterState> _states = new Dictionary<Type, CharacterState>();

        public void RegisterState(CharacterState state) => _states[state.GetType()] = state;

        public void Initialize<T>() where T : CharacterState
        {
            CurrentState = _states[typeof(T)];
            CurrentState.Enter();
        }

        public void ChangeState<T>() where T : CharacterState
        {
            CurrentState.Exit();
            CurrentState = _states[typeof(T)];
            CurrentState.Enter();
        }

        public void Update() => CurrentState?.Update();
    }

}