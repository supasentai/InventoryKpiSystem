using System;
using System.Collections.Generic;

namespace InventoryKpiSystem.DTOs
{
    public class KpiResult
    {
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public int TotalSkus { get; set; }
        public int OutOfStockItems { get; set; }
        public double AverageDailySales { get; set; }
        public double AverageInventoryAge { get; set; }
        public decimal InventoryValue { get; set; }

        // Danh sách KPI chi tiết của từng sản phẩm (tùy chọn xuất ra báo cáo)
        public List<ProductKpiResult> TopProducts { get; set; } = new List<ProductKpiResult>();
    }
}