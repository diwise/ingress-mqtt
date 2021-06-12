namespace Fiware
{
    public interface IContextBrokerProxy
    {
        public void PostMessage(DeviceMessage message);

        public void PostNewTrafficFlowObserved(TrafficFlowObserved tfo);
    } 
}