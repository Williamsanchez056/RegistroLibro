using RegistroLibro.Services;

namespace RegistroLibro.Extensors;

public static class ToastServiceExtensions
{
    public static void ShowSuccess(this ToastService toastService, string mensaje)
    {
        toastService.Notify(new ToastMessage
        {
            Type = ToastType.Success,
            Message = mensaje
        });
    }

    public static void ShowError(this ToastService toastService, string mensaje)
    {
        toastService.Notify(new ToastMessage
        {
            Type = ToastType.Danger,
            Message = mensaje
        });
    }
}