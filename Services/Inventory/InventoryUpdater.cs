using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.Inventory
{
    public class InventoryUpdater
    {
        private readonly InventoryState _state;

        public InventoryUpdater(InventoryState state)
        {
            _state = state;
        }

        public void ProcessInvoices(List<Invoice> invoices)
        {
            foreach (var invoice in invoices)
            {
                foreach (var line in invoice.LineItems)
                {
                    if (!_state.Products.ContainsKey(line.ItemCode))
                        continue;

                    if (invoice.Type == "ACCPAY")
                        _state.Products[line.ItemCode].QuantityOnHand += line.Quantity;

                    if (invoice.Type == "ACCREC")
                        _state.Products[line.ItemCode].QuantityOnHand -= line.Quantity;
                }
            }
        }
    }
}
