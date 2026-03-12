using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Reporting;

var report = new KpiReport
{
    TotalSkus = 120,
    InventoryValue = 50000,
    OutOfStockItems = 5,
    AverageDailySales = 30,
    AverageInventoryAge = 12
};

var generator = new ReportGenerator();
generator.GenerateReport(report);