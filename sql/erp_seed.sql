/* =============================================================================
   ONE ERP - ERP Database Seed Data
   Purpose  : Executed automatically by ONEERP.Platform.API when provisioning
              a new tenant database. NO GO statements (single Dapper batch).
              Parameterized by the provisioning service:
                @CompanyCode      - derived from the platform tenant code
                @CompanyName      - company name supplied at tenant creation
                @CompanyEmail     - admin/contact email for the tenant
                @CurrencyCode     - currency taken from the tenant plan
                @AdminUsername    - unique tenant admin username
                @AdminPasswordHash- BCrypt hash of the tenant admin password
                @AdminFullName    - display name of the tenant admin
   ============================================================================= */

DECLARE @CompanyId INT;
DECLARE @SuperAdminRoleId INT;
DECLARE @AdministratorRoleId INT;
DECLARE @AdminUserId INT;
DECLARE @CurrencyId INT;
DECLARE @LanguageId INT;
DECLARE @TimeZoneId INT;
DECLARE @BusinessTypeId INT;
DECLARE @IndustryTypeId INT;

/* ---------------------------------------------------------------------------
   Countries (location master hierarchy: Countries -> States -> Cities)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Countries ([Name], ISOCode2, ISOCode3, PhoneCode, CurrencyCode, Nationality, IsActive, CreatedBy)
SELECT c.[Name], c.ISOCode2, c.ISOCode3, c.PhoneCode, c.CurrencyCode, c.Nationality, 1, 'system'
FROM (VALUES
    ('India', 'IN', 'IND', '+91', 'INR', 'Indian'),
    ('United States', 'US', 'USA', '+1', 'USD', 'American'),
    ('United Arab Emirates', 'AE', 'ARE', '+971', 'AED', 'Emirati'),
    ('Singapore', 'SG', 'SGP', '+65', 'SGD', 'Singaporean'),
    ('Malaysia', 'MY', 'MYS', '+60', 'MYR', 'Malaysian')
) AS c([Name], ISOCode2, ISOCode3, PhoneCode, CurrencyCode, Nationality)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Countries t WHERE t.ISOCode2 = c.ISOCode2 OR t.ISOCode3 = c.ISOCode3
);

/* ---------------------------------------------------------------------------
   States (India)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.States (CountryId, [Name], StateCode, GSTStateCode, IsActive, CreatedBy)
SELECT c.CountryId, s.[Name], s.StateCode, s.GSTStateCode, 1, 'system'
FROM (VALUES
    ('Tamil Nadu', 'TN', '33'),
    ('Karnataka', 'KA', '29'),
    ('Kerala', 'KL', '32'),
    ('Andhra Pradesh', 'AP', '37'),
    ('Telangana', 'TS', '36'),
    ('Maharashtra', 'MH', '27')
) AS s([Name], StateCode, GSTStateCode)
INNER JOIN dbo.Countries c ON c.ISOCode2 = 'IN'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.States t WHERE t.CountryId = c.CountryId AND t.StateCode = s.StateCode
);

/* ---------------------------------------------------------------------------
   Cities (India)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Cities (CountryId, StateId, [Name], IsActive, CreatedBy)
SELECT c.CountryId, st.StateId, ci.[Name], 1, 'system'
FROM (VALUES
    ('TN', 'Chennai'), ('TN', 'Madurai'), ('TN', 'Coimbatore'), ('TN', 'Salem'),
    ('KA', 'Bengaluru'), ('KA', 'Mysuru'),
    ('KL', 'Kochi'), ('KL', 'Thiruvananthapuram'),
    ('MH', 'Mumbai'), ('MH', 'Pune')
) AS ci(StateCode, [Name])
INNER JOIN dbo.Countries c ON c.ISOCode2 = 'IN'
INNER JOIN dbo.States st ON st.CountryId = c.CountryId AND st.StateCode = ci.StateCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Cities t WHERE t.StateId = st.StateId AND t.[Name] = ci.[Name]
);

/* ---------------------------------------------------------------------------
   System Master Data - Currencies
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Currencies)
BEGIN
    INSERT INTO dbo.Currencies (CurrencyCode, CurrencyName, Symbol, ISOCode, DecimalPlaces, IsBaseCurrency, SortOrder)
    VALUES
    ('INR', 'Indian Rupee',         '₹',  '356', 2, 1, 1),
    ('USD', 'US Dollar',            '$',  '840', 2, 0, 2),
    ('EUR', 'Euro',                 '€',  '978', 2, 0, 3),
    ('GBP', 'Pound Sterling',        '£',  '826', 2, 0, 4),
    ('JPY', 'Japanese Yen',         '¥',  '392', 0, 0, 5),
    ('CNY', 'Yuan Renminbi',        '¥',  '156', 2, 0, 6),
    ('CHF', 'Swiss Franc',          'CHF', '756', 2, 0, 7),
    ('AUD', 'Australian Dollar',    'A$', '036', 2, 0, 8),
    ('CAD', 'Canadian Dollar',      'C$', '124', 2, 0, 9),
    ('NZD', 'New Zealand Dollar',   'NZ$', '554', 2, 0, 10),
    ('KRW', 'South Korean Won',    '₩',  '410', 0, 0, 11),
    ('SGD', 'Singapore Dollar',    'S$', '702', 2, 0, 12),
    ('MYR', 'Malaysian Ringgit',  'RM', '458', 2, 0, 13),
    ('THB', 'Thai Baht',           '฿',  '764', 2, 0, 14),
    ('IDR', 'Indonesian Rupiah',  'Rp', '360', 2, 0, 15),
    ('PHP', 'Philippine Peso',    '₱',  '608', 2, 0, 16),
    ('VND', 'Vietnamese Dong',    '₫',  '704', 2, 0, 17),
    ('MXN', 'Mexican Peso',       '$',  '484', 2, 0, 18),
    ('AED', 'UAE Dirham',         'د.إ', '784', 2, 0, 19),
    ('SAR', 'Saudi Riyal',        '﷼',  '682', 2, 0, 20),
    ('QAR', 'Qatari Riyal',       '﷼',  '634', 2, 0, 21),
    ('KWD', 'Kuwaiti Dinar',      'د.ك', '414', 3, 0, 22),
    ('BHD', 'Bahraini Dinar',     '.د.ب', '048', 3, 0, 23),
    ('OMR', 'Omani Rial',         '﷼',  '512', 3, 0, 24),
    ('EGP', 'Egyptian Pound',     '£',  '818', 2, 0, 25),
    ('ZAR', 'South African Rand', 'R',  '710', 2, 0, 26),
    ('NGN', 'Nigerian Naira',    '₦',  '566', 2, 0, 27),
    ('KES', 'Kenyan Shilling',   'KSh', '400', 2, 0, 28),
    ('TRY', 'Turkish Lira',      '₺',  '949', 2, 0, 29),
    ('RUB', 'Russian Ruble',     '₽',  '643', 2, 0, 30),
    ('BRL', 'Brazilian Real',    'R$', '986', 2, 0, 31),
    ('ARS', 'Argentine Peso',    '$',  '032', 2, 0, 32),
    ('BDT', 'Bangladeshi Taka', '৳',  '050', 2, 0, 33),
    ('LKR', 'Sri Lankan Rupee', 'Rs',  '144', 2, 0, 34),
    ('NPR', 'Nepalese Rupee',   '₨',  '524', 2, 0, 35),
    ('PKR', 'Pakistani Rupee',  '₨',  '586', 2, 0, 36);
END

/* ---------------------------------------------------------------------------
   System Master Data - Languages
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Languages)
BEGIN
    INSERT INTO dbo.Languages ([Name], Code, CultureCode, IsRTL, IsDefault, SortOrder)
    VALUES
    ('English', 'en', 'en-IN', 0, 1, 1),
    ('Tamil',   'ta', 'ta-IN', 0, 0, 2),
    ('Hindi',   'hi', 'hi-IN', 0, 0, 3),
    ('Arabic',  'ar', 'ar-SA', 1, 0, 4),
    ('French',  'fr', 'fr-FR', 0, 0, 5);
END

/* ---------------------------------------------------------------------------
   System Master Data - TimeZones
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.TimeZones)
BEGIN
    INSERT INTO dbo.TimeZones ([Name], TimeZoneName, UTCOffset)
    VALUES
    ('India Standard Time',      'Asia/Kolkata',       'UTC+05:30'),
    ('UTC',                      'UTC',                'UTC+00:00'),
    ('Singapore Standard Time',  'Asia/Singapore',     'UTC+08:00'),
    ('Arabian Standard Time',    'Asia/Dubai',         'UTC+04:00'),
    ('Eastern Standard Time',    'America/New_York',   'UTC-05:00');
END

/* ---------------------------------------------------------------------------
   System Master Data - GSTRegistrationTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.GSTRegistrationTypes)
BEGIN
    INSERT INTO dbo.GSTRegistrationTypes ([Name], [Description])
    VALUES
    ('Regular',       'Regular GST Registration'),
    ('Composition',   'Composition Scheme'),
    ('SEZ',           'Special Economic Zone'),
    ('Unregistered',  'Not GST Registered'),
    ('Consumer',      'End Consumer');
END

/* ---------------------------------------------------------------------------
   Master Data - BusinessTypes
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.BusinessTypes)
BEGIN
    INSERT INTO dbo.BusinessTypes ([Name], [Description], SortOrder)
    VALUES
    ('Retail',          'Retail Business',          1),
    ('Wholesale',       'Wholesale Business',       2),
    ('Distribution',    'Distribution Business',    3),
    ('Trading',         'Trading Company',          4),
    ('Manufacturing',   'Manufacturing Company',    5),
    ('Service',         'Service Business',         6),
    ('E-Commerce',      'Online Business',          7),
    ('Construction',    'Construction Business',    8),
    ('Healthcare',      'Healthcare Organization',  9),
    ('Education',       'Educational Institution',  10),
    ('Hospitality',     'Hotel & Restaurant',       11),
    ('Logistics',       'Transport & Logistics',    12),
    ('Finance',         'Financial Services',       13),
    ('Agriculture',     'Agriculture Business',       14),
    ('Multi Business',  'Multiple Business Activities', 15);
END

/* ---------------------------------------------------------------------------
   Master Data - IndustryTypes
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.IndustryTypes)
BEGIN
    INSERT INTO dbo.IndustryTypes ([Name], [Description], SortOrder)
    VALUES
    ('Electronics',     'Mobile & Electronics',        1),
    ('Supermarket',     'Supermarket & Grocery',       2),
    ('Pharmacy',        'Medical & Pharmacy',          3),
    ('Textile',         'Garments & Textile',          4),
    ('Automobile',      'Automobile Industry',         5),
    ('Furniture',       'Furniture Industry',          6),
    ('Jewellery',       'Jewellery Business',          7),
    ('Food & Beverage', 'Food Industry',               8),
    ('IT',              'Software & IT Services',      9),
    ('Healthcare',      'Hospital & Medical',          10),
    ('Education',       'School & College',            11),
    ('Construction',    'Construction Industry',       12),
    ('Manufacturing',   'General Manufacturing',       13),
    ('Logistics',       'Transport & Logistics',       14),
    ('E-Commerce',      'Online Commerce',             15);
END

/* ---------------------------------------------------------------------------
   Master Data - CompanyGroups
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.CompanyGroups)
BEGIN
    INSERT INTO dbo.CompanyGroups (GroupCode, GroupName, ShortName, [Description])
    VALUES
    ('DEFAULT',      'Default Group',        'DEFAULT',     'Default Company Group'),
    ('RETAIL',       'Retail Group',         'RETAIL',      'Retail Business Group'),
    ('WHOLESALE',    'Wholesale Group',      'WHOLESALE',   'Wholesale Business Group'),
    ('DISTRIBUTION', 'Distribution Group',   'DIST',        'Distribution Companies'),
    ('MANUFACTURING','Manufacturing Group',  'MFG',         'Manufacturing Companies'),
    ('SERVICES',     'Services Group',       'SERV',        'Service Companies'),
    ('HEALTH',       'Healthcare Group',     'HEALTH',      'Healthcare Organizations'),
    ('EDU',          'Education Group',      'EDU',         'Educational Institutions');
END

/* ---------------------------------------------------------------------------
   System Master Data - AddressTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.AddressTypes)
BEGIN
    INSERT INTO dbo.AddressTypes ([Name], [Description])
    VALUES
    ('Registered',  'Registered Office'),
    ('Billing',     'Billing Address'),
    ('Shipping',    'Shipping Address'),
    ('Office',      'Office Address'),
    ('Warehouse',   'Warehouse Address'),
    ('Factory',     'Factory Address');
END

/* ---------------------------------------------------------------------------
   System Master Data - ContactTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ContactTypes)
BEGIN
    INSERT INTO dbo.ContactTypes ([Name], [Description])
    VALUES
    ('Primary',   'Primary Contact'),
    ('Sales',     'Sales Contact'),
    ('Purchase',  'Purchase Contact'),
    ('Accounts',  'Accounts Contact'),
    ('HR',        'Human Resources'),
    ('Support',   'Customer Support');
END

/* ---------------------------------------------------------------------------
   System Master Data - DocumentTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentTypes)
BEGIN
    INSERT INTO dbo.DocumentTypes ([Name], [Description])
    VALUES
    ('GST Certificate',     'GST Registration'),
    ('PAN Card',            'Permanent Account Number'),
    ('TAN Certificate',     'Tax Deduction Number'),
    ('CIN Certificate',     'Company Registration'),
    ('Trade License',       'Trade License'),
    ('MSME Certificate',    'MSME Registration'),
    ('Logo',                'Company Logo'),
    ('Digital Signature',   'Digital Signature');
END

/* ---------------------------------------------------------------------------
   System Master Data - OrganizationTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.OrganizationTypes)
BEGIN
    INSERT INTO dbo.OrganizationTypes ([Name], Code, [Description], SortOrder)
    VALUES
    ('Head Office',    'HO',        'Corporate Head Office',  1),
    ('Branch',         'BRANCH',    'Company Branch',         2),
    ('Warehouse',      'WAREHOUSE', 'Warehouse',              3),
    ('Store',          'STORE',     'Retail Store',           4),
    ('Department',     'DEPARTMENT','Department',             5),
    ('Division',       'DIVISION',  'Business Division',      6),
    ('Business Unit',  'BU',        'Business Unit',          7),
    ('Cost Center',    'CC',        'Cost Center',            8),
    ('Profit Center',  'PC',        'Profit Center',          9),
    ('Factory',        'FACTORY',   'Manufacturing Unit',    10),
    ('Showroom',       'SHOWROOM',  'Sales Showroom',        11),
    ('Service Center', 'SERVICE',   'Service Center',        12);
END

/* ---------------------------------------------------------------------------
   Roles (must exist before Company for User assignment, and before permissions)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'SuperAdmin')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Super Admin', 'SuperAdmin', 'Full system access', 1, 1, 'system');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'Administrator')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Administrator', 'Administrator', 'Administrative access', 1, 1, 'system');
END

SELECT @SuperAdminRoleId = RoleId FROM dbo.Roles WHERE Code = 'SuperAdmin';
SELECT @AdministratorRoleId = RoleId FROM dbo.Roles WHERE Code = 'Administrator';

/* ---------------------------------------------------------------------------
   Company (master data must exist for FK references)
   --------------------------------------------------------------------------- */
