using System.ComponentModel.DataAnnotations;

namespace TaskService.Models;

public class TransactionModel
{
    public Guid Id { get; set; }
    public int userId { get; set; }
    public int totalItem { get; set; }
    public ICollection<TransactionItem> Items { get; set; }
}
public class TransactionItem
{
    [Key]
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public int FoodId { get; set; }
    public string FoodName { get; set; }
}