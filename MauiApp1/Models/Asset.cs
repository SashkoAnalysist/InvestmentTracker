using SQLite;

namespace MauiApp1.Models;

[Table("Assets")]
public class Asset
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    [NotNull, MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal CurrentPrice { get; set; }

    [Ignore]
    public decimal TotalValue => Amount * CurrentPrice;
}