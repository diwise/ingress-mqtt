using Newtonsoft.Json;

namespace Fiware
{
    public class WaterConsumptionObserved
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("waterConsumption")]
        public WaterConsumption Consumption { get; set; }

        [JsonProperty("@context")]
        public string[] Context { get; set; }

        public WaterConsumptionObserved(string id, string observedBy, string observedAt, int currentVolume)
        {
            Id = "urn:ngsi-ld:WaterConsumptionObserved:" + id;
            Type = "WaterConsumptionObserved";
            Context = new string[] { "https://raw.githubusercontent.com/easy-global-market/ngsild-api-data-models/master/WaterSmartMeter/jsonld-contexts/waterSmartMeter-compound.jsonld" };

            Consumption = new WaterConsumption("urn:ngsi-ld:Device:" + observedBy, observedAt, currentVolume);
        }

    }

    public class WaterConsumption : NumberPropertyFromInt
    {

        [JsonProperty("observedAt")]
        public string ObservedAt { get; set; }

        [JsonProperty("observedBy")]
        public SingleObjectRelationship ObservedBy { get; set; }

        [JsonProperty("unitCode")]
        public string UnitCode { get; set; }

        public WaterConsumption(string observedBy, string observedAt, int volume) : base(volume) {
            ObservedAt = observedAt;
            ObservedBy = new SingleObjectRelationship(observedBy);
            UnitCode = "LTR";
        }
    }
}