SELECT TOP 1 @CurrencyId = Id FROM dbo.Currencies WHERE CurrencyCode = @CurrencyCode;
SELECT TOP 1 @LanguageId = LanguageId FROM dbo.Languages WHERE IsDefault = 1;
SELECT TOP 1 @TimeZoneId = TimeZoneId FROM dbo.TimeZones ORDER BY TimeZoneId;
SELECT TOP 1 @BusinessTypeId = BusinessTypeId FROM dbo.BusinessTypes;
SELECT TOP 1 @IndustryTypeId = IndustryTypeId FROM dbo.IndustryTypes;

IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyCode = @CompanyCode)
BEGIN
    INSERT INTO dbo.Companies (CompanyCode, CompanyName, ShortName, Abbreviation, BusinessTypeId, IndustryTypeId,
        GSTRegistrationTypeId, GSTNumber, PANNumber, TANNumber, CINNumber, RegistrationNumber,
        CurrencyId, LanguageId, TimeZoneId, IsActive, IsBlocked, IsDeleted, LastLoginDate, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
    VALUES (@CompanyCode, @CompanyName, NULL, @CompanyCode, @BusinessTypeId, @IndustryTypeId,
        NULL, NULL, NULL, NULL, NULL, @CompanyCode + '-10001',
        @CurrencyId, @LanguageId, @TimeZoneId, 1, 0, 0, NULL, 0, SYSUTCDATETIME(), NULL, NULL);
    SET @CompanyId = SCOPE_IDENTITY();
END
ELSE
    SELECT @CompanyId = Id FROM dbo.Companies WHERE CompanyCode = @CompanyCode;

/* ---------------------------------------------------------------------------
    Permissions (SuperAdmin + Administrator)
    --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissions (RoleId, PermissionCode, CreatedBy)
SELECT @SuperAdminRoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('dashboard.view'),
    ('companies.view'), ('companies.create'), ('companies.edit'),
    ('users.view'), ('users.create'), ('users.edit'), ('users.delete'),
    ('roles.view'), ('roles.manage'),
    ('locations.view'), ('locations.create'), ('locations.edit'), ('locations.delete'),
    ('business-types.view'), ('business-types.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('settings.view'), ('settings.edit'),
    ('audit.view'),
    ('currencies.view'), ('currencies.manage'),
    ('profile.edit')
) AS p(PermissionCode)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = @SuperAdminRoleId AND rp.PermissionCode = p.PermissionCode
);

INSERT INTO dbo.RolePermissions (RoleId, PermissionCode, CreatedBy)
SELECT @AdministratorRoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('dashboard.view'),
    ('companies.view'), ('companies.create'), ('companies.edit'),
    ('users.view'), ('users.create'), ('users.edit'), ('users.delete'),
    ('roles.view'),
    ('locations.view'), ('locations.create'), ('locations.edit'), ('locations.delete'),
    ('business-types.view'), ('business-types.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('settings.view'), ('settings.edit'),
    ('currencies.view'), ('currencies.manage'),
    ('profile.edit')
) AS p(PermissionCode)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = @AdministratorRoleId AND rp.PermissionCode = p.PermissionCode
);

/* ---------------------------------------------------------------------------
    ERP Admin user (unique per tenant, BCrypt hashed)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @AdminUsername)
BEGIN
    INSERT INTO dbo.Users (CompanyId, Username, Email, Mobile, FullName, PasswordHash, Status, IsSuperAdmin, CreatedBy)
    VALUES (@CompanyId, @AdminUsername, @CompanyEmail, '+1 000 000 0000', @AdminFullName, @AdminPasswordHash, 'Active', 1, 'system');
    SET @AdminUserId = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedBy)
    VALUES (@AdminUserId, @SuperAdminRoleId, 'system');
END

/* ---------------------------------------------------------------------------
    Application Settings
    --------------------------------------------------------------------------- */
INSERT INTO dbo.ApplicationSettings (SettingKey, SettingValue, [Description], UpdatedBy)
SELECT k.SettingKey, k.SettingValue, k.[Description], 'system'
FROM (VALUES
    ('Company.Name', @CompanyName, 'Company display name'),
    ('Company.Address', '123 Business Avenue, City, Country', 'Company address'),
    ('Company.Email', @CompanyEmail, 'Company email'),
    ('Company.Phone', '+1 000 000 0000', 'Company phone'),
    ('Company.Currency', @CurrencyCode, 'Default currency'),
    ('Security.PasswordMinLength', '8', 'Minimum password length'),
    ('Security.PasswordRequireSpecial', 'True', 'Require special character in password')
) AS k(SettingKey, SettingValue, [Description])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ApplicationSettings s WHERE s.SettingKey = k.SettingKey
);

