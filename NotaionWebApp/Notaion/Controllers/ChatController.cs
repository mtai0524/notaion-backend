using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Notaion.Infrastructure.Context;
using Notaion.Domain.Entities;
using Notaion.Hubs;
using Microsoft.EntityFrameworkCore;
using Notaion.Domain.Models;
using Notaion.Application.DTOs.Chats;
using Notaion.Application.Common.Helpers;
using AutoMapper;
using Notaion.Application.Services;
using Notaion.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatService chatService;
        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext, IChatService chatService)
        {
            _context = context;
            _hubContext = hubContext;
            this.chatService = chatService;
        }

        //[HttpGet("test-genaric-repo")]
        //public async Task<IActionResult> GetChatWithGenaricRepo()
        //{
        //    var chats = await chatService.GetChatsAsync();
        //    return Ok(chats);
        //}

        //[Authorize]
        [HttpGet("get-chats")]
        public async Task<IActionResult> GetChats()
        {
            return Ok(await this.chatService.GetChatsAsync());
        }

        [HttpGet("get-chats-hidden")]
        public async Task<IActionResult> GetChatsHidden()
        {
            return Ok(await this.chatService.GetChatsHiddenAsync());
        }

        [HttpPost("add-chat")]
        public async Task<IActionResult> AddChat([FromBody] CreateChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.Content))
            {
                return BadRequest("Invalid chat message.");
            }

            try
            {
                var createdChat = await this.chatService.CreateChatAsync(chatDto);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", createdChat.UserName, createdChat.Content);

                return Ok(createdChat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("delete-all-chats")]
        public async Task<IActionResult> DeleteAllChats()
        {
            try
            {
                var updateRecords = await chatService.HideChatAllAsync();
                return Ok(new { Message = "All chats msg is hidden successfully", UpdateRecords = updateRecords });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "error when hide chats", Error = ex.Message });
            }
        }

        [HttpDelete("delete-me-chats/{id}")]
        public async Task<IActionResult> DeleteMeChats(string id)
        {
            var listChats = await _context.Chat.Where(x => x.User.Id == id).ToListAsync();
            if (listChats == null)
            {
                return BadRequest();
            }
            foreach (var chat in listChats)
            {
                chat.Hide = true;
            }
            _context.UpdateRange(listChats);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}

