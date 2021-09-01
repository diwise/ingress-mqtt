using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fiware
{
    public class TrafficFlowObserved
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("dateObserved")]
        public TextProperty DateObserved { get; set; }

        [JsonProperty("location")]
        public GeoProperty Location { get; set; }

        [JsonProperty("laneID")]
        public NumberPropertyFromInt LaneID { get; set; }

        [JsonProperty("refRoadSegment")]
        public TextProperty RefRoadSegment { get; set; }

        [JsonProperty("intensity")]
        public NumberPropertyFromInt Intensity { get; set; }

        [JsonProperty("averageVehicleSpeed")]
        public NumberPropertyFromDouble AverageVehicleSpeed { get; set; }

        [JsonProperty("@context")]
        public string[] Context { get; set; }

        public TrafficFlowObserved(string id, string dateObserved, int laneId, int intensity, string refRoad)
        {
            Id = "urn:ngsi-ld:TrafficFlowObserved:" + id;
            Type = "TrafficFlowObserved";
            Context = new string[2] { "https://schema.lab.fiware.org/ld/context", "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" };
            DateObserved = new TextProperty(dateObserved);
            LaneID = new NumberPropertyFromInt(laneId);
            Intensity = new NumberPropertyFromInt(intensity);
            RefRoadSegment = new TextProperty(refRoad);
        }

    }

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

}
