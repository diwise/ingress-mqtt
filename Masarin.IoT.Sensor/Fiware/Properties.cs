using Newtonsoft.Json;
using System;

namespace Fiware
{
    public class DateTimeProperty
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        [JsonProperty("@value")]
        public string Value { get; set; }

        public DateTimeProperty(string value)
        {
            Type = "Property";
            Value = value;
        }
    }

    public class NumberPropertyFromInt
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }

        public NumberPropertyFromInt(int value)
        {
            Type = "Property";
            Value = value;
        }
    }

    public class NumberPropertyFromDouble
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public double Value { get; set; }

        public NumberPropertyFromDouble(double value)
        {
            Type = "Property";
            Value = value;
        }
    }

    public class TextProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }

        public TextProperty(string value)
        {
            Type = "Property";
            Value = value;
        }
    }

    public class GeoProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public GeoPropertyValue Value { get; set; }

        public GeoProperty(double latitude, double longitude)
        {
            Type = "GeoProperty";
            Value = new GeoPropertyValue(latitude, longitude);
        }

    }

    public class GeoPropertyValue
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }

        public GeoPropertyValue(double latitude, double longitude)
        {
            Type = "Point";
            Coordinates = new double[2] { longitude, latitude };
        }

    }

    public class SingleObjectRelationship
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("object")]
        public String Object { get; set; }

        public SingleObjectRelationship(string theObject) {
            Type = "Relationship";
            Object = theObject;
        }
    }

}
