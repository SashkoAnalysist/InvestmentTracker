using Microsoft.Extensions.Logging;

namespace MauiApp1.Services;

public class GeolocationService : IGeolocationService
{
    private readonly ILogger<GeolocationService> _logger;

    public GeolocationService(ILogger<GeolocationService> logger)
    {
        _logger = logger;
    }

    public async Task<(double Latitude, double Longitude)?> GetCurrentCoordinatesAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                _logger.LogWarning("Користувач відхилив дозвіл на доступ до геолокації.");
                return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                _logger.LogInformation("Координати отримано: {0}, {1}", location.Latitude, location.Longitude);
                return (location.Latitude, location.Longitude);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка отримання геолокації.");
            return null;
        }
    }
}