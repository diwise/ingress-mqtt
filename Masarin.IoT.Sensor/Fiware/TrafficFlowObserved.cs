using Newtonsoft.Json;

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
}
