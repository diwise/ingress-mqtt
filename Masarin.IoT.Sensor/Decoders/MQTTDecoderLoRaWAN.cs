using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using System;
using System.Text;
using Fiware;
using System.Linq;

namespace Masarin.IoT.Sensor

{
    public class MQTTDecoderLoRaWAN : MQTTDecoder
    {
        private readonly IContextBrokerProxy _fiwareContextBroker = null;
        public MQTTDecoderLoRaWAN(IContextBrokerProxy fiwareContextBroker)
        {
            _fiwareContextBroker = fiwareContextBroker;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            string json = Encoding.UTF8.GetString(payload);
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            var deviceName = "se:servanet:lora:" + Convert.ToString(data.deviceName);
            var obj = data["object"];
            
            if (deviceName.Contains("sn-elt-livboj-"))
            {
                if (obj.ContainsKey("present"))
                {
                    var present = obj.present;
                    string value = "on";

                    if (present == false)
                    {
                        value = "off";
                    }

                    var message = new Fiware.DeviceMessage(deviceName, value);

                    _fiwareContextBroker.PostMessage(message);
                }
                else
                {
                    Console.WriteLine($"No \"present\" property in message from deviceName {deviceName}!: {json}");
                    return;
                }
            } 
            else if (deviceName.Contains("sk-elt-temp-"))
            {
                if (obj.ContainsKey("externalTemperature"))
                {
                    double value = obj.externalTemperature;

                    string stringValue = $"t%3D{value}";

                    var message = new Fiware.DeviceMessage(deviceName, stringValue);

                    _fiwareContextBroker.PostMessage(message);
                }
                else
                {
                    Console.WriteLine($"No \"externalTemperature\" property in message from deviceName {deviceName}!: {json}");
                    return;
                }
            }
            else if (deviceName.Contains("TRSense01"))
            {
                if (obj.ContainsKey("L0_CNT") && obj.ContainsKey("R0_CNT")) {

                    string shortDeviceName = deviceName.Remove(0,16);
                    string dateStr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 0, (int)obj.L0_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 1, (int)obj.L1_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 2, (int)obj.L2_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 3, (int)obj.L3_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 4, (int)obj.R0_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 5, (int)obj.R1_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 6, (int)obj.R2_CNT);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStr, 7, (int)obj.R3_CNT);
                }
                else
                {
                    Console.WriteLine($"No \"L0_CNT\" property in message from deviceName {deviceName}!: {json}");
                    return;
                }
            }
            else if (deviceName.Contains("mcg-ers-co2-"))
            {
                if (obj.ContainsKey("co2"))
                {
                    double value = obj.co2;
                    var strValue = $"co2%3D{value}";
                    var msg = new Fiware.DeviceMessage(deviceName, strValue);
                    _fiwareContextBroker.PostMessage(msg);
                }
                else
                {
                    Console.WriteLine($"No \"co2\" property in message from deviceName {deviceName}!: {json}");
                    return;
                }
            }

            Console.WriteLine($"Got message from deviceName {deviceName}: {json}");
        }
        private void ReportTrafficIntensityForLane(string deviceName, string dateStr, int lane, int intensity) {

            string refRoad = "urn:ngsi-ld:RoadSegment:19312:2860:35243";
            string shortDeviceName = $"{deviceName}:{lane}:{dateStr}";
            
            if (intensity > 0) {
                var message = new Fiware.TrafficFlowObserved(shortDeviceName, dateStr, lane, intensity, refRoad);

                try {
                    _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                } catch (Exception e) 
                {
                    Console.WriteLine($"Exception caught attempting to post TrafficFlowObserved: {e.Message}");
                };
            }
        }
    }
}
