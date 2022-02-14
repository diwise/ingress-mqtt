using Newtonsoft.Json;
using System;
using System.Text;
using Fiware;
using Storage;

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
            var dateStrNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            dynamic obj = null;

            if (data.ContainsKey("object"))
            {
                obj = data["object"];
            }
            
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

                    var message = new DeviceMessage(deviceName, value);

                    try {
                        _fiwareContextBroker.PostMessage(message);
                    }
                    catch (Exception e) 
                    {
                        Console.WriteLine($"Exception caught attempting to post Device update: {e.Message}");
                    };
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
                    var message = new DeviceMessage(deviceName, stringValue);

                    if (obj.ContainsKey("vdd"))
                    {
                        double batteryLevel = obj.vdd;
                        const double MAX_BATTERY_LEVEL = 3665;

                        if (batteryLevel > MAX_BATTERY_LEVEL)
                        {
                            Console.WriteLine($"Battery level of {deviceName} is bigger than the max level. {batteryLevel} > {MAX_BATTERY_LEVEL}!");
                        }

                        batteryLevel = batteryLevel / MAX_BATTERY_LEVEL;
                        message = message.WithVoltage(Math.Round(batteryLevel, 2));
                    }

                    try
                    {
                        _fiwareContextBroker.PostMessage(message);
                    }
                    catch (Exception e) 
                    {
                        Console.WriteLine($"Exception caught attempting to post Device update: {e.Message}");
                    };
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

                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 0, (int)obj.L0_CNT, (double)obj.L0_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 1, (int)obj.L1_CNT, (double)obj.L1_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 2, (int)obj.L2_CNT, (double)obj.L2_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 3, (int)obj.L3_CNT, (double)obj.L3_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 4, (int)obj.R0_CNT, (double)obj.R0_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 5, (int)obj.R1_CNT, (double)obj.R1_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 6, (int)obj.R2_CNT, (double)obj.R2_AVG);
                    ReportTrafficIntensityForLane(shortDeviceName, dateStrNow, 7, (int)obj.R3_CNT, (double)obj.R3_AVG);
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
                    double co2 = obj.co2;
                    var aqoMsg = new AirQualityObserved(deviceName, dateStrNow);
                    aqoMsg = aqoMsg.WithCO2(co2);

                    if (obj.ContainsKey("temperature")) 
                    {
                        double temp = obj.temperature;
                        aqoMsg = aqoMsg.WithTemperature(temp);
                    }

                    if (obj.ContainsKey("humidity")) 
                    {
                        double humidity = obj.humidity/100.0;
                        aqoMsg = aqoMsg.WithHumidity(humidity);
                    }

                    var deviceMsg = new DeviceMessage(deviceName);

                    if (obj.ContainsKey("vdd"))
                    {
                        double batteryLevel = obj.vdd;
                        batteryLevel = batteryLevel / 3665.0;
                        deviceMsg = deviceMsg.WithVoltage(Math.Round(batteryLevel, 2));
                    }

                    try
                    {
                        _fiwareContextBroker.CreateNewEntity(aqoMsg);
                        _fiwareContextBroker.PostMessage(deviceMsg);  
                    }
                    catch (Exception e) 
                    {
                        Console.WriteLine($"Exception caught attempting to post Device update: {e.Message}");
                    };
                }
                else
                {
                    Console.WriteLine($"No \"co2\" property in message from deviceName {deviceName}!: {json}");
                    return;
                }
            }
            else if (data.ContainsKey("applicationName") && (data.applicationName == "Watermetering" || data.applicationName == "Soraker" || data.applicationName == "Bergsaker"))
            {
                deviceName = "se:servanet:lora:msva:" + Convert.ToString(data.deviceName);

                if (topic == "/event/up")
                {
                    if (obj != null && obj.ContainsKey("statusCode"))
                    {
                        int curVol = 0;

                        if (obj.ContainsKey("curVol"))
                        {
                            curVol = obj.curVol;
                            if (curVol >= 0) {
                                var entity = new WaterConsumptionObserved(deviceName + ":" + dateStrNow, deviceName, dateStrNow, curVol);

                                try
                                {
                                    _fiwareContextBroker.CreateNewEntity(entity);
                                }
                                catch (Exception e) 
                                {
                                    Console.WriteLine($"Exception caught attempting to post WaterConsumptionObserved: {e.Message}");
                                };

                                int statusCode = obj.statusCode;
                                string stringStatus = statusCode.ToString();
                                string previousStatus = InMemoryDeviceStateStorage.GetDeviceState(deviceName);
                                
                                if (previousStatus == string.Empty)
                                {
                                    InMemoryDeviceStateStorage.StoreDeviceState(deviceName, stringStatus);
                                } else if (previousStatus != stringStatus) {
                                    var msg = new DeviceMessage(deviceName).WithDeviceState(stringStatus);

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
                            else
                            {
                                Console.WriteLine($"Ignoring negative current volume: {curVol}");
                            }
                        }
                    }

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

                            var msg = new DeviceMessage(deviceName).WithRSSI(rssiLevel).WithSNR(snrLevel);
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
                else if (topic == "/event/status")
                {
                    if (data.externalPowerSource == false && data.batteryLevelUnavailable == false)
                    {
                        double batteryLevel = data.batteryLevel / 100.0;
                        var msg = new DeviceMessage(deviceName).WithVoltage(Math.Round(batteryLevel, 2));
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

            Console.WriteLine($"Got message from {deviceName} on topic {topic}: {json}");
        }

        private void ReportTrafficIntensityForLane(string deviceName, string dateStr, int lane, int intensity, double averageSpeed) {

            string refRoad = "urn:ngsi-ld:RoadSegment:19312:2860:35243";
            string shortDeviceName = $"{deviceName}:{lane}:{dateStr}";
            
            if (intensity > 0) {
                var tfo = new TrafficFlowObserved(shortDeviceName, dateStr, lane, intensity, refRoad);
                tfo.AverageVehicleSpeed = new NumberPropertyFromDouble(averageSpeed);

                try {
                    _fiwareContextBroker.CreateNewEntity(tfo);
                } catch (Exception e) 
                {
                    Console.WriteLine($"Exception caught attempting to post TrafficFlowObserved: {e.Message}");
                };
            }
        }
    }
}
