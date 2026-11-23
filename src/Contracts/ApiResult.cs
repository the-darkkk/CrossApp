namespace Contracts;

public sealed record ApiResult<T>(bool Success, T? Data, string? Error = null);

public sealed record ValidationProblemResponse(string Type, string Title, int Status, Dictionary<string, string[]> Errors);