/* ---------------------------------------------------------------------------
    System Master Permissions (both SuperAdmin and Administrator)
    --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissions (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('languages.view'), ('languages.manage'),
    ('timezones.view'), ('timezones.manage'),
    ('gst-registration-types.view'), ('gst-registration-types.manage'),
    ('address-types.view'), ('address-types.manage'),
    ('contact-types.view'), ('contact-types.manage'),
    ('document-types.view'), ('document-types.manage'),
    ('organization-types.view'), ('organization-types.manage'),
    ('currencies.view'), ('currencies.manage')
) AS p(PermissionCode)
CROSS JOIN (SELECT RoleId FROM dbo.Roles WHERE Code IN ('SuperAdmin', 'Administrator')) r
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = p.PermissionCode
);

/* ---------------------------------------------------------------------------
    BranchTypes
    --------------------------------------------------------------------------- */
INSERT INTO dbo.BranchTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
SELECT k.[Name], k.Code, k.[Description], k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('Head Office', 'HQ', 'Main headquarters branch', 1, 1, 'system'),
    ('Sales Branch', 'SAL', 'Sales and marketing branch', 2, 1, 'system'),
    ('Purchase Branch', 'PUR', 'Purchase and procurement branch', 3, 1, 'system'),
    ('Service Branch', 'SRV', 'Service and support branch', 4, 1, 'system'),
    ('Warehouse', 'WH', 'Warehouse and inventory branch', 5, 1, 'system')
) AS k([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.BranchTypes bt WHERE bt.Code = k.Code
);

/* ---------------------------------------------------------------------------
    WarehouseTypes
    --------------------------------------------------------------------------- */
INSERT INTO dbo.WarehouseTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
SELECT k.[Name], k.Code, k.[Description], k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('Main Warehouse', 'MAIN', 'Primary warehouse', 1, 1, 'system'),
    ('Sales Warehouse', 'SAL', 'Sales warehouse', 2, 1, 'system'),
    ('Purchase Warehouse', 'PUR', 'Purchase warehouse', 3, 1, 'system'),
    ('Return Warehouse', 'RET', 'Returns and defective goods', 4, 1, 'system'),
    ('Raw Material', 'RAW', 'Raw material warehouse', 5, 1, 'system'),
    ('Finished Goods', 'FGD', 'Finished goods warehouse', 6, 1, 'system')
) AS k([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.WarehouseTypes wt WHERE wt.Code = k.Code
);

/* ---------------------------------------------------------------------------
    EmploymentTypes
    --------------------------------------------------------------------------- */
INSERT INTO dbo.EmploymentTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
SELECT k.[Name], k.Code, k.[Description], k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('Full Time', 'FT', 'Full-time employee', 1, 1, 'system'),
    ('Part Time', 'PT', 'Part-time employee', 2, 1, 'system'),
    ('Contract', 'CT', 'Contract employee', 3, 1, 'system'),
    ('Intern', 'IN', 'Intern or trainee', 4, 1, 'system'),
    ('Temporary', 'TMP', 'Temporary employee', 5, 1, 'system')
) AS k([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EmploymentTypes et WHERE et.Code = k.Code
);

/* ---------------------------------------------------------------------------
    Genders
    --------------------------------------------------------------------------- */
INSERT INTO dbo.Genders ([Name], Code, SortOrder)
SELECT k.[Name], k.Code, k.SortOrder
FROM (VALUES
    ('Male', 'M', 1),
    ('Female', 'F', 2),
    ('Other', 'O', 3)
) AS k([Name], Code, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Genders g WHERE g.Code = k.Code
);

/* ---------------------------------------------------------------------------
    MaritalStatuses
    --------------------------------------------------------------------------- */
INSERT INTO dbo.MaritalStatuses ([Name], Code, SortOrder)
SELECT k.[Name], k.Code, k.SortOrder
FROM (VALUES
    ('Single', 'S', 1),
    ('Married', 'M', 2),
    ('Divorced', 'D', 3),
    ('Widowed', 'W', 4)
) AS k([Name], Code, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.MaritalStatuses ms WHERE ms.Code = k.Code
);