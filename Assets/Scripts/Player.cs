using System;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public class Player : MonoBehaviour
    {
        public CharacterController Character;
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs inputs = new PlayerCharacterInputs();

            inputs.MoveAxisForward = Input.GetAxisRaw("Vertical");
            inputs.MoveAxisRight = Input.GetAxisRaw("Horizontal");
            inputs.CameraRotation = quaternion.identity;
            
            Character.SetInputs(ref inputs);
        }
    }
}