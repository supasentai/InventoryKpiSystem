using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Application.Services;

public class FifoCostingService : IFifoCostingService
{
    public void ApplySale(InventoryItem item, int quantity)
    {
        var remainingToDeduct = quantity;
        var availableBatches = item.PurchaseBatches
            .Where(batch => batch.RemainingQuantity > 0)
            .OrderBy(batch => batch.PurchaseDate)
            .ToList();

        foreach (var batch in availableBatches)
        {
            if (remainingToDeduct <= 0)
            {
                break;
            }

            if (batch.RemainingQuantity >= remainingToDeduct)
            {
                batch.RemainingQuantity -= remainingToDeduct;
                remainingToDeduct = 0;
            }
            else
            {
                remainingToDeduct -= batch.RemainingQuantity;
                batch.RemainingQuantity = 0;
            }
        }
    }
}
