using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
    public class WindowsController : MonoBehaviour
    {
        [SerializeField] private WindowEnum initialWindow = WindowEnum.Weather;
        [Space]
        [SerializeField] private GameObject loader;
        [SerializeField] private WindowData[] windows;
        [Space]
        [SerializeField] private Button weatherButton;
        [SerializeField] private Button dogsButton;
    
        private WindowEnum _currentWindow = WindowEnum.none;
    
        private void Awake()
        {
            CloseAllWindows();
            loader.SetActive(false);

            weatherButton.onClick.AddListener(() => OpenWindow(WindowEnum.Weather));
            dogsButton.onClick.AddListener(() => OpenWindow(WindowEnum.Dogs));
        }

        private void Start()
        {
            OpenWindow(initialWindow);
        }

        private void OpenWindow(WindowEnum windowKey)
        {
            if (_currentWindow == windowKey) return; 
            
            var windowData = windows.FirstOrDefault(x => x.Key == windowKey);
            
            if (windowData == null)
            {
                DebugManager.Log(DebugCategory.Errors, $"window with key {windowKey} not found", DebugStatus.Error);
                
                return;
            }

            CloseAllWindows();
            
            loader.SetActive(true);
        
            windowData.Window.OpenWindow(() => loader.SetActive(false));
            _currentWindow = windowData.Key;
        }
    
        private void CloseAllWindows()
        {
            foreach (var win in windows)
                win.Window.CloseWindow();
        }
    }

    [System.Serializable]
    class WindowData
    {
        [SerializeField] private WindowEnum key;
        [SerializeField] private WindowBase window;
        
        public WindowEnum Key => key;
        public WindowBase Window => window;
    }

    public enum WindowEnum
    {
        none,
        Weather,
        Dogs,
    }
}