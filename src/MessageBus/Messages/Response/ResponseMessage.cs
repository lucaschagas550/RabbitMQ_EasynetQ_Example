namespace MessageBus.Messages.Response
{
    public class ResponseMessage
    {
        public bool IsSuccess { get; set; }
        public object? Data { get; set; }

        public ResponseMessage(bool isSuccess, object? data)
        {
            IsSuccess = isSuccess;
            Data = data;
        }
    }
}
