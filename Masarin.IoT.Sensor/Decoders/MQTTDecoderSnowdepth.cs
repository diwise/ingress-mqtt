
using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Text;
using Fiware;

namespace Masarin.IoT.Sensor
{
    public class MQTTDecoderSnowdepth : MQTTDecoder
    {
        private readonly IContextBrokerProxy _fiwareContextBroker = null;
        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderSnowdepth(IMessageQueue messageQueue, IContextBrokerProxy contextBroker)
        {
            _fiwareContextBroker = contextBroker;
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            string json = Encoding.UTF8.GetString(payload);
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            payload = System.Convert.FromBase64String(Convert.ToString(data.data));

            string deviceInHex = device;
            device = Int64.Parse(device, System.Globalization.NumberStyles.HexNumber).ToString();

            ReadOnlySpan<byte> span = payload;
            /*
                uint8_t   Battery [%]
                uint16_t  raw Distance [mm]
                uint16_t  Angle [deg]
                unit16_t  Vertical distance [mm] 
                unit16_t  Snow depth [mm]
                unit16_t  Laser signal strength [0-400] (lower is better) 
                uint8_t   Laser sensor status
                int16_t   Temperature [°C]F
                unit8_t   Humidity [%]
                unit32_t  Pressure [Pa]

                DEVEUI:s
                1199411787624306471 Stöde 62.4081681,16.5687632
                1199411787624306472 Matfors 62.348384, 17.016098
                1199411787624306473 Njurunda 62.310288, 17.369975
                1199411787624306480 Sundsvall 62.392013, 17.285092
                1199411787624306481 Alnö 62.424865, 17.434870
                1199411787624306482 Sidsjö 62.374817, 17.269407
                1199411787624306483 Granloholm 62.409886, 17.270434
                1199411787624306484 Kovland 62.466341, 17.147527
                1199411787624306485 Fagerdalsparken 62.381662, 17.282563
                1199411787624306486 Finsta 62.461594, 17.345016
            */
            double latitude = 1.348364;
            double longitude = 1.016056;

            if (device == "1199411787624306471")
            {
                latitude = 62.4081681;
                longitude = 16.5687632;
            }
            else if (device == "1199411787624306472")
            {
                latitude = 62.348384;
                longitude = 17.016098;
            }
            else if (device == "1199411787624306473")
            {
                latitude = 62.310288;
                longitude = 17.369975;
            }
            else if (device == "1199411787624306480")
            {
                latitude = 62.392013;
                longitude = 17.285092;
            }
            else if (device == "1199411787624306481")
            {
                latitude = 62.424865;
                longitude = 17.434870;
            }
            else if (device == "1199411787624306483")
            {
                latitude = 62.409886;
                longitude = 17.270434;
            }
            else if (device == "1199411787624306482")
            {
                latitude = 62.374817;
                longitude = 17.269407;
            }
            else if (device == "1199411787624306484")
            {
                latitude = 62.466341;
                longitude = 17.147527;
            }
            else if (device == "1199411787624306485")
            {
                latitude = 62.381662;
                longitude = 17.282563;
            }
            else if (device == "1199411787624306486")
            {
                latitude = 62.461594;
                longitude = 17.345016;
            }

            // TODO: We need to decide on the device names. Should we use the name from the LoRa app server?
            string deviceName = "snow_" + deviceInHex;

            IoTHubMessageOrigin origin = new IoTHubMessageOrigin(deviceName, latitude, longitude);

            double volts = payload[0];
            volts = Math.Round(3 * ((volts * 0.005) + 1.1), 3);
            _messageQueue.PostMessage(new SensorStatusMessage(origin, timestamp, volts));

            const byte sensorStatusIsOK = 0;
            if (payload[11] == sensorStatusIsOK)
            {
                DateTime dt = DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ssZ", null);
                if (dt.Month < 5 || dt.Month > 10)
                {
                    double snowdepth = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 7, length: 2));
                    snowdepth = Math.Round(snowdepth / 10.0, 1);
                    _messageQueue.PostMessage(new TelemetrySnowdepth(origin, timestamp, snowdepth));
                }
                else
                {
                    Console.WriteLine($"Snowdepth readings are disabled.");
                }
            }
            else {
                Console.WriteLine($"Ignoring snowdepth reading from {deviceName}. Sensor is not OK.");
            }

            double temperature = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 12, length: 2));
            temperature = Math.Round((temperature / 10.0) - 100.0, 2);
            _messageQueue.PostMessage(new TelemetryTemperature(origin, timestamp, temperature));

            int humidity = payload[14];
            _messageQueue.PostMessage(new TelemetryHumidity(origin, timestamp, humidity));

            double pressure = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(start: 15, length: 4));
            _messageQueue.PostMessage(new TelemetryPressure(origin, timestamp, (int) pressure));

            if (data.ContainsKey("rxInfo"))
            {
                dynamic rxInfo = data["rxInfo"];
                if (rxInfo.Count > 0)
                {
                    dynamic gateway = rxInfo[0];
                    int maxRSSI = gateway.rssi;
                    int snr = gateway.loRaSNR;
                    string name = gateway.name;

                    for (int i = 1; i < rxInfo.Count; i++)
                    {
                        if (rxInfo[i].rssi > maxRSSI)
                        {
                            maxRSSI = rxInfo[i].rssi;
                            snr = gateway.loRaSNR;
                            name = rxInfo[i].name;
                        }
                    }

                    Console.WriteLine($"{deviceName} is connected to gateway {name} with rssi {maxRSSI} and snr {snr}");

                    double rssiLevel = Math.Round((125.0 - Math.Abs(maxRSSI)) / 100.0, 2);
                    rssiLevel = Math.Min(Math.Max(0, rssiLevel), 1.0); // RSSI level should be in range [0 1]

                    double snrLevel = Math.Round((snr + 20.0) / 32.0, 2);
                    snrLevel = Math.Min(Math.Max(0, snrLevel), 1.0);

                    deviceName = "se:servanet:lora:" + deviceName;

                    var msg = new Fiware.DeviceMessage(deviceName).WithRSSI(rssiLevel).WithSNR(snrLevel);
                    msg = msg.WithVoltage(Math.Min(Math.Max(0, volts / 4.925), 1));

                    try
                    {
                        _fiwareContextBroker.PostMessage(msg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception caught attempting to post Device update: {e.Message}");
                    };
                }
            }
        }
    }
}
