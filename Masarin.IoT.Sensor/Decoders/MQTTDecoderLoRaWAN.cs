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

                    string newId = deviceName.Remove(0,16);
                    string dateStr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    string refRoad = "urn:ngsi-ld:RoadSegment:19312:2860:35243";
                    int intensity;

                    if (obj.L0_CNT > 0) {
                        intensity = obj.L0_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 0, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.L1_CNT > 0) {
                        intensity = obj.L1_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 1, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.L2_CNT > 0) {
                        intensity = obj.L2_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 2, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.L3_CNT > 0) {
                        intensity = obj.L3_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 3, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.R0_CNT > 0) {
                        intensity = obj.R0_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 4, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.R1_CNT > 0) {
                        intensity = obj.R1_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 5, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.R2_CNT > 0) {
                        intensity = obj.R2_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 6, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }

                    if (obj.R3_CNT > 0) {
                        intensity = obj.R3_CNT;
                        var message = new Fiware.TrafficFlowObserved(newId, dateStr, 7, intensity, refRoad);

                        _fiwareContextBroker.PostNewTrafficFlowObserved(message);
                    }
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
    }
}
