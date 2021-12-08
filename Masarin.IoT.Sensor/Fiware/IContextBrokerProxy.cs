namespace Fiware
{
    public interface IContextBrokerProxy
    {
        public void PostMessage(DeviceMessage message);

        public void CreateNewEntity(object entity);
    } 
}
