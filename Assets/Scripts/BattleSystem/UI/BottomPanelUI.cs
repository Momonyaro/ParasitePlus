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
        public TextMeshProUGUI[] optionText;

        public TextMeshProUGUI descriptionContainer;

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

        public void UpdateOptionsDisplay()
        {
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
                optionText[q].text = (q == 0) ? options[i].title.ToUpper() : options[i].title;
                q++;
            }

            descriptionContainer.text = options[currentlySelected].description;
        }
    }

    [System.Serializable]
    public struct SelectableWheelOption
    {
        public string title;
        public string optionRef;
        public string description;

        public SelectableWheelOption(string title, string optionRef, string description)
        {
            this.title = title;
            this.optionRef = optionRef;
            this.description = description;
        }
    }
}
