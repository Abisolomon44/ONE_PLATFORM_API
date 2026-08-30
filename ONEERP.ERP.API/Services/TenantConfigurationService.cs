using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface ITenantConfigurationService
{
    Task<List<TenantConfigurationDto>> GetAllAsync(long tenantId);
    Task<List<TenantConfigApplicationDto>> GetGroupedAsync(long tenantId);
    Task<TenantConfigurationDto?> GetByIdAsync(long id);
    Task<TenantConfigurationDto> CreateAsync(long tenantId, long userId, CreateTenantConfigurationRequest request);
    Task<TenantConfigurationDto> UpdateAsync(long id, long userId, UpdateTenantConfigurationRequest request);
    Task DeleteAsync(long id);
}

public class TenantConfigurationService : ITenantConfigurationService
{
    private readonly ITenantConfigurationRepository _repo;

    public TenantConfigurationService(ITenantConfigurationRepository repo)
    {
        _repo = repo;
    }

    private static TenantConfigurationDto ToDto(TenantConfiguration e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ApplicationType = e.ApplicationType,
        TransactionType = e.TransactionType,
        FlowType = e.FlowType,
        PageCode = e.PageCode,
        FieldCode = e.FieldCode,
        SequenceNo = e.SequenceNo,
        IsPageEnabled = e.IsPageEnabled,
        IsVisible = e.IsVisible,
        IsRequired = e.IsRequired,
        IsReadonly = e.IsReadonly,
        DisplayOrder = e.DisplayOrder,
        DefaultValue = e.DefaultValue,
        IsActive = e.IsActive,
    };

    public async Task<List<TenantConfigurationDto>> GetAllAsync(long tenantId)
        => (await _repo.GetByTenantAsync(tenantId)).Select(ToDto).ToList();

    public async Task<List<TenantConfigApplicationDto>> GetGroupedAsync(long tenantId)
    {
        var rows = await GetAllAsync(tenantId);
        var apps = new Dictionary<string, TenantConfigApplicationDto>();
        foreach (var r in rows)
        {
            if (!apps.TryGetValue(r.ApplicationType, out var app))
            {
                app = new TenantConfigApplicationDto { ApplicationType = r.ApplicationType };
                apps[r.ApplicationType] = app;
            }
            var tx = app.Transactions.FirstOrDefault(x => x.TransactionType == r.TransactionType);
            if (tx == null)
            {
                tx = new TenantConfigTransactionDto { TransactionType = r.TransactionType };
                app.Transactions.Add(tx);
            }
            var flow = tx.Flows.FirstOrDefault(x => x.FlowType == r.FlowType);
            if (flow == null)
            {
                flow = new TenantConfigFlowDto { FlowType = r.FlowType };
                tx.Flows.Add(flow);
            }
            var page = flow.Pages.FirstOrDefault(x => x.PageCode == r.PageCode);
            if (page == null)
            {
                page = new TenantConfigPageDto { PageCode = r.PageCode };
                flow.Pages.Add(page);
            }
            page.Fields.Add(r);
        }
        return apps.Values.ToList();
    }

    public async Task<TenantConfigurationDto?> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? null : ToDto(e);
    }

    public async Task<TenantConfigurationDto> CreateAsync(long tenantId, long userId, CreateTenantConfigurationRequest request)
    {
        var entity = new TenantConfiguration
        {
            TenantId = tenantId,
            ApplicationType = request.ApplicationType.Trim(),
            TransactionType = request.TransactionType?.Trim(),
            FlowType = request.FlowType?.Trim(),
            PageCode = request.PageCode?.Trim(),
            FieldCode = request.FieldCode?.Trim(),
            SequenceNo = request.SequenceNo,
            IsPageEnabled = request.IsPageEnabled,
            IsVisible = request.IsVisible,
            IsRequired = request.IsRequired,
            IsReadonly = request.IsReadonly,
            DisplayOrder = request.DisplayOrder,
            DefaultValue = request.DefaultValue?.Trim(),
            IsActive = request.IsActive,
            CreatedBy = userId,
        };
        entity.Id = await _repo.InsertAsync(entity);
        return ToDto(entity);
    }

    public async Task<TenantConfigurationDto> UpdateAsync(long id, long userId, UpdateTenantConfigurationRequest request)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tenant configuration '{id}' was not found.");
        entity.ApplicationType = request.ApplicationType.Trim();
        entity.TransactionType = request.TransactionType?.Trim();
        entity.FlowType = request.FlowType?.Trim();
        entity.PageCode = request.PageCode?.Trim();
        entity.FieldCode = request.FieldCode?.Trim();
        entity.SequenceNo = request.SequenceNo;
        entity.IsPageEnabled = request.IsPageEnabled;
        entity.IsVisible = request.IsVisible;
        entity.IsRequired = request.IsRequired;
        entity.IsReadonly = request.IsReadonly;
        entity.DisplayOrder = request.DisplayOrder;
        entity.DefaultValue = request.DefaultValue?.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = userId;
        await _repo.UpdateAsync(entity);
        return ToDto(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tenant configuration '{id}' was not found.");
        await _repo.DeleteAsync(id);
    }
}
