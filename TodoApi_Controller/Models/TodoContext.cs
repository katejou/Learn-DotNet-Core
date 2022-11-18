using Microsoft.EntityFrameworkCore;

namespace TodoApi_Controller;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }
    
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
    //不懂這個地方，!是什麼，為什麼和沒有controller的TodoApi長不一樣？(見圖)
}