using Inventory.Application.DTOs;

namespace Inventory.Application.Interfaces;

public interface IReportWriter
{
    string Write(KpiResult report);
}
