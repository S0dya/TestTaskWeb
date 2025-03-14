using System;
using UnityEngine;

namespace Windows
{
    public abstract class WindowBase : MonoBehaviour
    {
        public Action OnWindowOpened;
        public abstract void OpenWindow();
        public abstract void CloseWindow();
    }
}