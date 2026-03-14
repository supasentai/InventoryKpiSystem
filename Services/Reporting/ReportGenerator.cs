using System;
using System.IO;
using System.Text.Json;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Services.Reporting
{
    public class ReportGenerator
    {
        public void GenerateReport(KpiReport report)
        {
            PrintToConsole(report);
            ExportToJson(report);
        }

        private void PrintToConsole(KpiReport report)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("           KPI REPORT");
            Console.WriteLine("=================================");         
            Console.WriteLine($"Total SKUs:             {report.TotalSkus:N0}units");
            Console.WriteLine($"Inventory Value:        ${report.InventoryValue:N2}");
            Console.WriteLine($"Out-of-Stock Items:     {report.OutOfStockItems:N0}");
            Console.WriteLine($"Average Daily Sales:    {report.AverageDailySales:N2} units/day");
            Console.WriteLine($"Average Inventory Age:  {report.AverageInventoryAge:N2} days");           
            Console.WriteLine("=================================");
        }

        private void ExportToJson(KpiReport report)
        {
            var directory = "reports";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = $"kpi-report-{DateTime.Now:yyyyMMddHHmmss}.json";

            var path = Path.Combine(directory, fileName);

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);

            Console.WriteLine($"Report exported to: {path}");
        }
    }
}