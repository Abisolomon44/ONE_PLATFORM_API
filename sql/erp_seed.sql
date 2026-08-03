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

/* ---------------------------------------------------------------------------
   Countries (location master hierarchy)
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
   Company
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyCode = @CompanyCode)
BEGIN
    INSERT INTO dbo.Companies (CompanyCode, CompanyName, [Address], Email, Phone, GST, Currency, Status, CreatedBy)
    VALUES (@CompanyCode, @CompanyName, '123 Business Avenue, City, Country', @CompanyEmail, '+1 000 000 0000', @CompanyCode + '-10001', @CurrencyCode, 'Active', 'system');
    SET @CompanyId = SCOPE_IDENTITY();
END
ELSE
    SELECT @CompanyId = CompanyId FROM dbo.Companies WHERE CompanyCode = @CompanyCode;

/* ---------------------------------------------------------------------------
   Roles
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
   Permissions
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
    ('Agriculture',     'Agriculture Business',     14),
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
    ('organization-types.view'), ('organization-types.manage')
) AS p(PermissionCode)
CROSS JOIN (SELECT RoleId FROM dbo.Roles WHERE Code IN ('SuperAdmin', 'Administrator')) r
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = p.PermissionCode
);
