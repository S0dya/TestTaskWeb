using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.Dogs
{
    public class DogDescriptionView : MonoBehaviour
    {
        [SerializeField] private GameObject windowObject;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button[] closeButtons;

        private void Awake()
        {
            CloseWindow();
            
            foreach (var closeButton in closeButtons)
            {
                closeButton.onClick.AddListener(OnCloseWindow);
            } 
        }

        public void OpenWindow(string name, string description)
        {
            nameText.text = name;
            descriptionText.text = description;
            
            windowObject.SetActive(true);
        }
        public void CloseWindow()
        {
            windowObject.SetActive(false);
        }
        
        private void OnCloseWindow()
        {
            CloseWindow();
        }
    }
}