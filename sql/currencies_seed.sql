/* =============================================================================
   ONE ERP - Currencies Master Seed Data
   Purpose  : Seeds the Currencies master table with commonly used world
              currencies following ISO 4217 standards.
   Idempotent: Safe to re-run; inserts only when the table is empty.
   ============================================================================= */

IF NOT EXISTS (SELECT 1 FROM dbo.Currencies)
BEGIN
    INSERT INTO dbo.Currencies
        (CurrencyCode, CurrencyName, Symbol, ISOCode, DecimalPlaces, IsBaseCurrency, SortOrder, IsActive, CreatedBy)
    VALUES
    -- India
    ('INR', 'Indian Rupee',         N'₹',  '356', 2, 1, 1, 1, 1),
    -- United States
    ('USD', 'US Dollar',            N'$',   '840', 2, 0, 2, 1, 1),
    -- European Union
    ('EUR', 'Euro',                 N'€',  '978', 2, 0, 3, 1, 1),
    -- United Kingdom
    ('GBP', 'Pound Sterling',        N'£',  '826', 2, 0, 4, 1, 1),
    -- Japan (0 decimal places)
    ('JPY', 'Japanese Yen',         N'¥',  '392', 0, 0, 5, 1, 1),
    -- China
    ('CNY', 'Yuan Renminbi',        N'¥',  '156', 2, 0, 6, 1, 1),
    -- Switzerland
    ('CHF', 'Swiss Franc',          N'CHF', '756', 2, 0, 7, 1, 1),
    -- Australia
    ('AUD', 'Australian Dollar',    N'A$', '036', 2, 0, 8, 1, 1),
    -- Canada
    ('CAD', 'Canadian Dollar',      N'C$', '124', 2, 0, 9, 1, 1),
    -- New Zealand
    ('NZD', 'New Zealand Dollar',   N'NZ$', '554', 2, 0, 10, 1, 1),
    -- South Korea (0 decimal places)
    ('KRW', 'South Korean Won',     N'₩',  '410', 0, 0, 11, 1, 1),
    -- Singapore
    ('SGD', 'Singapore Dollar',      N'S$', '702', 2, 0, 12, 1, 1),
    -- Malaysia
    ('MYR', 'Malaysian Ringgit',    N'RM', '458', 2, 0, 13, 1, 1),
    -- Thailand
    ('THB', 'Thai Baht',            N'฿',  '764', 2, 0, 14, 1, 1),
    -- Indonesia
    ('IDR', 'Indonesian Rupiah',    N'Rp', '360', 2, 0, 15, 1, 1),
    -- Philippines
    ('PHP', 'Philippine Peso',      N'₱',  '608', 2, 0, 16, 1, 1),
    -- Vietnam
    ('VND', 'Vietnamese Dong',      N'₫',  '704', 2, 0, 17, 1, 1),
    -- Mexico
    ('MXN', 'Mexican Peso',         N'$',  '484', 2, 0, 18, 1, 1),
    -- UAE
    ('AED', 'UAE Dirham',           N'د.إ', '784', 2, 0, 19, 1, 1),
    -- Saudi Arabia
    ('SAR', 'Saudi Riyal',          N'﷼',  '682', 2, 0, 20, 1, 1),
    -- Qatar
    ('QAR', 'Qatari Riyal',         N'﷼',  '634', 2, 0, 21, 1, 1),
    -- Kuwait (3 decimal places)
    ('KWD', 'Kuwaiti Dinar',        N'د.ك', '414', 3, 0, 22, 1, 1),
    -- Bahrain (3 decimal places)
    ('BHD', 'Bahraini Dinar',       N'.د.ب', '048', 3, 0, 23, 1, 1),
    -- Oman (3 decimal places)
    ('OMR', 'Omani Rial',           N'﷼',  '512', 3, 0, 24, 1, 1),
    -- Egypt
    ('EGP', 'Egyptian Pound',       N'£',  '818', 2, 0, 25, 1, 1),
    -- South Africa
    ('ZAR', 'South African Rand',  N'R',  '710', 2, 0, 26, 1, 1),
    -- Nigeria
    ('NGN', 'Nigerian Naira',       N'₦',  '566', 2, 0, 27, 1, 1),
    -- Kenya
    ('KES', 'Kenyan Shilling',      N'KSh', '400', 2, 0, 28, 1, 1),
    -- Turkey
    ('TRY', 'Turkish Lira',         N'₺',  '949', 2, 0, 29, 1, 1),
    -- Russia
    ('RUB', 'Russian Ruble',        N'₽',  '643', 2, 0, 30, 1, 1),
    -- Brazil
    ('BRL', 'Brazilian Real',       N'R$', '986', 2, 0, 31, 1, 1),
    -- Argentina
    ('ARS', 'Argentine Peso',       N'$',  '032', 2, 0, 32, 1, 1),
    -- Bangladesh
    ('BDT', 'Bangladeshi Taka',     N'৳',  '050', 2, 0, 33, 1, 1),
    -- Sri Lanka
    ('LKR', 'Sri Lankan Rupee',     N'Rs',  '144', 2, 0, 34, 1, 1),
    -- Nepal
    ('NPR', 'Nepalese Rupee',       N'₨',  '524', 2, 0, 35, 1, 1),
    -- Pakistan
    ('PKR', 'Pakistani Rupee',      N'₨',  '586', 2, 0, 36, 1, 1);
END
