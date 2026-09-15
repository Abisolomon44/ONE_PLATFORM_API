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

    public const string BusinessPartnerRolesView = "business-partner-roles.view";
    public const string BusinessPartnerRolesManage = "business-partner-roles.manage";

    public const string BusinessPartnersView = "business-partners.view";
    public const string BusinessPartnersManage = "business-partners.manage";

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

        public const string StoresView = "stores.view";
        public const string StoresCreate = "stores.create";
        public const string StoresEdit = "stores.edit";
        public const string StoresDelete = "stores.delete";

        public const string FinancialYearsView = "financial-years.view";
        public const string FinancialYearsCreate = "financial-years.create";
        public const string FinancialYearsEdit = "financial-years.edit";
        public const string FinancialYearsDelete = "financial-years.delete";

        public const string CountersView = "counters.view";
        public const string CountersCreate = "counters.create";
        public const string CountersEdit = "counters.edit";
        public const string CountersDelete = "counters.delete";

        public const string POSSessionView = "pos-sessions.view";
        public const string POSSessionCreate = "pos-sessions.create";
        public const string POSSessionEdit = "pos-sessions.edit";
        public const string POSSessionDelete = "pos-sessions.delete";

        public const string ProductCategoriesView = "product-categories.view";
        public const string ProductCategoriesCreate = "product-categories.create";
        public const string ProductCategoriesEdit = "product-categories.edit";
        public const string ProductCategoriesDelete = "product-categories.delete";

        public const string ProductSubCategoriesView = "product-subcategories.view";
        public const string ProductSubCategoriesCreate = "product-subcategories.create";
        public const string ProductSubCategoriesEdit = "product-subcategories.edit";
        public const string ProductSubCategoriesDelete = "product-subcategories.delete";

        public const string BrandsView = "brands.view";
        public const string BrandsCreate = "brands.create";
        public const string BrandsEdit = "brands.edit";
        public const string BrandsDelete = "brands.delete";

         public const string UnitsView = "units.view";
         public const string UnitsCreate = "units.create";
         public const string UnitsEdit = "units.edit";
         public const string UnitsDelete = "units.delete";

         public const string ProductsView = "products.view";
         public const string ProductsCreate = "products.create";
         public const string ProductsEdit = "products.edit";
         public const string ProductsDelete = "products.delete";

         public const string TaxTypeSystemsView = "tax-type-systems.view";
         public const string TaxTypeSystemsCreate = "tax-type-systems.create";
         public const string TaxTypeSystemsEdit = "tax-type-systems.edit";
         public const string TaxTypeSystemsDelete = "tax-type-systems.delete";

