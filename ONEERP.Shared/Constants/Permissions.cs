namespace ONEERP.Shared.Constants;

/// <summary>
/// Permission codes used for permission-based authorization.
/// Stored in the RolePermissions table per tenant and emitted as JWT claims.
/// </summary>
public static class Permissions
{
    public const string DashboardView = "dashboard.view";

    public const string CompaniesView = "companies.view";
    public const string CompaniesCreate = "companies.create";
    public const string CompaniesEdit = "companies.edit";

    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit = "users.edit";
    public const string UsersDelete = "users.delete";

    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";

    public const string BusinessTypesView = "business-types.view";
    public const string BusinessTypesManage = "business-types.manage";

    public const string IndustryTypesView = "industry-types.view";
    public const string IndustryTypesManage = "industry-types.manage";

    public const string CompanyGroupsView = "company-groups.view";
    public const string CompanyGroupsManage = "company-groups.manage";

    public const string LocationsView = "locations.view";
    public const string LocationsCreate = "locations.create";
    public const string LocationsEdit = "locations.edit";
    public const string LocationsDelete = "locations.delete";

    public const string LanguagesView = "languages.view";
    public const string LanguagesManage = "languages.manage";

    public const string TimeZonesView = "timezones.view";
    public const string TimeZonesManage = "timezones.manage";

    public const string GstRegistrationTypesView = "gst-registration-types.view";
    public const string GstRegistrationTypesManage = "gst-registration-types.manage";

    public const string AddressTypesView = "address-types.view";
    public const string AddressTypesManage = "address-types.manage";

    public const string ContactTypesView = "contact-types.view";
    public const string ContactTypesManage = "contact-types.manage";

    public const string DocumentTypesView = "document-types.view";
    public const string DocumentTypesManage = "document-types.manage";

public const string OrganizationTypesView = "organization-types.view";
    public const string OrganizationTypesManage = "organization-types.manage";

    public const string BranchTypesView = "branch-types.view";
    public const string BranchTypesManage = "branch-types.manage";

    public const string WarehouseTypesView = "warehouse-types.view";
    public const string WarehouseTypesManage = "warehouse-types.manage";

    public const string EmploymentTypesView = "employment-types.view";
    public const string EmploymentTypesManage = "employment-types.manage";

    public const string BranchesView = "branches.view";
    public const string BranchesCreate = "branches.create";
    public const string BranchesEdit = "branches.edit";
    public const string BranchesDelete = "branches.delete";

    public const string DepartmentsView = "departments.view";
    public const string DepartmentsCreate = "departments.create";
    public const string DepartmentsEdit = "departments.edit";
    public const string DepartmentsDelete = "departments.delete";

    public const string DesignationsView = "designations.view";
    public const string DesignationsCreate = "designations.create";
    public const string DesignationsEdit = "designations.edit";
    public const string DesignationsDelete = "designations.delete";

    public const string EmployeesView = "employees.view";
    public const string EmployeesCreate = "employees.create";
    public const string EmployeesEdit = "employees.edit";
    public const string EmployeesDelete = "employees.delete";

    public const string WarehousesView = "warehouses.view";
    public const string WarehousesCreate = "warehouses.create";
    public const string WarehousesEdit = "warehouses.edit";
    public const string WarehousesDelete = "warehouses.delete";

    public const string CurrenciesView = "currencies.view";
    public const string CurrenciesManage = "currencies.manage";

    public const string SettingsView = "settings.view";
    public const string SettingsEdit = "settings.edit";

    public const string AuditView = "audit.view";
    public const string ProfileEdit = "profile.edit";

    public const string PermissionModulesView = "permission-modules.view";
    public const string PermissionModulesManage = "permission-modules.manage";

    public const string PermissionActionsView = "permission-actions.view";
    public const string PermissionActionsManage = "permission-actions.manage";

    public static readonly string[] All =
    {
        DashboardView,
        CompaniesView, CompaniesCreate, CompaniesEdit,
        UsersView, UsersCreate, UsersEdit, UsersDelete,
        RolesView, RolesManage,
        BusinessTypesView, BusinessTypesManage,
        IndustryTypesView, IndustryTypesManage,
        CompanyGroupsView, CompanyGroupsManage,
        LocationsView, LocationsCreate, LocationsEdit, LocationsDelete,
        LanguagesView, LanguagesManage,
        TimeZonesView, TimeZonesManage,
        GstRegistrationTypesView, GstRegistrationTypesManage,
        AddressTypesView, AddressTypesManage,
        ContactTypesView, ContactTypesManage,
        DocumentTypesView, DocumentTypesManage,
         OrganizationTypesView, OrganizationTypesManage,
        BranchTypesView, BranchTypesManage,
        WarehouseTypesView, WarehouseTypesManage,
        EmploymentTypesView, EmploymentTypesManage,
        BranchesView, BranchesCreate, BranchesEdit, BranchesDelete,
        DepartmentsView, DepartmentsCreate, DepartmentsEdit, DepartmentsDelete,
        DesignationsView, DesignationsCreate, DesignationsEdit, DesignationsDelete,
        EmployeesView, EmployeesCreate, EmployeesEdit, EmployeesDelete,
        WarehousesView, WarehousesCreate, WarehousesEdit, WarehousesDelete,
        CurrenciesView, CurrenciesManage,
        SettingsView, SettingsEdit,
        AuditView, ProfileEdit,
        PermissionModulesView, PermissionModulesManage,
        PermissionActionsView, PermissionActionsManage
    };
}
