using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Entities
{
    public class User
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [MaxLength(255)]
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public ICollection<Task>? Tasks { get; set; }
    }
}