public const string TaxesView = "taxes.view";
           public const string TaxesCreate = "taxes.create";
           public const string TaxesEdit = "taxes.edit";
           public const string TaxesDelete = "taxes.delete";

           public const string PriceTypesView = "price-types.view";
           public const string PriceTypesCreate = "price-types.create";
           public const string PriceTypesEdit = "price-types.edit";
           public const string PriceTypesDelete = "price-types.delete";

           public const string UnitConversionsView = "unit-conversions.view";
           public const string UnitConversionsCreate = "unit-conversions.create";
           public const string UnitConversionsEdit = "unit-conversions.edit";
           public const string UnitConversionsDelete = "unit-conversions.delete";

           public const string BarcodesView = "barcodes.view";
           public const string BarcodesCreate = "barcodes.create";
           public const string BarcodesEdit = "barcodes.edit";
           public const string BarcodesDelete = "barcodes.delete";

           public const string HsnSacsView = "hsn-sacs.view";
           public const string HsnSacsCreate = "hsn-sacs.create";
           public const string HsnSacsEdit = "hsn-sacs.edit";
           public const string HsnSacsDelete = "hsn-sacs.delete";

           public const string ServiceCategoriesView = "service-categories.view";
           public const string ServiceCategoriesCreate = "service-categories.create";
           public const string ServiceCategoriesEdit = "service-categories.edit";
           public const string ServiceCategoriesDelete = "service-categories.delete";

           public const string ServicesView = "services.view";
           public const string ServicesCreate = "services.create";
           public const string ServicesEdit = "services.edit";
           public const string ServicesDelete = "services.delete";

           public const string PriceListsView = "price-lists.view";
           public const string PriceListsCreate = "price-lists.create";
           public const string PriceListsEdit = "price-lists.edit";
           public const string PriceListsDelete = "price-lists.delete";

           public const string PriceListDetailsView = "price-list-details.view";
           public const string PriceListDetailsCreate = "price-list-details.create";
           public const string PriceListDetailsEdit = "price-list-details.edit";
           public const string PriceListDetailsDelete = "price-list-details.delete";

           public const string DiscountRulesView = "discount-rules.view";
           public const string DiscountRulesCreate = "discount-rules.create";
           public const string DiscountRulesEdit = "discount-rules.edit";
           public const string DiscountRulesDelete = "discount-rules.delete";

           public const string OffersView = "offers.view";
           public const string OffersCreate = "offers.create";
           public const string OffersEdit = "offers.edit";
           public const string OffersDelete = "offers.delete";

           public const string OfferDetailsView = "offer-details.view";
           public const string OfferDetailsCreate = "offer-details.create";
           public const string OfferDetailsEdit = "offer-details.edit";
           public const string OfferDetailsDelete = "offer-details.delete";

           public const string CouponsView = "coupons.view";
           public const string CouponsCreate = "coupons.create";
           public const string CouponsEdit = "coupons.edit";
           public const string CouponsDelete = "coupons.delete";

            public const string MasterImportView = "master-import.view";
           public const string MasterImportManage = "master-import.manage";
           public const string ImportLogsView = "import-logs.view";

            public const string TenantConfigView = "tenant-config.view";
            public const string TenantConfigManage = "tenant-config.manage";

              public const string PurchasesView = "purchases.view";
              public const string PurchasesCreate = "purchases.create";
              public const string PurchasesEdit = "purchases.edit";
              public const string PurchasesCancel = "purchases.cancel";
              public const string PurchasesDelete = "purchases.delete";
              public const string PurchasesManage = "purchases.manage";
              // Legacy dot-form return permissions (matrix bridge derives these).
              public const string PurchasesReturnView = "purchases.return.view";
              public const string PurchasesReturnManage = "purchases.return.manage";
              // Final hyphen-form purchase-return permissions (per transaction spec).
              public const string PurchaseReturnCreate = "purchases-return.create";
              public const string PurchaseReturnView = "purchases-return.view";
              public const string PurchaseReturnEdit = "purchases-return.edit";
              public const string PurchaseReturnCancel = "purchases-return.cancel";
              public const string PurchaseReturnDelete = "purchases-return.delete";
             public const string SalesView = "sales.view";
             public const string SalesManage = "sales.manage";
             public const string SalesReturnView = "sales.return.view";
             public const string SalesReturnManage = "sales.return.manage";
             public const string SalesPOSView = "sales.pos.view";
             public const string StockView = "stock.view";
             public const string StockManage = "stock.manage";

             public const string PaymentTypesView = "payment-types.view";
            public const string PaymentTypesManage = "payment-types.manage";
            public const string PaymentMethodsView = "payment-methods.view";
            public const string PaymentMethodsManage = "payment-methods.manage";
            public const string PaymentMethodDetailsView = "payment-method-details.view";
            public const string PaymentMethodDetailsManage = "payment-method-details.manage";
            public const string PaymentsView = "payments.view";
            public const string PaymentsManage = "payments.manage";


         public const string CurrenciesView = "currencies.view";
    public const string CurrenciesManage = "currencies.manage";

    public const string SettingsView = "settings.view";
    public const string SettingsEdit = "settings.edit";

    public const string EntitiesView = "entities.view";
    public const string EntitiesManage = "entities.manage";

    public const string InvoiceTemplatesView = "invoice-templates.view";
    public const string InvoiceTemplatesCreate = "invoice-templates.create";
    public const string InvoiceTemplatesEdit = "invoice-templates.edit";
    public const string InvoiceTemplatesDelete = "invoice-templates.delete";
    public const string InvoiceTemplatesDuplicate = "invoice-templates.duplicate";
    public const string InvoiceTemplatesPreview = "invoice-templates.preview";
    public const string InvoiceTemplatesPublish = "invoice-templates.publish";
    public const string InvoiceTemplatesAssign = "invoice-templates.assign";
    public const string InvoiceTemplatesPrint = "invoice-templates.print";
    public const string InvoiceTemplatesExport = "invoice-templates.export";

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
        BusinessPartnerRolesView, BusinessPartnerRolesManage,
        BusinessPartnersView, BusinessPartnersManage,
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
        StoresView, StoresCreate, StoresEdit, StoresDelete,
        FinancialYearsView, FinancialYearsCreate, FinancialYearsEdit, FinancialYearsDelete,
        CountersView, CountersCreate, CountersEdit, CountersDelete,
        POSSessionView, POSSessionCreate, POSSessionEdit, POSSessionDelete,
         ProductCategoriesView, ProductCategoriesCreate, ProductCategoriesEdit, ProductCategoriesDelete,
         ProductSubCategoriesView, ProductSubCategoriesCreate, ProductSubCategoriesEdit, ProductSubCategoriesDelete,
         BrandsView, BrandsCreate, BrandsEdit, BrandsDelete,
          UnitsView, UnitsCreate, UnitsEdit, UnitsDelete,
          ProductsView, ProductsCreate, ProductsEdit, ProductsDelete,
          TaxTypeSystemsView, TaxTypeSystemsCreate, TaxTypeSystemsEdit, TaxTypeSystemsDelete,
           TaxesView, TaxesCreate, TaxesEdit, TaxesDelete,
           PriceTypesView, PriceTypesCreate, PriceTypesEdit, PriceTypesDelete,
           UnitConversionsView, UnitConversionsCreate, UnitConversionsEdit, UnitConversionsDelete,
           BarcodesView, BarcodesCreate, BarcodesEdit, BarcodesDelete,
           HsnSacsView, HsnSacsCreate, HsnSacsEdit, HsnSacsDelete,
           ServiceCategoriesView, ServiceCategoriesCreate, ServiceCategoriesEdit, ServiceCategoriesDelete,
           ServicesView, ServicesCreate, ServicesEdit, ServicesDelete,
           PriceListsView, PriceListsCreate, PriceListsEdit, PriceListsDelete,
           PriceListDetailsView, PriceListDetailsCreate, PriceListDetailsEdit, PriceListDetailsDelete,
           DiscountRulesView, DiscountRulesCreate, DiscountRulesEdit, DiscountRulesDelete,
           OffersView, OffersCreate, OffersEdit, OffersDelete,
           OfferDetailsView, OfferDetailsCreate, OfferDetailsEdit, OfferDetailsDelete,
           CouponsView, CouponsCreate, CouponsEdit, CouponsDelete,

            MasterImportView, MasterImportManage, ImportLogsView,
             TenantConfigView, TenantConfigManage,
              PurchasesView, PurchasesCreate, PurchasesEdit, PurchasesCancel, PurchasesDelete, PurchasesManage,
              PurchasesReturnView, PurchasesReturnManage,
              PurchaseReturnCreate, PurchaseReturnView, PurchaseReturnEdit, PurchaseReturnCancel, PurchaseReturnDelete,
              StockView, StockManage,
             SalesView, SalesManage, SalesReturnView, SalesReturnManage, SalesPOSView,
             PaymentTypesView, PaymentTypesManage,
             PaymentMethodsView, PaymentMethodsManage,
             PaymentMethodDetailsView, PaymentMethodDetailsManage,
             PaymentsView, PaymentsManage,

           CurrenciesView, CurrenciesManage,
        SettingsView, SettingsEdit,
        EntitiesView, EntitiesManage,
        InvoiceTemplatesView, InvoiceTemplatesCreate, InvoiceTemplatesEdit, InvoiceTemplatesDelete,
        InvoiceTemplatesDuplicate, InvoiceTemplatesPreview, InvoiceTemplatesPublish,
        InvoiceTemplatesAssign, InvoiceTemplatesPrint, InvoiceTemplatesExport,
        AuditView, ProfileEdit,
        PermissionModulesView, PermissionModulesManage,
        PermissionActionsView, PermissionActionsManage
    };
}
