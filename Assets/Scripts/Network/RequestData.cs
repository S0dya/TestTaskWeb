using System;
using Cysharp.Threading.Tasks;

namespace Network
{
    public interface IRequestData
    {
        public UniTask<object> Execute();
        public void Complete(object result);
        public void Cancel();
        public bool IsCancelled { get; }
        public string RequestId { get; }
    }

    public class RequestData<T> : IRequestData
    {
        private readonly Func<UniTask<T>> _requestFunc;
        private readonly Action<T> _onComplete;
        private readonly Action _onCancel;
    
        public bool IsCancelled { get; private set; }
        public string RequestId { get; } 

        public RequestData(string requestId, Func<UniTask<T>> requestFunc, Action<T> onComplete, Action onCancel)
        {
            RequestId = requestId;
            _requestFunc = requestFunc;
            _onComplete = onComplete;
            _onCancel = onCancel;
        }

        public async UniTask<object> Execute()
        {
            return IsCancelled ? default : await _requestFunc();
        }

        public void Complete(object result)
        {
            if (!IsCancelled && result is T)
            {
                _onComplete?.Invoke((T)result);
            }
        }

        public void Cancel()
        {
            IsCancelled = true;
            _onCancel?.Invoke();
        }
    }
}