using System.Collections.Generic;

namespace Masarin.IoT.Sensor.Extensions;

public static class DictionaryExtension
{
    public static void AddOrUpdate(this Dictionary<string, string> dictionary, string deviceName, string state)
    {
        if(!dictionary.TryAdd(deviceName, state))
        {
            dictionary[deviceName] = state;
        }
    }
}
