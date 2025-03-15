using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class RequestHandler
    {
        public async UniTask<string> SendStringRequest(string url, CancellationToken token)
        {
            var request = await SendRequest(UnityWebRequest.Get(url), token);
            var result = request.downloadHandler.text;
            return !string.IsNullOrEmpty(result) ?  result : null;
        }
        
        public async UniTask<Texture2D> SendTextureRequest(string url, CancellationToken token)
        {
            var request = await SendRequest(UnityWebRequestTexture.GetTexture(url), token);
            return request != null ? DownloadHandlerTexture.GetContent(request) : null;
        }
        
        private async UniTask<UnityWebRequest> SendRequest(UnityWebRequest request, CancellationToken token)
        {
            request.timeout = 10;

            var operation = request.SendWebRequest();

            try
            {
                await UniTask.WaitUntil(() => operation.isDone, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                request.Abort();
                
                return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Request failed: {request.error}");
                
                return null;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token); 

            return request;
        }
    }
}