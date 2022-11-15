// using Microsoft.EntityFrameworkCore;

// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));//<-假資料庫
// builder.Services.AddDatabaseDeveloperPageExceptionFilter();//意外處理
// var app = builder.Build();

// var todoItems = app.MapGroup("/todoitems");//路由

// todoItems.MapGet("/", GetAllTodos);
// todoItems.MapGet("/complete", GetCompleteTodos);
// todoItems.MapGet("/{id}", GetTodo);
// todoItems.MapPost("/", CreateTodo);
// todoItems.MapPut("/{id}", UpdateTodo);
// todoItems.MapDelete("/{id}", DeleteTodo);

// app.Run();

// // 上述的段落是骨肉分離
// // 下述是不使用 await 和 Results(Lambda) 之後，改用 Task<IResult> 和 TypedResults

// static async Task<IResult> GetAllTodos(TodoDb db) // 方法的名稱有寫得更加的清楚，都不用注解了。
// {
//     return TypedResults.Ok(await db.Todos.ToArrayAsync());
// }

// // 這一段是 Assert ? 但我不太懂可以放在哪？
// // public async Task GetAllTodos_ReturnsOkOfTodosResult()
// // {
// //     // Arrange
// //     var db = CreateDbContext();

// //     // Act
// //     var result = await TodosApi.GetAllTodos(db);

// //     // Assert: Check for the correct returned type
// //     Assert.IsType<Ok<Todo[]>>(result);
// // }

// static async Task<IResult> GetCompleteTodos(TodoDb db)
// {
//     return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
// }

// static async Task<IResult> GetTodo(int id, TodoDb db)
// {
//     return await db.Todos.FindAsync(id)
//         is Todo todo
//             ? TypedResults.Ok(todo)
//             : TypedResults.NotFound();
// }

// static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
// {
//     db.Todos.Add(todo);
//     await db.SaveChangesAsync();

//     return TypedResults.Created($"/todoitems/{todo.Id}", todo);
// }

// static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoDb db)
// {
//     var todo = await db.Todos.FindAsync(id);

//     if (todo is null) return TypedResults.NotFound();

//     todo.Name = inputTodo.Name;
//     todo.IsComplete = inputTodo.IsComplete;

//     await db.SaveChangesAsync();

//     return TypedResults.NoContent();
// }

// static async Task<IResult> DeleteTodo(int id, TodoDb db)
// {
//     if (await db.Todos.FindAsync(id) is Todo todo)
//     {
//         db.Todos.Remove(todo);
//         await db.SaveChangesAsync();
//         return TypedResults.Ok(todo);
//     }

//     return TypedResults.NotFound();
// }