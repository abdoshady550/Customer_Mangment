namespace Customer_Mangment.Model.Results;

public readonly record struct Error(string Code, string Description, ErrorKind Type)
{
    public static Error None => new(string.Empty, string.Empty, ErrorKind.None);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorKind.Failure);

    public static Error Unexpected(string code, string description) =>
        new(code, description, ErrorKind.Unexpected);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorKind.Validation);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorKind.Conflict);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorKind.NotFound);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorKind.Unauthorized);
}