using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));//<-假資料庫
//"TodoList" 是假資料庫的名稱，TodoDb是一個DbContext和Todo物件有關連
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var todoItems = app.MapGroup("/todoitems");//路由

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete/{tf}", GetCompleteTodos); //<這個自己加的，和 DTO 沒什麼關系
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapGet("/Secret/{id}", GetSecret); //<這個自己加的，來作對比。
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

// 這裡的重點是 DTO, DTO 和 Todo這個完全和資料庫Table一樣的物件分離。
// 回傳DTO，則可以操作，使客戶只得到一部份處理過的內容，而不是完全公開的資料。

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
    // 取回全部，然後逐個輸入為新的TodoItemDTO
    // 新的 TodoItemDTO 不收 Secret ，並且被 ToArrayAsync() 不同步地結成串列。
    // 回傳給使用者時，就沒有了所有的 Secret 
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo)) //同理，會不收Secret 
            : TypedResults.NotFound();
}

// 自己加，來作對比(上方是有DTO，這裡是沒有)
static async Task<IResult> GetSecret(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
}

//新增
static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db) //收入 DTO
{
    var todoItem = new Todo // init 出一個 Todo，傳入來自外部 DTO 的值。
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name,
        Secret = "Not telling you" //我自己設的
    };

    db.Todos.Add(todoItem); // 入資料庫
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

//修改
static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);//找出來

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;//修改
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

//刪除是沒差…
static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.Ok(todo);
    }

    return TypedResults.NotFound();
}

//為什麼有條件的那個不見了？我等一下自己寫看看…
static async Task<IResult> GetCompleteTodos(string tf, TodoDb db)
{
    if(tf == "t")
        return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete == true).ToListAsync());
    else 
        return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete == false).ToListAsync());
}