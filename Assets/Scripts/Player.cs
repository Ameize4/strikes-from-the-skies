using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class Player : MonoBehaviour
    {
        public CharacterController Character;
        public InputActionReference move;
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            HandleCharacterInput();
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs inputs = new PlayerCharacterInputs();

            var v = move.action.ReadValue<Vector2>();

            inputs.MoveAxisForward = v.y;
            inputs.MoveAxisRight = v.x;
            inputs.CameraRotation = quaternion.identity;
            
            Character.SetInputs(ref inputs);
        }
    }
}