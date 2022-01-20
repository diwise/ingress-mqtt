using Newtonsoft.Json;

namespace Fiware 
{
    public class AirQualityObserved 
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    
        [JsonProperty("@context")]
        public string[] Context { get; set; }

        [JsonProperty("dateObserved")]
        public TextProperty DateObserved { get; set; }

        [JsonProperty("location")]
        public GeoProperty Location { get; set; }

        [JsonProperty("CO2")]
        public Pollutant CO2 { get; set; }

        [JsonProperty("relativeHumidity")]
        public NumberPropertyFromDouble Humidity { get; set; }

        [JsonProperty("temperature")]
        public NumberPropertyFromDouble Temperature { get; set; }

        public AirQualityObserved(string id, string dateObserved)
        {
            Id = "urn:ngsi-ld:AirQualityObserved:" + id;
            Type = "AirQualityObserved";
            Context = new string[2] { "https://schema.lab.fiware.org/ld/context", "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" };
            DateObserved = new TextProperty(dateObserved);
        }

        public AirQualityObserved WithCO2(double ppm)
        {
            CO2 = new Pollutant(ppm);
            return this;
        }

        public AirQualityObserved WithHumidity(double humidity)
        {
            Humidity = new NumberPropertyFromDouble(humidity);
            return this;
        }

        public AirQualityObserved WithTemperature(double temp)
        {
            Temperature = new NumberPropertyFromDouble(temp);
            return this;
        }

    }

    public partial class Pollutant
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("unitCode")]
        public string UnitCode { get; set; }

        public Pollutant(double ppm)
        {
            Type = "Property";
            Value = ppm;
            UnitCode = "59";
        }
    }

}