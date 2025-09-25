using UnityEngine;

namespace DefaultNamespace
{
    public class LightmapSwapper : MonoBehaviour
    {
        public void SetLightmaps(LightingInfo info)
        {
            LightmapData data = new LightmapData();
            data.lightmapColor = info.Color;
            // data.shadowMask = info.ShadowMask;
            LightmapSettings.lightmaps = new LightmapData[] { data };
        }
    }
}