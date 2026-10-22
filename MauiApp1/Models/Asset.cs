using SQLite;

namespace MauiApp1.Models;

[Table("Assets")]
public class Asset
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal CurrentPrice { get; set; }

    // Обчислювана властивість для відображення в UI
    [SQLite.Ignore]
    public decimal TotalValue => Amount * CurrentPrice;

    // Лабораторна №6: Апаратна інтеграція (Камера та GPS)
    public string? ReceiptImagePath { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}