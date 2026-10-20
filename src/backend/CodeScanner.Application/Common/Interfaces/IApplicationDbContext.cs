using CodeScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeScanner.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Model> Models { get; }
    DbSet<OfficeMember> OfficeMembers { get; }
    DbSet<Line> Lines { get; }
    DbSet<PrinterModel> PrinterModels { get; }
    DbSet<PrinterModelValue> PrinterModelValues { get; }
    DbSet<Response> Responses { get; }
    DbSet<ResponseSummary> ResponseSummaries { get; }
    DbSet<Setting> Settings { get; }
    DbSet<SettingInfo> SettingInfos { get; }
    DbSet<DailyReport> DailyReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
