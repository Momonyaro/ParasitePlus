using UnityEngine;

namespace UI
{
    public class MinimapTreatment : MonoBehaviour
    {
        public Material screenMat;
        public RenderTexture output;
        
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, screenMat);
            output = dest;
        }
    }
}
