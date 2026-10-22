namespace MauiApp1.Services;

public interface IGeolocationService
{
    Task<(double Latitude, double Longitude)?> GetCurrentCoordinatesAsync();
}
