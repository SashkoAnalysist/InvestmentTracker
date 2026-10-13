namespace MauiApp1.Models;

public class Asset
{
    public string Name { get; set; } = string.Empty;       // Назва активу (наприклад, Bitcoin, Apple)
    public string Symbol { get; set; } = string.Empty;     // Тікер (BTC, AAPL, USD)
    public decimal Amount { get; set; }                    // Кількість у портфелі
    public decimal CurrentPrice { get; set; }              // Ціна за одиницю

    public decimal TotalValue => Amount * CurrentPrice;    // Загальна вартість
}
