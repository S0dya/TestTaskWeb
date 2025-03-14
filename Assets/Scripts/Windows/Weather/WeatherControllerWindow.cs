using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Network;
using UnityEngine;
using Zenject;

namespace Windows.Weather
{
    public class WeatherControllerWindow : WindowBase
    {
        [SerializeField] private float weatherUpdateInterval = 5;
        [Space]
        [SerializeField] private WeatherView weatherView;

        [Inject] private RequestHandler _requestHandler;
        [Inject] private RequestsQueue _requestsQueue;
        
        private CancellationTokenSource _cts;
        
        public override void OpenWindow()
        {
            gameObject.SetActive(true);
            
            FetchWeather(OnWindowOpened);
        }
        
        public override void CloseWindow()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            gameObject.SetActive(false);
        }
        
        private async void FetchWeather(Action onFinished = null)
        {
            _cts = new CancellationTokenSource();

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    FetchAndProcessWeather(onFinished);
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(weatherUpdateInterval), cancellationToken: _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                DebugManager.Log(DebugCategory.Net, "FetchWeather was cancelled");
            }
        }
        
        private void FetchAndProcessWeather(Action onFinished = null)
        {
            DebugManager.Log(DebugCategory.Net, "Fetch weather");
            
            _requestsQueue.Enqueue(
                async () => await _requestHandler.SendStringRequest(APIEndpoints.WeatherAPI, _cts.Token),
                result =>
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        var weatherResponse = JsonUtility.FromJson<WeatherData>(result);
                        
                        if (weatherResponse != null && weatherResponse.properties?.periods?.Count > 0)
                        {
                            ProcessWeatherPeriod(weatherResponse.properties.periods[0], onFinished);
                        }
                    }
                },
                () => _cts.Cancel()
            );    
        }

        private void ProcessWeatherPeriod(WeatherPeriod period, Action onFinished = null)
        {
            DebugManager.Log(DebugCategory.Net, "Process weather period");

            _requestsQueue.Enqueue(async () => await  _requestHandler.SendTextureRequest(period.icon, _cts.Token),
                resultTexture =>
                {
                    if (resultTexture != null)
                    {
                        DebugManager.Log(DebugCategory.Net, "Set weather");

                        weatherView.SetWeather(resultTexture.ConvertToSprite(), period.temperature.ToString());

                        onFinished?.Invoke();
                    }
                }, 
                () => _cts.Cancel());
        }
    }
}