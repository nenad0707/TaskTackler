using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTackler.Models;


public class TodoModel
{
    /// <summary>
    /// Gets or sets the ID of the todo item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the task description of the todo item.
    /// </summary>
    [Required(ErrorMessage = "Task is required")]
    public string? Task { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the todo item is completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}