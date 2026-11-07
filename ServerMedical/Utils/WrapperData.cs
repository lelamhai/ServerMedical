namespace ServerMedical.Utils
{
    public class WrapperData<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
