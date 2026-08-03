namespace ONEERP.ERP.API.DTOs;

/* ---------------- Languages ---------------- */

public record CreateLanguageRequest(string Name, string Code, string? CultureCode, bool IsRTL, bool IsDefault, int SortOrder = 1);

public record UpdateLanguageRequest(string Name, string Code, string? CultureCode, bool IsRTL, bool IsDefault, int SortOrder, bool IsActive);

public class LanguageDto
{
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? CultureCode { get; set; }
    public bool IsRTL { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- TimeZones ---------------- */

public record CreateTimeZoneRequest(string Name, string TimeZoneName, string? UTCOffset);

public record UpdateTimeZoneRequest(string Name, string TimeZoneName, string? UTCOffset, bool IsActive);

public class TimeZoneDto
{
    public int TimeZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZoneName { get; set; } = string.Empty;
    public string? UTCOffset { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- GSTRegistrationTypes ---------------- */

public record CreateGstRegistrationTypeRequest(string Name, string? Description);

public record UpdateGstRegistrationTypeRequest(string Name, string? Description, bool IsActive);

public class GstRegistrationTypeDto
{
    public int GstRegistrationTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- AddressTypes ---------------- */

public record CreateAddressTypeRequest(string Name, string? Description);

public record UpdateAddressTypeRequest(string Name, string? Description, bool IsActive);

public class AddressTypeDto
{
    public int AddressTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- ContactTypes ---------------- */

public record CreateContactTypeRequest(string Name, string? Description);

public record UpdateContactTypeRequest(string Name, string? Description, bool IsActive);

public class ContactTypeDto
{
    public int ContactTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- DocumentTypes ---------------- */

public record CreateDocumentTypeRequest(string Name, string? Description);

public record UpdateDocumentTypeRequest(string Name, string? Description, bool IsActive);

public class DocumentTypeDto
{
    public int DocumentTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- OrganizationTypes ---------------- */

public record CreateOrganizationTypeRequest(string Name, string Code, string? Description, int SortOrder = 1);

public record UpdateOrganizationTypeRequest(string Name, string Code, string? Description, int SortOrder, bool IsActive);

public class OrganizationTypeDto
{
    public int OrganizationTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
