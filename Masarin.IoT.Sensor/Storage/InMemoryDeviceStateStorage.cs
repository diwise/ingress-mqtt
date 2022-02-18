using System.Collections.Generic;

namespace Storage
{
    public static class InMemoryDeviceStateStorage 
    {
        private static Dictionary<string, string> _deviceStates = new Dictionary<string, string>();
        public static void StoreDeviceState(string deviceName, string state)
        {
            if(!_deviceStates.TryAdd(deviceName, state))
            {
                _deviceStates[deviceName] = state;
            }
        }
        public static string GetDeviceState(string deviceName) 
        {
            var state = string.Empty;
            _deviceStates.TryGetValue(deviceName,out state);

            return state;
        }
    }
}