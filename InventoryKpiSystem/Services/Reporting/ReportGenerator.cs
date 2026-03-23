using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using InventoryKpiSystem.DTOs;
using InventoryKpiSystem.Models;
using InventoryKpiSystem.Services.Inventory;
using InventoryKpiSystem.Services.KPI;

namespace InventoryKpiSystem.Services.Reporting
{
    public class ReportGenerator
    {
        private readonly InventoryState _state;
        private readonly KpiEngine _kpiEngine;

        public ReportGenerator(InventoryState state, KpiEngine kpiEngine)
        {
            _state = state;
            _kpiEngine = kpiEngine;
        }

        public void RunInteractiveMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=============================");
                Console.WriteLine("    MENU QUẢN LÝ KHO HÀNG");
                Console.WriteLine("=============================");
                Console.WriteLine("1. Xem báo cáo tổng quan KPI (Kèm Xuất File JSON)");
                Console.WriteLine("2. Tra cứu chi tiết từng sản phẩm");
                Console.WriteLine("3. Top 10 sản phẩm có giá trị tồn kho cao nhất");
                Console.WriteLine("0. Thoát chương trình");
                Console.Write("Chọn chức năng (0-3): ");

                var choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1":
                        var allItems = _state.GetAllInventory();

                        Console.WriteLine($"\n--- BÁO CÁO TỔNG QUAN ---");
                        Console.WriteLine($"- Tổng số SKUs: {_kpiEngine.GetTotalSkus(allItems)}");
                        Console.WriteLine($"- Tổng giá trị tồn kho: {_kpiEngine.GetStockValue(allItems):C}");
                        Console.WriteLine($"- Số mặt hàng hết kho: {_kpiEngine.GetOutOfStockItems(allItems)}");
                        Console.WriteLine($"- Tốc độ bán trung bình: {_kpiEngine.GetAverageDailySales(allItems):F2} sp/ngày");
                        Console.WriteLine($"- Tuổi thọ tồn kho trung bình: {_kpiEngine.GetAverageInventoryAge(allItems):F2} ngày");

                        var report = new KpiResult
                        {
                            GeneratedAt = DateTime.Now,
                            TotalSkus = _kpiEngine.GetTotalSkus(allItems),
                            InventoryValue = _kpiEngine.GetStockValue(allItems),
                            OutOfStockItems = _kpiEngine.GetOutOfStockItems(allItems),
                            AverageDailySales = _kpiEngine.GetAverageDailySales(allItems),
                            AverageInventoryAge = _kpiEngine.GetAverageInventoryAge(allItems)
                        };

                        ExportToJson(report);
                        break;

                    case "2":
                        Console.Write("\nNhập mã sản phẩm (ItemCode) cần tra cứu: ");
                        var searchCode = Console.ReadLine()?.Trim();

                        if (string.IsNullOrEmpty(searchCode))
                        {
                            Console.WriteLine("[Lỗi] Mã sản phẩm không được để trống!");
                            break;
                        }

                        if (_state.Products.TryGetValue(searchCode, out var product))
                        {
                            var singleItemList = new List<ProductInventory> { product };
                            double age = _kpiEngine.GetAverageInventoryAge(singleItemList);

                            decimal unitValue = product.QuantityOnHand > 0
                                ? product.TotalStockValue / product.QuantityOnHand
                                : 0;

                            Console.WriteLine($"\n--- CHI TIẾT SẢN PHẨM: {searchCode} ---");
                            Console.WriteLine($"- Tên sản phẩm    : {(string.IsNullOrEmpty(product.Name) ? "[Chưa cập nhật tên]" : product.Name)}");
                            Console.WriteLine($"- Mã sản phẩm     : {product.ItemCode}");
                            Console.WriteLine($"- Đơn giá (Giá trị): {unitValue:C}");
                            Console.WriteLine($"- Tồn kho         : {product.QuantityOnHand} cái");
                            Console.WriteLine($"- Giá trị tồn kho : {product.TotalStockValue:C}");
                            Console.WriteLine($"- Tuổi thọ kho    : {age:F2} ngày");
                        }
                        else
                        {
                            Console.WriteLine($"\n[Hệ thống] Không tìm thấy sản phẩm nào có mã '{searchCode}'.");
                        }
                        break;

                    case "3":
                        Console.WriteLine("\n=== TOP 10 SẢN PHẨM CÓ GIÁ TRỊ TỒN KHO CAO NHẤT ===");

                        var top10Products = _state.Products.Values
                            .Where(p => p.QuantityOnHand > 0 && p.TotalStockValue > 0)
                            .OrderByDescending(p => p.TotalStockValue)
                            .Take(10)
                            .ToList();

                        if (!top10Products.Any())
                        {
                            Console.WriteLine("Kho hàng hiện đang trống hoặc không có sản phẩm nào có giá trị tồn kho.");
                            break;
                        }

                        int rank = 1;
                        foreach (var topProduct in top10Products)
                        {
                            var singleItemList = new List<ProductInventory> { topProduct };
                            double age = _kpiEngine.GetAverageInventoryAge(singleItemList);
                            decimal unitValue = topProduct.TotalStockValue / topProduct.QuantityOnHand;

                            Console.WriteLine($"\n[{rank}] Mã SP: {topProduct.ItemCode} | Tên: {(string.IsNullOrEmpty(topProduct.Name) ? "[Chưa cập nhật tên]" : topProduct.Name)}");
                            Console.WriteLine($"    - Tồn kho      : {topProduct.QuantityOnHand:N0} cái");
                            Console.WriteLine($"    - Đơn giá      : {unitValue:C}");
                            Console.WriteLine($"    - Giá trị tồn  : {topProduct.TotalStockValue:C}");
                            Console.WriteLine($"    - Tuổi thọ kho : {age:F2} ngày");

                            rank++;
                        }
                        break;

                    case "0":
                        Console.WriteLine("\nĐang đóng hệ thống và sao lưu dữ liệu. Tạm biệt!");
                        return;

                    default:
                        Console.WriteLine("\nLựa chọn không hợp lệ, vui lòng gõ lại!");
                        break;
                }
            }
        }

        // ==========================================
        // HÀM XUẤT FILE JSON TỰ ĐỘNG
        // ==========================================
        private void ExportToJson(KpiResult report)
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
            Console.WriteLine($"\n[Thành công] Đã xuất báo cáo ra file: {path}");
        }
    }
}