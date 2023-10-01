namespace TaskService.Dtos
{
    public record CreateTaskRec(string Title, string Description, DateTime DueDate);
    public record UpdateTaskRec(string? Title, string? Description, DateTime? DueDate, bool IsCompleted = false);
}
