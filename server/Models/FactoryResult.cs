namespace SmartCv.Models;

public enum FactoryStatus
{
    Success,
    NotFound,
    Invalid
}

public class FactoryResult
{
    public FactoryStatus Status { get; protected init; }
    public string Error { get; protected init; } = string.Empty;

    public static FactoryResult Success() => new() { Status = FactoryStatus.Success };
    public static FactoryResult NotFound(string error) => new() { Status = FactoryStatus.NotFound, Error = error };
    public static FactoryResult Invalid(string error) => new() { Status = FactoryStatus.Invalid, Error = error };
}

public class FactoryResult<T> : FactoryResult
{
    public T? Value { get; private init; }

    public static FactoryResult<T> Success(T value) => new() { Status = FactoryStatus.Success, Value = value };
    public static new FactoryResult<T> NotFound(string error) => new() { Status = FactoryStatus.NotFound, Error = error };
    public static new FactoryResult<T> Invalid(string error) => new() { Status = FactoryStatus.Invalid, Error = error };
}
