using System;
using System.Text;
using Xunit;
using Moq;
using Fiware;

namespace Masarin.IoT.Sensor.Tests
{
    public class MQTTDecoderSnowdepthTests
    {
        [Fact]
        public void IfSummer_SnowdepthShouldNotSend()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object, contextBroker.Object);
            
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };
            
            decoder.Decode("2020-08-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Never());
        }
        
        
        [Fact]
        public void IfNotSummer_SnowdepthShouldSend()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object, contextBroker.Object);
            
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };

            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Once());
        }

        [Fact]
        public void IfDate_IsWrongFormatThenExceptionShouldBeThrown()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object, contextBroker.Object);
            
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };

            Assert.Throws<FormatException>(() => decoder.Decode("gurka", "ff", "", CreateTestPayload(bytes)));
        }

        [Fact]
        public void IfSensor_IsNotOkaySnowdepthShouldNotSend()
        {   
            var contextBroker = new Mock<IContextBrokerProxy>();
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object, contextBroker.Object);

            //The byte array is needed to form the payload that PostMessage sends. Without it, the test does not work.
            //The 11th byte (11), should be 0 if the sensor is okay (see previous tests).
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Never());
        }

        [Fact]
        public void IfSnowdepth_ReceivedMatchesExpectedSentOutcome()
        {
            var contextBroker = new Mock<IContextBrokerProxy>();
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object, contextBroker.Object);
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 1, 244, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };

            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(ms => ms.PostMessage( 
                It.Is<TelemetrySnowdepth>(mo => mo.Depth == 50)
            ), Times.Once());
        }

        private static byte[] CreateTestPayload(byte[] bytes)
        {
            var data = Convert.ToBase64String(bytes);

            var payload = "{\"applicationID\":\"5\",\"applicationName\":\"exempel-app\",\"deviceName\":\"SnowDepth_10a52aaa84c35735\",\"deviceProfileName\":\"exempel\",\"deviceProfileID\":\"device-profile-id-in-hex\",\"devEUI\":\"10a52aaa84c35735\",\"rxInfo\":[{\"gatewayID\":\"somegwid\",\"uplinkID\":\"some-uplink-id\",\"name\":\"SN-LGW-047\",\"time\":\"2022-01-19T22:33:55.410084369Z\",\"rssi\":-118,\"loRaSNR\":-3.2,\"location\":{\"latitude\":62.2,\"longitude\":17.2,\"altitude\":0}}],\"txInfo\":{\"frequency\":867100000,\"dr\":5},\"adr\":false,\"fCnt\":23598,\"fPort\":1,\"data\":\"" + data + "\",\"object\":{\"payload\":\"" + data + "\"}}";

            return Encoding.UTF8.GetBytes(payload);
        }
    }
}
