using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Notaion.Application.DTOs.English;
using Notaion.Application.Interfaces.Services;
using Notaion.Application.Services;

namespace Notaion.Controllers
{
    // English practice for the /english page and the notaion MCP server:
    // the user's cards / sentences / texts / writings, and the AI writing check.
    // Scheme named explicitly: the app registers cookie (OAuth) and JWT auth.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EnglishController : ControllerBase
    {
        public const string CheckRateLimitPolicy = "english-check";
        public const string DataRateLimitPolicy = "english-data";
        private const int MaxTextLength = 4000;

        private readonly IEnglishService _englishService;
        private readonly IEnglishItemService _itemService;

        public EnglishController(IEnglishService englishService, IEnglishItemService itemService)
        {
            _englishService = englishService;
            _itemService = itemService;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("items")]
        [EnableRateLimiting(DataRateLimitPolicy)]
        [ProducesResponseType(typeof(Dictionary<string, List<JsonObject>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetItemsAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(await _itemService.GetAllAsync(userId));
        }

        [HttpPut("items/{kind}/{id}")]
        [EnableRateLimiting(DataRateLimitPolicy)]
        [ProducesResponseType(typeof(JsonObject), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveItemAsync(string kind, string id, [FromBody] JsonObject? item)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var problem = EnglishItemRules.Validate(kind, item);
            if (problem != null) return Error(StatusCodes.Status422UnprocessableEntity, "invalid_item", problem);
            if (EnglishItemRules.ReadId(item!) != id) return Error(StatusCodes.Status422UnprocessableEntity, "id_mismatch", "The id in the URL and the body differ.");

            return Ok(await _itemService.SaveAsync(userId, kind, item!));
        }

        // Saves many items at once (first sync of browser data, MCP batches).
        [HttpPost("items/bulk")]
        [EnableRateLimiting(DataRateLimitPolicy)]
        [ProducesResponseType(typeof(EnglishBulkResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveItemsAsync([FromBody] Dictionary<string, List<JsonObject>>? itemsByKind)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (itemsByKind == null) return Error(StatusCodes.Status422UnprocessableEntity, "invalid_body", "Send an object of item lists by kind.");

            var total = itemsByKind.Values.Sum(items => items?.Count ?? 0);
            if (total > EnglishItemRules.MaxBulkItems)
            {
                return Error(StatusCodes.Status422UnprocessableEntity, "too_many_items", $"Too many items (max {EnglishItemRules.MaxBulkItems}).");
            }

            foreach (var (kind, items) in itemsByKind)
            {
                foreach (var item in items ?? new List<JsonObject>())
                {
                    var problem = EnglishItemRules.Validate(kind, item);
                    if (problem != null) return Error(StatusCodes.Status422UnprocessableEntity, "invalid_item", problem);
                }
            }

            var saved = await _itemService.SaveManyAsync(userId, itemsByKind.Where(pair => pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value));
            return Ok(new EnglishBulkResultDto { Saved = saved });
        }

        [HttpDelete("items/{kind}/{id}")]
        [EnableRateLimiting(DataRateLimitPolicy)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteItemAsync(string kind, string id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (!EnglishItemRules.IsKind(kind)) return Error(StatusCodes.Status422UnprocessableEntity, "invalid_kind", $"Unknown kind '{kind}'.");

            var deleted = await _itemService.DeleteAsync(userId, kind, id);
            if (!deleted) return Error(StatusCodes.Status404NotFound, "not_found", "Item not found.");
            return NoContent();
        }

        [HttpPost("check")]
        [EnableRateLimiting(CheckRateLimitPolicy)]
        [ProducesResponseType(typeof(EnglishCheckResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckAsync([FromBody] EnglishCheckDto dto)
        {
            var text = dto?.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return Error(StatusCodes.Status422UnprocessableEntity, "text_required", "Text is required.");
            }
            if (text.Length > MaxTextLength)
            {
                return Error(StatusCodes.Status422UnprocessableEntity, "text_too_long", $"Text is too long (max {MaxTextLength} characters).");
            }

            var result = await _englishService.CheckAsync(text);
            if (result == null)
            {
                return Error(StatusCodes.Status502BadGateway, "ai_unavailable", "The AI could not check this text. Please try again.");
            }

            return Ok(result);
        }

        private ObjectResult Error(int status, string code, string message) =>
            StatusCode(status, new { error = new { code, message } });
    }
}
