namespace TaskTackler.Models;

public class PaginatedResponse<T>
{
    public T? Data { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
