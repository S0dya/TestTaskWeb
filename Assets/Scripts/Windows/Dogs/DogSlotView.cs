using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.Dogs
{
    public class DogSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI breedNameText;
        [SerializeField] private TextMeshProUGUI indexText;
        [SerializeField] private RectTransform loadingArrowsTransform;
        
        private float _initialArrowRotation;
        
        private Tween _loadingTween;

        private void Awake()
        {
            StopLoading();
        }
        
        public void Init(Action onButtonPressed, string breedName, int index)
        {
            button.onClick.AddListener(() => onButtonPressed?.Invoke());
            breedNameText.text = breedName;
            indexText.text = index.ToString();
        }
        
        public void StartLoading()
        {
            PlayLoading();
            loadingArrowsTransform.gameObject.SetActive(true);
        }
        public void StopLoading()
        {
            _loadingTween?.Kill();
            loadingArrowsTransform.gameObject.SetActive(false);
            
            loadingArrowsTransform.rotation = Quaternion.Euler(0, 0, _initialArrowRotation);
        }

        private void PlayLoading()
        {
            _loadingTween?.Kill();
            
            _loadingTween = loadingArrowsTransform
                .DORotate(new Vector3(0, 0, 360), 1, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear) 
                .SetLoops(-1, LoopType.Restart);
        }
    }
}