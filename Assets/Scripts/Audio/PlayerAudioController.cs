using System;
using Sonity;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Footstep Settings")]
        [SerializeField] private SoundEvent footstepSound;
        [SerializeField] private float stepDistance = 1.0f;
        [SerializeField] private float minVelocity = 0.1f;

        private CharacterController character;
        private Vector3 lastPosition;
        private float distanceAccumulator;

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            HandleFootsteps();
        }

        private void HandleFootsteps()
        {
            float velocity = character.Motor.Velocity.magnitude;
            if (velocity < minVelocity)
                return;

            Vector3 currentPosition = transform.position;
            float deltaDistance = Vector3.Distance(currentPosition, lastPosition);
            distanceAccumulator += deltaDistance;
            lastPosition = currentPosition;

            if (distanceAccumulator >= stepDistance)
            {
                PlayFootstep();
                distanceAccumulator = 0f;
            }
        }

        private void PlayFootstep()
        {
            footstepSound.Play(transform);
        }
    }
}
