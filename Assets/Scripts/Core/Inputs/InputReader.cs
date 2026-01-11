using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Core.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Core/InputReader")]
    public class InputReader : ScriptableObject, GameInput.IGameplayActions
    {
        // Events for other classes to listen to
        public event UnityAction<Vector2> MoveEvent;

        private GameInput _gameInput;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.Gameplay.SetCallbacks(this);
            }
            _gameInput.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _gameInput?.Gameplay.Disable();
        }

        // Interface Implementation
        public void OnMovement(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
    }
}