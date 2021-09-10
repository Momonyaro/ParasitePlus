using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class TargetingUI : MonoBehaviour
    {
        public Image cursorImage;
        public float singleTargetWidth = 150;
        public float multiTargetWidth = 800;

        public void SetCursorPosAndVisibility(Vector2 canvasPos, bool visibility, bool multiTarget)
        {
            cursorImage.enabled = visibility;
            PlaceCursorAtCanvasPos(canvasPos, multiTarget);   
        }
        
        public void PlaceCursorAtCanvasPos(Vector2 canvasPos, bool multiTarget)
        {
            RectTransform imageTransform = cursorImage.GetComponent<RectTransform>();
            imageTransform.position = canvasPos;
            
            Vector2 sizeDelta = imageTransform.sizeDelta;

            float xDelta = (multiTarget) ? multiTargetWidth : singleTargetWidth;
            sizeDelta = new Vector2(xDelta, singleTargetWidth);
            
            imageTransform.sizeDelta = sizeDelta;
        }
    }
}
