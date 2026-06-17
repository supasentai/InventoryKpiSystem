using System.Text.Json;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;

namespace Inventory.Infrastructure.Reporting;

public class JsonReportWriter : IReportWriter
{
    private readonly string _directory;

    public JsonReportWriter(string directory = "reports")
    {
        _directory = directory;
    }

    public string Write(KpiResult report)
    {
        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }

        var fileName = $"kpi-report-{DateTime.Now:yyyyMMddHHmmss}.json";
        var path = Path.Combine(_directory, fileName);
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(path, json);
        return path;
    }
}
