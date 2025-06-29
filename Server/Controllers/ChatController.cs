using AuthWithAdmin.Server.Models.Bot;
using AuthWithAdmin.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthWithAdmin.Server.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // New endpoint to fetch all chat data at once
        [HttpGet("GetAllData")]
        public async Task<IActionResult> GetAllChatData()
        {
            try
            {
                _logger.LogInformation("Request received for all chat data");
                var chatData = await _chatService.GetAllPublicChatDataAsync();
                return Ok(chatData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all chat data");
                // Return a sanitized error message
                return StatusCode(500, "An error occurred while retrieving chat data.");
            }
        }

        // Individual endpoints for more granular data access
        [HttpGet("State/{id}")]
        public async Task<IActionResult> GetState(int id)
        {
            try
            {
                var state = await _chatService.GetStateByIdAsync(id);
                if (state == null)
                    return NotFound();

                return Ok(state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving state with ID: {StateId}", id);
                return StatusCode(500, "An error occurred while retrieving the state.");
            }
        }

        [HttpGet("Category/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _chatService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", id);
                return StatusCode(500, "An error occurred while retrieving the category.");
            }
        }

        [HttpGet("Question/{id}")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            try
            {
                var question = await _chatService.GetQuestionByIdAsync(id);
                if (question == null)
                    return NotFound();

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question with ID: {QuestionId}", id);
                return StatusCode(500, "An error occurred while retrieving the question.");
            }
        }

        [HttpGet("Category/{id}/Questions")]
        public async Task<IActionResult> GetQuestionsByCategory(int id)
        {
            try
            {
                var questions = await _chatService.GetQuestionsByCategoryAsync(id);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for category ID: {CategoryId}", id);
                return StatusCode(500, "An error occurred while retrieving the questions.");
            }
        }
    }
}