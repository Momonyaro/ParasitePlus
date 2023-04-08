using System.Collections;
using TMPro;
using UnityEngine;

namespace BattleSystem.UI
{
    public class BottomPanelUI : MonoBehaviour
    {
        //This script is responsible for providing a list of selectable options, descriptions for those options and that's
        // about it...

        public SelectableWheelOption[] options = new SelectableWheelOption[0];
        public int currentlySelected = 0;
        
        public TextMeshProUGUI previousObjectText;
        public TextMeshProUGUI activeApCostText;
        public TextMeshProUGUI[] optionText;

        public TextMeshProUGUI descriptionContainer;

        private const float revealYPos = 145.63f;
        private const float hiddenYPos = -106;

        public void PopulateOptions(SelectableWheelOption[] newOptions, int startIndex = 0)
        {
            options = newOptions;
            currentlySelected = startIndex;
            UpdateOptionsDisplay();
        }

        public void NavigateOptions(int difference)
        {
            int clampedNewIndex = Mathf.Clamp(currentlySelected + difference, 0, options.Length - 1);
            
            currentlySelected = clampedNewIndex; // I'm fucking stupid, I swear... 
            UpdateOptionsDisplay();
        }

        public string GetSelectedReference()
        {
            return options[currentlySelected].optionRef;
        }

        public void UpdateOptionsDisplay(bool hidePlayer = false)
        {
            //Set all options as blank
            previousObjectText.text = "";
            for (int i = 0; i < optionText.Length; i++)
            {
                optionText[i].text = "";
            }

            if (currentlySelected > 0)
                previousObjectText.text = options[currentlySelected - 1].title;
            
            int q = 0;
            for (int i = currentlySelected; i < options.Length; i++)
            {
                if (q >= optionText.Length) continue;
                optionText[q].text = (q == 0) ? options[i].title.ToUpper() : options[i].title;
                q++;
            }

            descriptionContainer.text = options[currentlySelected].description;
            activeApCostText.text = options[currentlySelected].cost;
        }

        public void SetMenuVisibility(bool visibility)
        {
            StartCoroutine(MenuVisibilityEnumerator(visibility));
        }

        private IEnumerator MenuVisibilityEnumerator(bool visibility)
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            float finalY = visibility ? revealYPos : hiddenYPos;
            Vector3 startPos = rectTransform.anchoredPosition;
            Vector3 finalPos = startPos;
            finalPos.y = finalY;
            float timer = .6f;

            while (timer > 0)
            {
                rectTransform.anchoredPosition = Vector3.Lerp(finalPos, startPos, timer);
                timer -= Time.deltaTime * 2;
                yield return new WaitForEndOfFrame();
            }

            rectTransform.anchoredPosition = finalPos;
            
            yield break;
        }
    }

    [System.Serializable]
    public struct SelectableWheelOption
    {
        public string title;
        public string cost;
        public string optionRef;
        public string description;

        public SelectableWheelOption(string title, string cost, string optionRef, string description)
        {
            this.title = title;
            this.cost = cost;
            this.optionRef = optionRef;
            this.description = description;
        }
    }
}
