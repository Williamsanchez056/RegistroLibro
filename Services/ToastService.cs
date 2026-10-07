namespace RegistroLibro.Services;
public enum ToastType
{
    Info,
    Success,
    Warning,
    Danger
}
public class ToastMessage
{
    public ToastType Type { get; set; }
    public string? Message { get; set; }
}
public class ToastService
{
    public event Action<ToastMessage>? OnNotify;
    public void Notify(ToastMessage message)
    {
        OnNotify?.Invoke(message);
    }
}
