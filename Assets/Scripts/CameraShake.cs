using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraShake
    {
        [Serializable]
        public struct CameraShakeProperties
        {
            public float frequency;

            public float recoverySpeed;
            public float traumaExponent;
            
            public Vector3 maximumTranslationShake;
            public Vector3 maximumAngularShake;
        }
        
        private readonly CameraShakeProperties properties;

        private float trauma;
        private Transform target;
        private Vector3 localPosition, localRotation; 
        
        public CameraShake(CameraShakeProperties args)
        {
            properties = args;
            trauma = 0f;
        }

        public void SetTrauma(float trauma)
        {
            this.trauma = trauma;
        }

        public void SetNewTarget(Transform newTrackingObject)
        {
            if (target != null)
            {
                target.localPosition = localPosition;
                target.localRotation = Quaternion.Euler(localRotation);
            }

            target = newTrackingObject;
            localPosition = newTrackingObject.localPosition;
            localRotation = newTrackingObject.localRotation.eulerAngles;
        }

        public void Process()
        {
            if (target == null) return;
            
            var shake = Mathf.Pow(trauma, properties.traumaExponent);
            target.localPosition = localPosition + new Vector3(
                properties.maximumTranslationShake.x * Mathf.PerlinNoise(0, Time.time * properties.frequency) * 2 - 1, 
                properties.maximumTranslationShake.y * Mathf.PerlinNoise(1, Time.time * properties.frequency) * 2 - 1,
                properties.maximumTranslationShake.z * Mathf.PerlinNoise(2, Time.time * properties.frequency) * 2 - 1
            ) * shake;
            target.localRotation = Quaternion.Euler(localRotation + new Vector3(
                properties.maximumAngularShake.x * (Mathf.PerlinNoise(3, Time.time * properties.frequency) * 2 - 1),
                properties.maximumAngularShake.y * (Mathf.PerlinNoise(4, Time.time * properties.frequency) * 2 - 1),
                properties.maximumAngularShake.z * (Mathf.PerlinNoise(5, Time.time * properties.frequency) * 2 - 1)
            ) * shake);
            trauma = Mathf.Clamp01(trauma - properties.recoverySpeed * Time.deltaTime);
        }
    }
}