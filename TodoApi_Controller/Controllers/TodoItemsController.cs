using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi_Controller;

namespace TodoApi_Controller.Controllers
{
    [Route("api/ABC")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        // 接收DI的建構子，接收了同一個 DB_Context，給所有的方法使用
        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        //使用 DTO 時，會多了這個方法，這是個 Arrow Function 就沒有 return 
        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete
            };

        //查全部
        // GET: api/TodoItems
        // [HttpGet()] <這個標簽的話，就會用 GET: api/TodoItems
        [HttpGet("ALL")]// <這個標簽的話，就會用GET: api/TodoItems/ALL
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            
            if (_context.TodoItems == null)
            {
              return NotFound();
            }

            //未用 DTO 防止過份曝光之前…
            //return await _context.TodoItems.ToListAsync();

            return await _context.TodoItems
            .Select(x => ItemToDTO(x)) //引用上方的轉換方法(用作加密也不錯…)
            .ToListAsync(); // 記得要改<IEnumerable<TodoItem>>為 TodoItemDTO
        }

        //查一筆
        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
        {
            if (_context.TodoItems == null)
            {
                return NotFound();
            }
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            //return todoItem;
            return ItemToDTO(todoItem);
            // 記得要改<IEnumerable<TodoItem>>為 TodoItemDTO
        }

        //修改
        // PUT: api/TodoItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItemDTO todoDTO)//TodoItem todoItem)
        {
            //凡 todoItem 的，都加DTO 
            if (id != todoDTO.Id)//todoItem.Id)
            {
                return BadRequest();
            }

#region 這一段是DTO之後才加的
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }
            todoItem.Name = todoDTO.Name;
            todoItem.IsComplete = todoDTO.IsComplete;
#endregion

            //這個是使用DTO之前生成的，是連查都沒查出來，就直接覆蓋了吧？
            //_context.Entry(todoItem).State = EntityState.Modified;//這個連配對值都不用，十分簡潔？
            //如像先把它放入一個罐子，下方才真的送出去。

            try
            {
                await _context.SaveChangesAsync();
            }
            // catch (DbUpdateConcurrencyException)
            // {
            //     if (!TodoItemExists(id))
            //     {
            //         return NotFound();
            //     }
            //     else
            //     {
            //         throw;
            //     }
            // } // 上面是自動生成的，後來我看DTO的時候，發現可以這樣改？catch 現在都有 when 了？
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            
            return NoContent();
        }

        // 新增
        // POST: api/TodoItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> PostTodoItem(TodoItemDTO todoDTO)//>> PostTodoItem(TodoItem todoItem)
        {
            if (_context.TodoItems == null) //但是Default那邊不是設成了null!嗎？不懂？
            {
                return Problem("Entity set 'TodoContext.TodoItems' is null.");
            }

#region 這一段是DTO之後才加的
            var todoItem = new TodoItem
            {
                IsComplete = todoDTO.IsComplete,
                Name = todoDTO.Name
            };
#endregion

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            //return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
            //使用nameof的話，可以保證自己沒有拼錯字。
            //Console.WriteLine(nameof(List<int>));  // output: List
            // CreatedAtAction 也是之前沒見過的新方法呢！

            //使用 DTO 前︰上方，後︰下方

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, ItemToDTO(todoItem));
        }

        // 刪除 (不受 DTO 影響)
        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            if (_context.TodoItems == null)
            {
                return NotFound();
            }
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //被Put修改所運用
        private bool TodoItemExists(long id)
        {
            return (_context.TodoItems?.Any(e => e.Id == id)).GetValueOrDefault();
            //TodoItems 可能為預設的 null，所以要加問號
            //Any，比較省效能，因為不用抓那一筆資料回來
            //.GetValueOrDefault() 和 ToArray() 一樣，是召喚這個 Linq 的方法。
            //Default 是 bool 嗎？ 如果 TodoItems 為null 的話? Any 是回傳 false ?
            
            //https://stackoverflow.com/questions/29626329/how-does-getvalueordefault-work
            //以我的理解是這個地方說︰GetValueOrDefault之中，有一個HasValue的判斷。
            //TodoItems? 不能觸發 Any 的話，就進到 GetValueOrDefault()。
            //HasValue的判斷 null 為 false，於是回傳 false。

            //我用 Swagger 去測，也測到這方法回傳 false 了。

        }
    }
}
