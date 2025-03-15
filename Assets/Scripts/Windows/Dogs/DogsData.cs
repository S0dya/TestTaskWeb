using System;
using System.Collections.Generic;

namespace Windows.Dogs
{
    [Serializable]
    public class DogsResponse
    {
        public List<DogData> data;
    }

    [Serializable]
    public class DogResponse
    {
        public DogData data;
    }
    
    [Serializable]
    public class DogData
    {
        public string id;
        public string type;
        public DogAttributes attributes;
    }

    [Serializable]
    public class DogAttributes
    {
        public string name;
        public string description;
    }
}