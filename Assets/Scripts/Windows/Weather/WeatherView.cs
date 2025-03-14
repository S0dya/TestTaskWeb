using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.Weather
{
    public class WeatherView : MonoBehaviour
    {
        [SerializeField] private Image weatherIcon;
        [SerializeField] private TextMeshProUGUI weatherText;

        public void SetWeather(Sprite sprite, string text)
        {
            if (sprite != null) weatherIcon.sprite = sprite;
            weatherText.text = "Сегодня - " + text;
        }
    }
}