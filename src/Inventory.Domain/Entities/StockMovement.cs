using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class StockMovement
{
    public string ItemCode { get; set; } = string.Empty;

    public StockMovementType Type { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public DateTime MovementDate { get; set; }
}
