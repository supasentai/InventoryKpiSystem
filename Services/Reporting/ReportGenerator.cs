using System;
using System.IO;
using System.Text.Json;
using InventoryKpiSystem.DTOs; // Bắt buộc thêm dòng này để gọi được KpiResult

namespace InventoryKpiSystem.Services.Reporting
{
    public class ReportGenerator
    {
        public void GenerateReport(KpiResult report) // Đã đổi thành KpiResult
        {
            PrintToConsole(report);
            ExportToJson(report);
        }

        private void PrintToConsole(KpiResult report) // Đã đổi thành KpiResult
        {
            Console.WriteLine("=================================");
            Console.WriteLine("           KPI REPORT");
            Console.WriteLine("=================================");
            Console.WriteLine($"Generated At:           {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Total SKUs:             {report.TotalSkus:N0} units");
            Console.WriteLine($"Inventory Value:        ${report.InventoryValue:N2}");
            Console.WriteLine($"Out-of-Stock Items:     {report.OutOfStockItems:N0}");
            Console.WriteLine($"Average Daily Sales:    {report.AverageDailySales:N2} units/day");
            Console.WriteLine($"Average Inventory Age:  {report.AverageInventoryAge:N0} days");
            Console.WriteLine("=================================");
        }

        private void ExportToJson(KpiResult report) // Đã đổi thành KpiResult
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