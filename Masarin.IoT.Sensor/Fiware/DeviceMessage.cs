    namespace Fiware
    {
        public class DeviceMessage
        {
            public string Id { get; }
            public string Type { get; }
            public TextProperty Value { get; }

            public NumberPropertyFromDouble BatteryLevel { get; set; }
            public NumberPropertyFromDouble RSSI { get; set; }
            public NumberPropertyFromDouble SNR { get; set; }


            public DeviceMessage(string id)
            {
                Id = "urn:ngsi-ld:Device:" + id;
                Type = "Device";
            }

            public DeviceMessage(string id, string value) : this(id)
            {
                Value = new TextProperty(value);
            }

            public DeviceMessage WithVoltage(double voltage)
            {
                BatteryLevel = new NumberPropertyFromDouble(voltage);
                return this;
            }

            public DeviceMessage WithRSSI(double rssi)
            {
                RSSI = new NumberPropertyFromDouble(rssi);
                return this;
            }

            public DeviceMessage WithSNR(double snr)
            {
                SNR = new NumberPropertyFromDouble(snr);
                return this;
            }
        }
    }
