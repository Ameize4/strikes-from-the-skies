using KinematicCharacterController;
using UnityEngine;

namespace DefaultNamespace
{
    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
    }
    
    public class CharacterController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        public Transform playerInputSpace;

        public float speedWalk = 2f;
        public float speedRun = 5f;
        
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        
        private void Start()
        {
            Motor.CharacterController = this;
        }

        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            _moveInputVector = new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward);
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_moveInputVector == Vector3.zero) return;
            currentRotation = Quaternion.LookRotation(_moveInputVector, Motor.CharacterUp);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            float speed = speedWalk;
            if (Input.GetKey(KeyCode.LeftShift))
                speed = speedRun;
            
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient source velocity on current ground slope
                // (this is because we don't want our smoothing to cause any velocity losses in slope changes)
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, 
                    Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(
                    Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                Vector3 tmv = reorientedInput * speed;
                tmv = playerInputSpace.TransformDirection(tmv);
                currentVelocity = tmv;
            }
            else
            {
                // Gravity
                currentVelocity += new Vector3(0, -30, 0) * deltaTime;
            }
        }

        #region NotImplemented

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        #endregion
    }
}