namespace TaskTackler.Models;

public class ApiResponse
{
    public List<TodoModel> Todos { get; set; }
    public int TotalPages { get; set; }
}