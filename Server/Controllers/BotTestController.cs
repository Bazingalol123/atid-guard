// Controllers/BotTestController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using AuthWithAdmin.Server.Data;
using AuthWithAdmin.Models;
using AuthWithAdmin.Models.Bot;
using AuthWithAdmin.Server.Models;


namespace AuthWithAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotTestController : ControllerBase
    {
        private readonly DbRepository _repository;

        public BotTestController(DbRepository repository)
        {
            _repository = repository;
        }

        // GET: api/BotTest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BotStates>>> GetAllStates()
        {
            try
            {
                // Get all states
                string query = "SELECT Id, Type as type, Content as content, CategoryId as categoryId FROM BotStates";
                var states = await _repository.GetRecordsAsync<BotStates>(query);

                if (states == null || !states.Any())
                    return NotFound("No states found");

                // For each state, fetch its related options
                List<BotStates> result = new List<BotStates>();
                foreach (var state in states)
                {
                    string optionsQuery = "SELECT Id, StateId as stateId, Text as text, NextStateId as nextStateId, Action as action, ActionParams as actionParams FROM BotOptions WHERE StateId = @StateId";
                    var options = await _repository.GetRecordsAsync<BotOptions>(optionsQuery, new { StateId = state.Id });

                    state.Options = options.ToList();
                    result.Add(state);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error fetching states: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetState(int id)
        {
            try
            {
                string query = "SELECT Id, Type as type, Content as content, CategoryId as categoryId FROM BotStates WHERE Id = @Id";
                var states = await _repository.GetRecordsAsync<BotStates>(query, new { Id = id });
                var state = states.FirstOrDefault();

                if (state == null)
                    return NotFound($"State with ID '{id}' not found");

                // Check if this is the welcome state
                if (state.type == "welcome")
                {
                    // Get categories from database
                    string categoriesQuery = "SELECT ID, CategoryName FROM Categories";
                    var categories = await _repository.GetRecordsAsync<Categories>(categoriesQuery);

                    // Load the default options
                    string optionsQuery = "SELECT Id, StateId as stateId, Text as text, NextStateId as nextStateId, Action as action, ActionParams as actionParams FROM BotOptions WHERE StateId = @StateId";
                    var defaultOptions = await _repository.GetRecordsAsync<BotOptions>(optionsQuery, new { StateId = id });

                    // Create list to hold all options
                    List<BotOptions> allOptions = defaultOptions.ToList();

                    // For each category, find its questions state and add an option
                    foreach (var category in categories)
                    {
                        // Find the questions state for this category
                        string questionsStateQuery = "SELECT Id FROM BotStates WHERE type = 'questions' AND categoryId = @CategoryId LIMIT 1";
                        var questionsStates = await _repository.GetRecordsAsync<dynamic>(questionsStateQuery, new { CategoryId = category.ID });

                        if (questionsStates.Any())
                        {
                            int nextStateId = Convert.ToInt32(questionsStates.First().Id);

                            // Add this category as an option
                            allOptions.Add(new BotOptions
                            {
                                Id = -1 * category.ID, // Use negative IDs for dynamic options
                                stateId = id,
                                text = category.categoryName + " Questions",
                                nextStateId = nextStateId,
                                action = "navigate",
                                actionParams = ""
                            });
                        }
                    }

                    state.Options = allOptions;
                }
                else
                {
                    // For non-welcome states, get options normally
                    string optionsQuery = "SELECT Id, StateId as stateId, Text as text, NextStateId as nextStateId, Action as action, ActionParams as actionParams FROM BotOptions WHERE StateId = @StateId";
                    var options = await _repository.GetRecordsAsync<BotOptions>(optionsQuery, new { StateId = id });
                    state.Options = options.ToList();
                }

                return Ok(state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching state: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

  
        // POST: api/BotTest
        [HttpPost]
        public async Task<IActionResult> AddState(BotStates stateToAdd)
        {
            try
            {
                // Don't include ID in the insert - let SQLite auto-generate it
                string query = "INSERT INTO BotStates (Content, Type, CategoryId) VALUES (@content, @type, @categoryId)";

                object newCategoryParam = new
                {
                    content = stateToAdd.content,
                    type = stateToAdd.type,
                    categoryId = stateToAdd.categoryId
                };

                int newStateId = await _repository.InsertReturnIdAsync(query, newCategoryParam);

                if (newStateId > 0)
                {
                    // Just fetch the newly created category
                    string stateQuery = "SELECT ID as Id, content, type FROM BotStates WHERE ID = @ID";
                    var categoryResult = await _repository.GetRecordsAsync<BotStates>(stateQuery, new { ID = newStateId });
                    BotStates newState = categoryResult.FirstOrDefault();

                    // Initialize empty questions list
                    stateToAdd.Options = new List<BotOptions>();

                    return Ok(newState);
                }

                return BadRequest("Category not added");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/BotTest/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateState(int id, BotStates stateToUpdate)
        {
            try
            {
                if (id != stateToUpdate.Id)
                {
                    return BadRequest("ID mismatch");
                }

                // Check if state exists
                string checkQuery = "SELECT COUNT(1) FROM BotStates WHERE Id = @Id";
                int count = await _repository.ExecuteScalarAsync<int>(checkQuery, new { Id = id });

                if (count == 0)
                {
                    return NotFound($"State with ID '{id}' not found");
                }

                // Update state
                object stateParam = new
                {
                    Id = id,
                    Type = stateToUpdate.type,
                    Content = stateToUpdate.content,
                    CategoryId = stateToUpdate.categoryId
                };

                string updateQuery = "UPDATE BotStates SET Type = @Type, Content = @Content, CategoryId = @CategoryId WHERE Id = @Id";
                await _repository.SaveDataAsync(updateQuery, stateParam);

                // Delete existing options
                string deleteOptionsQuery = "DELETE FROM BotOptions WHERE StateId = @StateId";
                await _repository.SaveDataAsync(deleteOptionsQuery, new { StateId = id });

                // Insert new options
                if (stateToUpdate.Options != null && stateToUpdate.Options.Any())
                {
                    foreach (var option in stateToUpdate.Options)
                    {
                        object optionParam = new
                        {
                            StateId = id,
                            Text = option.text,
                            NextStateId = option.nextStateId,
                            Action = option.action,
                            ActionParams = option.actionParams
                        };

                        string insertOptionQuery = "INSERT INTO BotOptions (StateId, Text, NextStateId, Action, ActionParams) VALUES (@StateId, @Text, @NextStateId, @Action, @ActionParams)";
                        await _repository.SaveDataAsync(insertOptionQuery, optionParam);
                    }
                }

                return Ok("State updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating state: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/BotTest/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteState(int id)
        {
            try
            {
                // Delete options first
                string deleteOptionsQuery = "DELETE FROM BotOptions WHERE StateId = @StateId";
                await _repository.SaveDataAsync(deleteOptionsQuery, new { StateId = id });

                // Delete state
                string deleteStateQuery = "DELETE FROM BotStates WHERE Id = @Id";
                int rowsAffected = await _repository.SaveDataAsync(deleteStateQuery, new { Id = id });

                if (rowsAffected > 0)
                {
                    return Ok("State is Deleted successfully");
                }
                else
                {
                    return NotFound("State is not found, Hence was not deleted");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting state: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // POST: api/BotTest/next
        [HttpPost("next")]
        public async Task<ActionResult<BotStates>> GetNextState([FromBody] BotOptions userSelection)
        {
            try
            {
                // Get the next state based on the user's selected option
                string query = "SELECT Id, Type, Content, CategoryId FROM BotStates WHERE Id = @NextStateId";
                var states = await _repository.GetRecordsAsync<BotStates>(query, new { NextStateId = userSelection.nextStateId });

                var nextState = states.FirstOrDefault();
                if (nextState == null)
                    return NotFound("Next state not found");

                // Load options for the next state
                string optionsQuery = "SELECT Id, StateId, Text, NextStateId, Action, ActionParams FROM BotOptions WHERE StateId = @StateId";
                var options = await _repository.GetRecordsAsync<BotOptions>(optionsQuery, new { StateId = nextState.Id });

                nextState.Options = options.ToList();

                // If the selected option has an action, process it
                if (!string.IsNullOrEmpty(userSelection.action))
                {
                    switch (userSelection.action.ToLower())
                    {
                        case "save":
                            // Example: Save user input (future implementation)
                            Console.WriteLine($"Saving data: {userSelection.actionParams}");
                            break;

                        case "call_api":
                            // Example: Call an external API (future implementation)
                            Console.WriteLine($"Calling API with params: {userSelection.actionParams}");
                            break;

                        case "redirect":
                            // Example: Redirect user (handled in frontend)
                            return Ok(new { redirectTo = userSelection.actionParams });

                        default:
                            Console.WriteLine("Unknown action");
                            break;

                        case "showanswer":
                            // Special case for handling question answers
                            if (int.TryParse(userSelection.actionParams, out int questionId))
                            {
                                string questionQuery = "SELECT Id, questionText, answerText, categoryId FROM Questions WHERE Id = @Id";
                                var questions = await _repository.GetRecordsAsync<Questions>(questionQuery, new { Id = questionId });

                                var question = questions.FirstOrDefault();
                                if (question != null)
                                {
                                    // Create an ad-hoc state for the answer
                                    var answerState = new BotStates
                                    {
                                        Id = -1, // Use a temporary ID
                                        type = "answer",
                                        content = question.answerText,
                                        categoryId = question.categoryId,
                                        Options = new List<BotOptions>
                {
                    new BotOptions
                    {
                        Id = -1,
                        stateId = -1,
                        text = "Download PDF Guide",
                        nextStateId = -1,
                        action = "downloadPdf",
                        actionParams = $"question_{questionId}.pdf"
                    },
                    new BotOptions
                    {
                        Id = -2,
                        stateId = -1,
                        text = "Next Steps",
                        nextStateId = await GetNextStepsStateId(),
                        action = "navigate",
                        actionParams = ""
                    },
                    new BotOptions
                    {
                        Id = -3,
                        stateId = -1,
                        text = "End Conversation",
                        nextStateId = await GetEndChatStateId(),
                        action = "navigate",
                        actionParams = ""
                    }
                }
                                    };

                                    return Ok(answerState);
                                }
                                else
                                {
                                    return NotFound($"Question with ID '{questionId}' not found");
                                }
                            }
                            break;

                    }
                }

                return Ok(nextState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching next state: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task<int> GetNextStepsStateId()
        {
            string query = "SELECT Id FROM BotStates WHERE type = 'next_steps' ORDER BY Id ASC LIMIT 1";
            var result = await _repository.GetRecordsAsync<int>(query);
            return result.FirstOrDefault();
        }

        private async Task<int> GetEndChatStateId()
        {
            string query = "SELECT Id FROM BotStates WHERE type = 'end_chat' ORDER BY Id ASC LIMIT 1";
            var result = await _repository.GetRecordsAsync<int>(query);
            return result.FirstOrDefault();
        }

    }
}