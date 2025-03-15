namespace Network
{
    public static class APIEndpoints
    {
        public const string WeatherAPI = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
        public const string DogBreedsAPI = "https://dogapi.dog/api/v2/breeds";
        public static string GetBreedDetails(string breedId) => $"https://dogapi.dog/api/v2/breeds/{breedId}";
    }
}