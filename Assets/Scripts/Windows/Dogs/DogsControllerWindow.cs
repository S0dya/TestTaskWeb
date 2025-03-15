using System;
using System.Collections.Generic;
using System.Threading;
using Network;
using Tools;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace Windows.Dogs
{
    public class DogsControllerWindow : WindowBase
    {
        [SerializeField] private DogSlotView dogSlotView;
        [SerializeField] private Transform dogSlotsParent;
        [Space]
        [SerializeField] private DogDescriptionView dogDescriptionView;
        
        [Inject] private RequestHandler _requestHandler;
        [Inject] private RequestsQueue _requestsQueue;
        
        private DogSlotView _currentDogSlotView;
        
        private CancellationTokenSource _dogsCts;
        private CancellationTokenSource _dogCts;

        private ObjectPool _dogSlotsPool = new();
        private List<DogSlotView> _activeDogSlotViews = new();

        private void Awake()
        {
            _dogSlotsPool.Init(new []{ dogSlotView.gameObject }, dogSlotsParent, 10);
        }
        
        public override void OpenWindow(Action onOpenFinished)
        {
            gameObject.SetActive(true);

            FetchDogs(onOpenFinished);
        }
        public override void CloseWindow()
        {
            _dogsCts?.Cancel(); 
            _dogCts?.Cancel(); 
            
            gameObject.SetActive(false);

            _currentDogSlotView?.StopLoading();
            
            foreach (var activeDogSlotView in _activeDogSlotViews) _dogSlotsPool.Set(activeDogSlotView.gameObject);
            _activeDogSlotViews.Clear();
        }

        private void FetchDogs(Action onFinished = null)
        {
            _dogsCts = new CancellationTokenSource();

            DebugManager.Log(DebugCategory.Net, "Fetch dogs");
            
            _requestsQueue.Enqueue(
                RequestsIDs.FetchDogs,
                async () => await _requestHandler.SendStringRequest(APIEndpoints.DogBreedsAPI, _dogsCts.Token),
                result => 
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        var dogsData = JsonUtility.FromJson<DogsResponse>(result);

                        if (dogsData != null && dogsData.data?.Count > 0)
                        {
                            InitSlots(dogsData.data);
                            
                            onFinished?.Invoke();
                        }
                    }
                }, 
                () => _dogsCts.Cancel());
        }
        
        private void FetchDog(string id)
        {
            _dogCts?.Cancel(); 
            
            _dogCts = new CancellationTokenSource();
        
            DebugManager.Log(DebugCategory.Net, "Fetch dog " + id);

            _requestsQueue.Enqueue(
                RequestsIDs.FetchDog,
                async () => await _requestHandler.SendStringRequest(APIEndpoints.GetBreedDetails(id), _dogCts.Token),
                result =>
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        var dogData = JsonUtility.FromJson<DogResponse>(result);
                        if (dogData != null)
                        {
                            dogDescriptionView.OpenWindow(dogData.data.attributes.name,
                                dogData.data.attributes.description);

                            _currentDogSlotView.StopLoading();
                            _currentDogSlotView = null;
                        }
                    }
                },
                () => _dogCts.Cancel());
        }

        private void InitSlots(List<DogData> dogsData)
        {
            for (int i = 0; i < dogsData.Count; i++)
            {
                var dogSlotView = _dogSlotsPool.Get().GetComponent<DogSlotView>();
                
                int index = i;
                dogSlotView.Init(
                    () => OnOpenSlotPressed(index, dogsData[index].id), 
                    dogsData[index].attributes.name, index + 1);

                _activeDogSlotViews.Add(dogSlotView);
            }
        }

        private void OnOpenSlotPressed(int slotI, string id)
        {
            if (_currentDogSlotView == _activeDogSlotViews[slotI]) return;
            
            _currentDogSlotView?.StopLoading();
            _currentDogSlotView = _activeDogSlotViews[slotI];
            _currentDogSlotView.StartLoading();
            
            FetchDog(id);
        }
    }
}