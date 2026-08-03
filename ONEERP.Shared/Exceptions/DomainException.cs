namespace ONEERP.Shared.Exceptions;

/// <summary>
/// Business/domain error surfaced to the client with an explicit HTTP status.
/// Handled by the global exception middleware in both APIs.
/// </summary>
public class DomainException : Exception
{
    public int StatusCode { get; }

    public DomainException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }
}

public class UnauthorizedAccess : DomainException
{
    public UnauthorizedAccess(string message = "Unauthorized.") : base(message, 401)
    {
    }
}
