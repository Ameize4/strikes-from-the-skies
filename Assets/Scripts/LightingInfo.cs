using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu]
    public class LightingInfo : ScriptableObject
    {
        public Texture2D Color;
        public Texture2D ShadowMask;
    }
}