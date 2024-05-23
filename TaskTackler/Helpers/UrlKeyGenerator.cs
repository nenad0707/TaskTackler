namespace TaskTackler.Helpers;

public static class UrlKeyGenerator
{
    public static string GenerateUriKey(int pageNumber, int pageSize)
    {
        return $"Todos?pageNumber={pageNumber}&pageSize={pageSize}";
    }
}
