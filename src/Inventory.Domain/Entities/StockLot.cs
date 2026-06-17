namespace Inventory.Domain.Entities;

public class StockLot
{
    public DateTime PurchaseDate { get; set; }

    public decimal UnitCost { get; set; }

    public int InitialQuantity { get; set; }

    public int RemainingQuantity { get; set; }

    public double GetAgeInDays(DateTime currentDate)
    {
        return (currentDate - PurchaseDate).TotalDays;
    }
}
