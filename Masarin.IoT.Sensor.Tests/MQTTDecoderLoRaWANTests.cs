using System;
using System.Text;
using Xunit;
using Moq;
using Fiware;

namespace Masarin.IoT.Sensor.Tests
{
    public class MQTTDecoderLoRaWANTests
    {
        [Fact]
        public void TestThatDeviceMessageIsPostedWithCorrectId()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"sn-elt-livboj-02\",\"devEUI\":\"a81758fffe04d854\",\"data\":\"Bw4yDQA=\",\"object\":{\"present\":true}}";

            decoder.Decode("2020-08-26T07:11:31Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            contextBroker.Verify(foo => foo.PostMessage(It.Is<DeviceMessage>(mo => mo.Id == "urn:ngsi-ld:Device:se:servanet:lora:sn-elt-livboj-02")));
        }

        [Fact]
        public void TestThatDeviceMessageIsPostedWithCorrectValue()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"sk-elt-temp-20\",\"devEUI\":\"a81758fffe04d834\",\"data\":\"Bw45DABu\",\"object\":{\"externalTemperature\":11,\"vdd\":3641},\"tags\":{\"Location\":\"Flasian_south\"}}";

            decoder.Decode("2020-08-26T07:11:31Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            contextBroker.Verify(foo => foo.PostMessage(It.Is<DeviceMessage>(mo => mo.Value.Value == "t%3D11")));
        }

        [Fact]
        public void TestThatTrafficFlowObservedPostsOnlyAsManyTimesAsExpected()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"TRSense01\",\"devEUI\":\"353438396e399112\",\"data\":\"vgICAAAAAAFPAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\",\"object\":{\"L0_AVG\":0,\"L0_CNT\":2,\"L1_AVG\":0,\"L1_CNT\":4,\"L2_AVG\":0,\"L2_CNT\":3,\"L3_AVG\":0,\"L3_CNT\":1,\"R0_AVG\":0,\"R0_CNT\":2,\"R1_AVG\":0,\"R1_CNT\":3,\"R2_AVG\":0,\"R2_CNT\":0,\"R3_AVG\":0,\"R3_CNT\":0,\"SBX_BATT\":0,\"SBX_PV\":0,\"TEMP\":33}}";

            decoder.Decode("2020-08-26T07:11:31Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            contextBroker.Verify(foo => foo.CreateNewEntity(It.IsAny<TrafficFlowObserved>()), Times.Exactly(6));
        }

        [Fact]
        public void TestThatDecoderHandlesNullObjectProperly()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"sk-elt-temp-01\",\"devEUI\":\"xxxxxxxxxxxxxxx\",\"data\":null,\"object\":{},\"tags\":{\"Location\":\"Sidsjobacken\"}}";

            decoder.Decode("2020-10-07T15:46:45Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            contextBroker.Verify(foo => foo.PostMessage(It.IsAny<DeviceMessage>()), Times.Never());
        }

        [Fact]
        public void TestThatErsCo2CanFetchCO2()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"mcg-ers-co2-01\",\"devEUI\":\"xxxxxxxxxxxxxxx\",\"data\":\"AQDlAiUEAO8FAAYBwQcOBQ==\",\"object\":{\"co2\":449,\"humidity\":37,\"light\":239,\"motion\":0,\"temperature\":22.9,\"vdd\":3589}}";
            decoder.Decode("2020-10-07T15:46:45Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            
            contextBroker.Verify(foo => foo.PostMessage(It.IsAny<DeviceMessage>()), Times.Once());
            contextBroker.Verify(foo => foo.CreateNewEntity(It.IsAny<AirQualityObserved>()), Times.Once());
        }

        [Fact]
        public void TestThatWaterTempParsesVDDCorrectly()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"sk-elt-temp-20\",\"devEUI\":\"xxxxxxxxxxxxxxxxxxxx\",\"data\":\"Bw4pDP/8\",\"object\":{\"externalTemperature\":2.4,\"vdd\":3522},\"tags\":{\"Location\":\"UnSet\"}}";
            decoder.Decode("2020-10-07T15:46:45Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            contextBroker.Verify(foo => foo.PostMessage(It.Is<DeviceMessage>(mo => Math.Abs(mo.BatteryLevel.Value - 0.96) < 0.01)));
        }

        [Fact]
        public void TestWaterConsumptionDoesNotSendDeviceMessageOrCreateEntityOnNoStatusCode()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"applicationID\":\"2\",\"applicationName\":\"Watermetering\",\"deviceName\":\"05393901\",\"deviceProfileName\":\"Axioma_Universal_Codec\",\"deviceProfileID\":\"xxxxxxxxxxxxxxx\",\"devEUI\":\"xxxxxxxxxxxxx\"}";
            decoder.Decode("2020-10-07T15:46:45Z", "iothub", "/event/up", Encoding.UTF8.GetBytes(payload));    

            contextBroker.Verify(foo => foo.CreateNewEntity(It.IsAny<WaterConsumptionObserved>()), Times.Never());

            contextBroker.Verify(foo => foo.PostMessage(It.IsAny<DeviceMessage>()), Times.Never());
        }

        [Fact]
        public void TestWaterConsumptionCreatesNewEntityOnStatusAndCurrentVolumeOkay()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"applicationID\":\"2\",\"applicationName\":\"Watermetering\",\"deviceName\":\"05394167\",\"deviceProfileName\":\"Axioma_Universal_Codec\",\"deviceProfileID\":\"xxxxxxxxxxx\",\"devEUI\":\"xxxxxxxxxx\",\"txInfo\":{\"frequency\":867100000,\"dr\":0},\"adr\":true,\"fCnt\":182,\"fPort\":100,\"data\":\"xxxxxxxxxxxxxxxxxxxxxx\",\"object\":{\"curDateTime\":\"2022-02-10 15:13:57\",\"curVol\":1009,\"deltaVol\":{\"id1\":0,\"id10\":13,\"id11\":10,\"id12\":2,\"id13\":0,\"id14\":1,\"id15\":0,\"id16\":5,\"id17\":0,\"id18\":0,\"id19\":0,\"id2\":8,\"id20\":2,\"id21\":0,\"id22\":0,\"id23\":0,\"id3\":0,\"id4\":0,\"id5\":0,\"id6\":0,\"id7\":0,\"id8\":5,\"id9\":6},\"frameVersion\":1,\"statusCode\":0},\"tags\":{\"Location\":\"UnSet\",\"SerialNo\":\"05394167\"}}";
            decoder.Decode("2020-10-07T15:46:45Z", "iothub", "/event/up", Encoding.UTF8.GetBytes(payload));    

            contextBroker.Verify(foo => foo.CreateNewEntity(It.IsAny<WaterConsumptionObserved>()), Times.Once());

            contextBroker.Verify(foo => foo.PostMessage(It.IsAny<DeviceMessage>()), Times.Once());
        }
    }
}
