using Microsoft.Extensions.Logging;

namespace MauiApp1.Services;

public class CameraService : ICameraService
{
    private readonly ILogger<CameraService> _logger;

    public CameraService(ILogger<CameraService> logger)
    {
        _logger = logger;
    }

    public async Task<string?> CaptureReceiptPhotoAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                _logger.LogWarning("Користувач відхилив дозвіл на використання камери.");
                return null;
            }

            if (!MediaPicker.Default.IsCaptureSupported)
            {
                _logger.LogWarning("Камера не підтримується на цьому пристрої.");
                return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return null;

            var localFilePath = Path.Combine(FileSystem.AppDataDirectory, string.Format("{0}_{1}", Guid.NewGuid(), photo.FileName));
            using var sourceStream = await photo.OpenReadAsync();
            using var localFileStream = File.Create(localFilePath);
            await sourceStream.CopyToAsync(localFileStream);

            _logger.LogInformation("Фото збережено: {0}", localFilePath);
            return localFilePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час зйомки камерою.");
            return null;
        }
    }
}