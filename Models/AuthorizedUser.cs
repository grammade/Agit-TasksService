namespace TaskService.Models;

public class AuthorizedUser
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string IP { get; set; }
}
