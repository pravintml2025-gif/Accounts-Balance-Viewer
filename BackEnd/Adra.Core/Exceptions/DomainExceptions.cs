namespace Adra.Core.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(string accountName) 
        : base($"Account '{accountName}' was not found") { }
    
    public AccountNotFoundException(Guid accountId)
        : base($"Account with ID '{accountId}' was not found") { }
}

public class DuplicateAccountException : DomainException
{
    public DuplicateAccountException(string accountName)
        : base($"Account '{accountName}' already exists") { }
}

public class InvalidFileFormatException : DomainException
{
    public InvalidFileFormatException(string message) : base(message) { }
    public InvalidFileFormatException(string message, Exception innerException) : base(message, innerException) { }
}

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}

public class UnauthorizedOperationException : DomainException
{
    public UnauthorizedOperationException(string operation) 
        : base($"Unauthorized to perform operation: {operation}") { }
}
