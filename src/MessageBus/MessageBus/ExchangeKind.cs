namespace MessageBus.MessageBus
{
    public enum ExchangeKind
    {
        direct = 0,
        topic = 1,
        fanout = 2,
        headers = 3,
    }
}
