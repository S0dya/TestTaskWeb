using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public class RequestsQueue
    {
        private Queue<IRequestData> _queue = new();
        private bool _isProcessing;
        private IRequestData _currentRequest;

        public void Enqueue<T>(Func<UniTask<T>> requestFunc, Action<T> onComplete, Action onCancel = null)
        {
            var request = new RequestData<T>(requestFunc, onComplete, onCancel);
            _queue.Enqueue(request);

            DebugManager.Log(DebugCategory.Net, "Request enqueued");
            
            if (!_isProcessing)
            {
                ProcessQueue().Forget();
            }
        }

        public void RemovePendingRequests()
        {
            _queue.Clear();
            
            DebugManager.Log(DebugCategory.Net, "Queue cleared");
        }

        public void CancelCurrentRequest()
        {
            if (_currentRequest != null)
            {
                _currentRequest.Cancel();
                _currentRequest = null;

                DebugManager.Log(DebugCategory.Net, "Current request canceled");
            }
        }

        private async UniTaskVoid ProcessQueue()
        {
            DebugManager.Log(DebugCategory.Net, "Request started processing");
            
            _isProcessing = true;

            while (_queue.Count > 0)
            {
                _currentRequest = _queue.Dequeue();
                if (_currentRequest.IsCancelled) continue;

                object result = await _currentRequest.Execute();
                if (!_currentRequest.IsCancelled)
                {
                    _currentRequest.Complete(result);
                    
                    DebugManager.Log(DebugCategory.Net, "Current request completed");
                }
            }

            _isProcessing = false;
            _currentRequest = null;
        }
    }
}