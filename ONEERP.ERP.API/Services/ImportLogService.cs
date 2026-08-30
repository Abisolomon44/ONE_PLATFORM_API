using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;

namespace ONEERP.ERP.API.Services;

public interface IImportLogService
{
    Task<long> CreateAsync(ImportLog log);
    Task<IEnumerable<ImportLogDto>> GetAllAsync(int page = 1, int pageSize = 50);
}

public class ImportLogService : IImportLogService
{
    private readonly IImportLogRepository _repository;

    public ImportLogService(IImportLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> CreateAsync(ImportLog log)
        => await _repository.InsertAsync(log);

    public async Task<IEnumerable<ImportLogDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var logs = await _repository.GetAllAsync(page, pageSize);
        return logs.Select(l => new ImportLogDto
        {
            Id = l.Id,
            CompanyId = l.CompanyId,
            BranchId = l.BranchId,
            ImportType = l.ImportType,
            ModuleName = l.ModuleName,
            EntityName = l.EntityName,
            FileName = l.FileName,
            FileType = l.FileType,
            TotalRows = l.TotalRows,
            SuccessRows = l.SuccessRows,
            FailedRows = l.FailedRows,
            Status = l.Status,
            ErrorMessage = l.ErrorMessage,
            ImportedBy = l.ImportedBy,
            ImportedAt = l.ImportedAt
        });
    }
}
