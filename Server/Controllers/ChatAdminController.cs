using AuthWithAdmin.Models;
using AuthWithAdmin.Models.Bot;
using AuthWithAdmin.Server.AuthHelpers;
using AuthWithAdmin.Server.Models;
using AuthWithAdmin.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthWithAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,User")]
    [ApiController]
    public class ChatAdminController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatAdminController> _logger;

        public ChatAdminController(IChatService chatService, ILogger<ChatAdminController> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Bot State Management

        [HttpPost("State")]
        public async Task<IActionResult> CreateState(BotStates state)
        {
            try
            {
                int newId = await _chatService.CreateStateAsync(state);
                return CreatedAtAction(nameof(GetState), new { id = newId }, state);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot state");
                return StatusCode(500, "An error occurred while creating the state.");
            }
        }

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

        [HttpPut("State/{id}")]
        public async Task<IActionResult> UpdateState(int id, BotStates state)
        {
            try
            {
                if (id != state.Id)
                    return BadRequest("ID mismatch");

                bool success = await _chatService.UpdateStateAsync(state);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating state with ID: {StateId}", id);
                return StatusCode(500, "An error occurred while updating the state.");
            }
        }

        [HttpDelete("State/{id}")]
        public async Task<IActionResult> DeleteState(int id)
        {
            try
            {
                bool success = await _chatService.DeleteStateAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting state with ID: {StateId}", id);
                return StatusCode(500, "An error occurred while deleting the state.");
            }
        }

        #endregion

        #region Category Management

        [HttpPost("Category")]
        public async Task<IActionResult> CreateCategory(Categories category)
        {
            try
            {
                int newId = await _chatService.CreateCategoryAsync(category);
                var createdCategory = await _chatService.GetCategoryByIdAsync(newId);

                return CreatedAtAction(nameof(GetCategory), new { id = newId }, createdCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "An error occurred while creating the category.");
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

        [HttpPut("Category/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Categories category)
        {
            try
            {
                if (id != category.ID)
                    return BadRequest("ID mismatch");

                bool success = await _chatService.UpdateCategoryAsync(category);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", id);
                return StatusCode(500, "An error occurred while updating the category.");
            }
        }

        [HttpDelete("Category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                bool success = await _chatService.DeleteCategoryAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", id);
                return StatusCode(500, "An error occurred while deleting the category.");
            }
        }

        #endregion

        #region Question Management

        [HttpPost("Question")]
        public async Task<IActionResult> CreateQuestion(Questions question)
        {
            try
            {
                int newId = await _chatService.CreateQuestionAsync(question);
                var createdQuestion = await _chatService.GetQuestionByIdAsync(newId);

                return CreatedAtAction(nameof(GetQuestion), new { id = newId }, createdQuestion);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                return StatusCode(500, "An error occurred while creating the question.");
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

        [HttpPut("Question/{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, Questions question)
        {
            try
            {
                if (id != question.id)
                    return BadRequest("ID mismatch");

                bool success = await _chatService.UpdateQuestionAsync(question);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question with ID: {QuestionId}", id);
                return StatusCode(500, "An error occurred while updating the question.");
            }
        }

        [HttpDelete("Question/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                bool success = await _chatService.DeleteQuestionAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question with ID: {QuestionId}", id);
                return StatusCode(500, "An error occurred while deleting the question.");
            }
        }

        #endregion
    }
}