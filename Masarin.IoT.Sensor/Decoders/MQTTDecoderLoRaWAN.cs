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
                if (obj.ContainsKey("L0_CNT")) {
                    int[] arr = new int[8] {obj.L0_CNT, obj.L1_CNT, obj.L2_CNT, obj.L3_CNT, obj.R0_CNT, obj.R1_CNT, obj.R2_CNT, obj.R3_CNT};

                    int totalCount = arr.Sum();
                    
                    string stringValue = $"i%3D{totalCount}";
                    
                    var message = new Fiware.DeviceMessage(deviceName, stringValue);

                    _fiwareContextBroker.PostMessage(message); 
                }
                else
                {
                    Console.WriteLine($"No \"L0_CNT\" property in message from deviceName {deviceName}!: {json}");
                    return;
                }  
            }

            Console.WriteLine($"Got message from deviceName {deviceName}: {json}");
        }
    }
}
