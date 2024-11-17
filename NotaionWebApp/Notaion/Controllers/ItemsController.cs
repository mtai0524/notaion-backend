using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notaion.Infrastructure.Context;
using Notaion.Domain.Entities;
using Notaion.Application.Common.Interfaces;

namespace Notaion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        public ItemsController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            return await _context.Items.OrderBy(x => x.Order).Where(x => x.IsHide == false).ToListAsync();
        }
        [HttpGet("get-list-items-hidden")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsHidden()
        {
            return await _context.Items.OrderBy(x => x.Order).Where(x => x.IsHide == true).ToListAsync();
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
            foreach (var i in items)
            {
                var item = await _context.Items.Where(x => x.Id == i.Id).FirstOrDefaultAsync();
                // if != null => item existing in db => update item
                if (item != null)
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
                foreach (var item in items)
                {
                    item.IsHide = true;
                }
                _context.UpdateRange(items);
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
                var item = await _context.Items.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (item != null)
                {
                    item.IsHide = true;
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Deleted" });
                }
                return NotFound(new { message = "Not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the item.", error = ex.Message });
            }
        }
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Upload a valid file.");
            }

            var imageUrl = await _cloudinaryService.UploadImageAsync(file);

            if (imageUrl == null)
            {
                return StatusCode(500, "Failed to upload image.");
            }

            return Ok(new { Url = imageUrl });
        }

    }
}