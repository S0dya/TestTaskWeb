using System;
using UnityEngine;

namespace Windows
{
    public abstract class WindowBase : MonoBehaviour
    {
        public abstract void OpenWindow(Action onOpenFinished);
        public abstract void CloseWindow();
    }
}