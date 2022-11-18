using Microsoft.EntityFrameworkCore; //<- 後加的為了使用虛假的資料庫
using TodoApi_Controller;//<- 後加為了使用 TodoContext 這個類別

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>  //<- 後加 (見最上方兩行)
    opt.UseInMemoryDatabase("TodoList"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();   //Core的做法，就是都先引入後再一起build
//這樣就可以DI，保證下方所有的拿到的物件都是同一。

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers(); // <-- 詳情可見︰
//https://learn.microsoft.com/zh-tw/aspnet/core/mvc/controllers/routing?view=aspnetcore-7.0#attribute-routing-for-rest-apis

app.Run();
