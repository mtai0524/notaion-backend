using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Context;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            return await _context.Items.OrderBy(x=> x.Order).ToListAsync();
        }

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
        }

        // POST: api/Items/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> PostItemsBulk(IEnumerable<Item> items)
        {
            foreach(var i in items)
            {
                var item = await _context.Items.Where(x=> x.Id == i.Id).FirstOrDefaultAsync();
                // if != null => item existing in db => update item
                if(item != null)
                {
                    item.Content = i.Content;
                    item.Heading = i.Heading;
                    item.Order = i.Order;
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _context.Items.Add(i);
                    await _context.SaveChangesAsync();
                }
            }
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                // Xóa tất cả các mục
                var items = await _context.Items.ToListAsync();
                _context.Items.RemoveRange(items);
                await _context.SaveChangesAsync();

                return Ok(new { message = "All items deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting items.", error = ex.Message });
            }
        }

        [HttpDelete("delete-item/{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                // Tìm tất cả các mục với ID đã cho
                var items = await _context.Items.Where(x => x.Id == id).ToListAsync();

                if (items.Any())
                {
                    // Xóa tất cả các mục
                    _context.Items.RemoveRange(items);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Deleted" });
                }

                // Nếu không tìm thấy mục nào để xóa
                return NotFound(new { message = "Not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the item.", error = ex.Message });
            }
        }

    }
}
