using System;
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
            currentRotation = Quaternion.LookRotation(_moveInputVector, Motor.CharacterUp);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Reorient source velocity on current ground slope
            // (this is because we don't want our smoothing to cause any velocity losses in slope changes)
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, 
                Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(
                Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
            Vector3 tmv = reorientedInput * 5;

            currentVelocity = tmv;
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