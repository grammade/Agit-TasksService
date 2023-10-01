namespace TaskService.Dtos
{
    public class UserDto
    {
        public string Username { get; set; }
        public string? Password { get; set; }
    }
    public record UserRec(string Username, string Password);
    public record LoginRec(string Username, string Password);
}
