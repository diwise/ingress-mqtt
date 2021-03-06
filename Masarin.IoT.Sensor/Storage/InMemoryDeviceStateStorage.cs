using System.Collections.Generic;
using Masarin.IoT.Sensor.Extensions;

namespace Storage
{
    public static class InMemoryDeviceStateStorage 
    {
        private static Dictionary<string, string> _deviceStates = new Dictionary<string, string>();
        public static void StoreDeviceState(string deviceName, string state)
        {
            _deviceStates.AddOrUpdate(deviceName, state);
        }
        public static string GetDeviceState(string deviceName) 
        {
            var state = string.Empty;
            _deviceStates.TryGetValue(deviceName,out state);

            return state;
        }
    }
}
