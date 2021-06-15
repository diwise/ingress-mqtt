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
            else if (deviceName.Contains("sn-tcr-01"))
            {
                if (obj.ContainsKey("L0_CNT") && obj.ContainsKey("R0_CNT")) {
                    int[] leftArr = new int[4] {obj.L0_CNT, obj.L1_CNT, obj.L2_CNT, obj.L3_CNT};

                    int totalCountLeft = leftArr.Sum();
                    
                    string newId = deviceName.Remove(0,16);
                    string dateStr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    string refRoad = "urn:ngsi-ld:RoadSegment:19312:2860:35243";

                    var leftMessage = new Fiware.TrafficFlowObserved(newId, dateStr, 0, totalCountLeft, refRoad);

                    _fiwareContextBroker.PostNewTrafficFlowObserved(leftMessage);

                    int[] rightArr = new int[4] {obj.L0_CNT, obj.L1_CNT, obj.L2_CNT, obj.L3_CNT};

                    int totalCountRight = rightArr.Sum();

                    var rightMessage = new Fiware.TrafficFlowObserved(newId, dateStr, 1, totalCountRight, refRoad);

                    _fiwareContextBroker.PostNewTrafficFlowObserved(rightMessage); 
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
