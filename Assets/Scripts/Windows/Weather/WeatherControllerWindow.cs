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
        
        public override void OpenWindow(Action onOpenFinished)
        {
            gameObject.SetActive(true);
            
            FetchWeather(onOpenFinished);
        }
        
        public override void CloseWindow()
        {
            _cts?.Cancel();
            
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
                DebugManager.Log(DebugCategory.Net, "Fetch weather was cancelled");
            }
        }
        
        private void FetchAndProcessWeather(Action onFinished = null)
        {
            DebugManager.Log(DebugCategory.Net, "Fetch weather");
            
            _requestsQueue.Enqueue(
                RequestsIDs.FetchWeather,
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
            DebugManager.Log(DebugCategory.Net, "Process weather period, fetch icon");

            _requestsQueue.Enqueue(
                RequestsIDs.FetchWeatherIcon,
                async () => await  _requestHandler.SendTextureRequest(period.icon, _cts.Token),
                resultTexture =>
                {
                    if (resultTexture != null)
                    {
                        weatherView.SetWeather(resultTexture.ConvertToSprite(), period.temperature.ToString());

                        onFinished?.Invoke();
                    }
                }, 
                () => _cts.Cancel());
        }
    }
}