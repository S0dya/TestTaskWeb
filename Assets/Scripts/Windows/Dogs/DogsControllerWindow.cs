using System;
using System.Collections.Generic;
using System.Threading;
using Network;
using UnityEngine;
using Zenject;

namespace Windows.Dogs
{
    public class DogsControllerWindow : WindowBase
    {
        [SerializeField] private DogSlotView[] dogSlotViews;
        [SerializeField] private DogDescriptionView dogDescriptionView;
        
        [Inject] private RequestHandler _requestHandler;
        [Inject] private RequestsQueue _requestsQueue;
        
        private DogSlotView _currentDogSlotView;
        
        private CancellationTokenSource _cts;

        public override void OpenWindow()
        {
            gameObject.SetActive(true);

            FetchDogs(OnWindowOpened);
        }
        public override void CloseWindow()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            gameObject.SetActive(false);

            _currentDogSlotView?.StopLoading();
        }

        private async void FetchDogs(Action onFinished = null)
        {
            _cts = new CancellationTokenSource();

            try
            {
                _requestsQueue.Enqueue(
                    async () => await _requestHandler.SendStringRequest(APIEndpoints.DogBreedsAPI, _cts.Token),
                    result => 
                    {
                        if (!string.IsNullOrEmpty(result))
                        {
                            var dogsData = JsonUtility.FromJson<DogsData>(result);

                            if (dogsData != null && dogsData.data?.Count > 0)
                            {
                                InitSlots(dogsData.data);
                                
                                onFinished?.Invoke();
                            }
                        }
                    });
            }
            catch (OperationCanceledException)
            {
                DebugManager.Log(DebugCategory.Net, "FetchDogs was cancelled");
            }
        }

        private void FetchDog(string id)
        {
            _requestsQueue.Enqueue(
                async () => await _requestHandler.SendStringRequest(APIEndpoints.GetBreedDetails(id), _cts.Token),
                result => 
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        var dogData = JsonUtility.FromJson<SingleDogData>(result);

                        if (dogData != null)
                        {
                            dogDescriptionView.OpenWindow(dogData.data.attributes.name, dogData.data.attributes.description);
                                
                            _currentDogSlotView.StopLoading();
                            _currentDogSlotView = null;
                        }
                    }
                });
        }

        private void InitSlots(List<DogData> dogsData)
        {
            for (int i = 0; i < dogSlotViews.Length; i++)
            {
                int index = i;
                dogSlotViews[i].Init(
                    () => OnOpenSlotPressed(index, dogsData[index].id), 
                    dogsData[index].attributes.name, index + 1);
            }
        }

        private void OnOpenSlotPressed(int slotI, string id)
        {
            if (_currentDogSlotView == dogSlotViews[slotI]) return;
            
            _currentDogSlotView?.StopLoading();
            _currentDogSlotView = dogSlotViews[slotI];
            _currentDogSlotView.StartLoading();
            
            FetchDog(id);
        }
    }
}