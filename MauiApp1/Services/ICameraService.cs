namespace MauiApp1.Services;

public interface ICameraService
{
    Task<string?> CaptureReceiptPhotoAsync();
}
