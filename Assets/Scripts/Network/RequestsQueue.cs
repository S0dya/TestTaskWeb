using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Network
{
    public class RequestsQueue
    {
        private Queue<IRequestData> _queue = new();
        private bool _isProcessing;
        private IRequestData _currentRequest;

        public void Enqueue<T>(string requestId, Func<UniTask<T>> requestFunc, Action<T> onComplete, Action onCancel = null)
        {
            CancelRequest(requestId);

            var request = new RequestData<T>(requestId, requestFunc, onComplete, onCancel);
            _queue.Enqueue(request);

            DebugManager.Log(DebugCategory.Net, $"Request enqueued - {requestId}");
    
            if (!_isProcessing)
            {
                ProcessQueue().Forget();
            }
        }
        
        public void CancelRequest(string requestId)
        {
            var request = _queue.FirstOrDefault(x => x.RequestId == requestId);
            if (request != null)
            {
                request.Cancel();
                
                DebugManager.Log(DebugCategory.Net, $"Existing request cancelled - {requestId}");
            }
        }

        private async UniTaskVoid ProcessQueue()
        {
            _isProcessing = true;

            while (_queue.Count > 0)
            {
                _currentRequest = _queue.Dequeue();

                if (_currentRequest.IsCancelled)
                {
                    DebugManager.Log(DebugCategory.Net, $"Skipped cancelled request - {_currentRequest.RequestId}");
                    continue;
                }

                try
                {
                    var executeTask = _currentRequest.Execute();
            
                    while (executeTask.Status == UniTaskStatus.Pending)
                    {
                        if (_currentRequest.IsCancelled)
                        {
                            DebugManager.Log(DebugCategory.Net, $"Request {_currentRequest.RequestId} cancelled while executing");
                            break;
                        }
                        await UniTask.Yield(); 
                    }

                    if (!_currentRequest.IsCancelled)
                    {
                        object result = await executeTask;
                        _currentRequest.Complete(result);
                        DebugManager.Log(DebugCategory.Net, "Current request completed");
                    }
                }
                catch (OperationCanceledException)
                {
                    DebugManager.Log(DebugCategory.Net, $"Request {_currentRequest.RequestId} was forcefully cancelled");
                }
            }

            _isProcessing = false;
            _currentRequest = null;
        }
    }
}