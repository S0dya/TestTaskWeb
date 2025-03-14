using System;
using System.Collections.Generic;

namespace Windows.Weather
{
    [Serializable]
    public class WeatherData
    {
        public WeatherProperties properties;
    }

    [Serializable]
    public class WeatherProperties
    {
        public List<WeatherPeriod> periods;
    }

    [Serializable]
    public class WeatherPeriod
    {
        public string name;
        public int temperature;
        public string icon;
        public string temperatureUnit;
        public string shortForecast;
    }
}