using AuthWithAdmin.Models;
using AuthWithAdmin.Models.Bot;
using AuthWithAdmin.Server.Controllers;
using AuthWithAdmin.Server.Data;
using AuthWithAdmin.Server.Models;
using AuthWithAdmin.Server.Models.Bot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthWithAdmin.Server.Services
{
    public class ChatService : IChatService
    {
        private readonly DbRepository _repository;
        private readonly ILogger<ChatService> _logger;

        public ChatService(DbRepository repository, ILogger<ChatService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Public Chat Data Methods

        /// <summary>
        /// Retrieves all chat data needed for the public chat interface
        /// </summary>
        public async Task<ChatDataModel> GetAllPublicChatDataAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all public chat data");

                // Create the response model
                var chatData = new ChatDataModel();

                // 1. Fetch all bot states
                var states = await _repository.GetRecordsAsync<BotStates>(
                    "SELECT * FROM BotStates");

                // For each state, get its options
                foreach (var state in states)
                {
                    var options = await _repository.GetRecordsAsync<BotOptions>(
                        "SELECT * FROM BotOptions WHERE stateId = @StateId",
                        new { StateId = state.Id });

                    state.Options = options.ToList();
                }

                chatData.States = states.ToDictionary(s => s.Id);

                // 2. Fetch all categories
                var categories = await _repository.GetRecordsAsync<Categories>(
     "SELECT * FROM Categories");
                chatData.Categories = categories.ToList();

                // Fetch all questions
                var questions = await _repository.GetRecordsAsync<Questions>(
                    "SELECT * FROM Questions");

                // Group questions by category ID
                var groupedQuestions = questions.GroupBy(q => q.categoryId)
                                                .ToDictionary(g => g.Key, g => g.ToList());

                // Assign questions to their respective categories
                foreach (var category in chatData.Categories)
                {
                    if (groupedQuestions.TryGetValue(category.ID, out var categoryQuestions))
                    {
                        category.questions = categoryQuestions;
                    }
                    else
                    {
                        category.questions = new List<Questions>(); // Ensure it's never null
                    }
                }

                // Also store the questions separately if needed
                chatData.Questions = groupedQuestions;

                // Extract next steps from the questions
                chatData.nextSteps = questions
                    .Where(q => !string.IsNullOrEmpty(q.nextStep))
                    .ToDictionary(q => q.id, q => q.nextStep);

                return chatData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all public chat data");
                throw; // Rethrow to let the controller handle it appropriately
            }
        }

        /// <summary>
        /// Gets a specific bot state by its ID
        /// </summary>
        public async Task<BotStates> GetStateByIdAsync(int stateId)
        {
            try
            {
                _logger.LogInformation("Retrieving bot state with ID: {StateId}", stateId);

                var stateQuery = "SELECT Id, type, content, categoryId FROM BotStates WHERE Id = @Id";
                var states = await _repository.GetRecordsAsync<BotStates>(stateQuery, new { Id = stateId });

                var state = states.FirstOrDefault();
                if (state != null)
                {
                    var optionsQuery = "SELECT Id, stateId, text, nextStateId, action, actionParams FROM BotOptions WHERE stateId = @StateId";
                    var options = await _repository.GetRecordsAsync<BotOptions>(optionsQuery, new { StateId = stateId });
                    state.Options = options.ToList();
                }

                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bot state with ID: {StateId}", stateId);
                throw;
            }
        }

        /// <summary>
        /// Gets a specific category by its ID
        /// </summary>
        public async Task<Categories> GetCategoryByIdAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Retrieving category with ID: {CategoryId}", categoryId);

                var query = "SELECT ID, CategoryName, image FROM Categories WHERE ID = @ID";
                var categories = await _repository.GetRecordsAsync<Categories>(query, new { ID = categoryId });

                return categories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", categoryId);
                throw;
            }
        }

        /// <summary>
        /// Gets a specific question by its ID
        /// </summary>
        public async Task<Questions> GetQuestionByIdAsync(int questionId)
        {
            try
            {
                _logger.LogInformation("Retrieving question with ID: {QuestionId}", questionId);

                var query = "SELECT Id, questionText, answerText, categoryId, nextStep FROM Questions WHERE Id = @Id";
                var questions = await _repository.GetRecordsAsync<Questions>(query, new { Id = questionId });

                return questions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question with ID: {QuestionId}", questionId);
                throw;
            }
        }

        /// <summary>
        /// Gets all questions for a specific category
        /// </summary>
        public async Task<List<Questions>> GetQuestionsByCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Retrieving questions for category ID: {CategoryId}", categoryId);

                var query = "SELECT Id, questionText, answerText, categoryId, nextStep, pdfPath FROM Questions WHERE categoryId = @CategoryId";
                var questions = await _repository.GetRecordsAsync<Questions>(query, new { CategoryId = categoryId });

                return questions.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for category ID: {CategoryId}", categoryId);
                throw;
            }
        }

        #endregion

        #region Admin Bot State Management

        /// <summary>
        /// Creates a new bot state and its associated options
        /// </summary>
        public async Task<int> CreateStateAsync(BotStates state)
        {
            try
            {
                _logger.LogInformation("Creating new bot state");

                // Validate the state
                if (string.IsNullOrEmpty(state.type) || string.IsNullOrEmpty(state.content))
                {
                    throw new ArgumentException("State type and content are required");
                }

                // Insert the state
                string insertQuery = "INSERT INTO BotStates (type, content, categoryId) VALUES (@type, @content, @categoryId)";
                int newStateId = await _repository.InsertReturnIdAsync(insertQuery, new
                {
                    type = state.type,
                    content = state.content,
                    categoryId = state.categoryId
                });

                // Insert options if any
                if (state.Options != null && state.Options.Any())
                {
                    foreach (var option in state.Options)
                    {
                        option.stateId = newStateId;
                        await CreateOptionAsync(option);
                    }
                }

                return newStateId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot state");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing bot state and its options
        /// </summary>
        public async Task<bool> UpdateStateAsync(BotStates state)
        {
            try
            {
                _logger.LogInformation("Updating bot state with ID: {StateId}", state.Id);

                // Validate the state
                if (state.Id <= 0)
                {
                    throw new ArgumentException("Invalid state ID");
                }

                if (string.IsNullOrEmpty(state.type) || string.IsNullOrEmpty(state.content))
                {
                    throw new ArgumentException("State type and content are required");
                }

                // Update the state
                string updateQuery = "UPDATE BotStates SET type = @type, content = @content, categoryId = @categoryId WHERE Id = @Id";
                int result = await _repository.SaveDataAsync(updateQuery, new
                {
                    Id = state.Id,
                    type = state.type,
                    content = state.content,
                    categoryId = state.categoryId
                });

                if (result <= 0)
                {
                    return false; // No rows affected, state not found
                }

                // Update options
                if (state.Options != null)
                {
                    // Delete existing options
                    await DeleteOptionsForStateAsync(state.Id);

                    // Add new options
                    foreach (var option in state.Options)
                    {
                        option.stateId = state.Id;
                        await CreateOptionAsync(option);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot state with ID: {StateId}", state.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a bot state and its options
        /// </summary>
        public async Task<bool> DeleteStateAsync(int stateId)
        {
            try
            {
                _logger.LogInformation("Deleting bot state with ID: {StateId}", stateId);

                // Delete options first
                await DeleteOptionsForStateAsync(stateId);

                // Delete the state
                string deleteQuery = "DELETE FROM BotStates WHERE Id = @Id";
                int result = await _repository.SaveDataAsync(deleteQuery, new { Id = stateId });

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bot state with ID: {StateId}", stateId);
                throw;
            }
        }

        #endregion

        #region Admin Bot Option Management

        /// <summary>
        /// Creates a new bot option
        /// </summary>
        public async Task<int> CreateOptionAsync(BotOptions option)
        {
            try
            {
                _logger.LogInformation("Creating new bot option for state ID: {StateId}", option.stateId);

                // Validate the option
                if (option.stateId <= 0)
                {
                    throw new ArgumentException("Invalid state ID for option");
                }

                if (string.IsNullOrEmpty(option.text))
                {
                    throw new ArgumentException("Option text is required");
                }

                // Insert the option
                string insertQuery = @"
                    INSERT INTO BotOptions (stateId, text, nextStateId, action, actionParams) 
                    VALUES (@stateId, @text, @nextStateId, @action, @actionParams)";

                int newOptionId = await _repository.InsertReturnIdAsync(insertQuery, new
                {
                    stateId = option.stateId,
                    text = option.text,
                    nextStateId = option.nextStateId,
                    action = option.action,
                    actionParams = option.actionParams
                });

                return newOptionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot option");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing bot option
        /// </summary>
        public async Task<bool> UpdateOptionAsync(BotOptions option)
        {
            try
            {
                _logger.LogInformation("Updating bot option with ID: {OptionId}", option.Id);

                // Validate the option
                if (option.Id <= 0)
                {
                    throw new ArgumentException("Invalid option ID");
                }

                if (string.IsNullOrEmpty(option.text))
                {
                    throw new ArgumentException("Option text is required");
                }

                // Update the option
                string updateQuery = @"
                    UPDATE BotOptions 
                    SET stateId = @stateId, text = @text, nextStateId = @nextStateId, action = @action, actionParams = @actionParams 
                    WHERE Id = @Id";

                int result = await _repository.SaveDataAsync(updateQuery, new
                {
                    Id = option.Id,
                    stateId = option.stateId,
                    text = option.text,
                    nextStateId = option.nextStateId,
                    action = option.action,
                    actionParams = option.actionParams
                });

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot option with ID: {OptionId}", option.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a bot option
        /// </summary>
        public async Task<bool> DeleteOptionAsync(int optionId)
        {
            try
            {
                _logger.LogInformation("Deleting bot option with ID: {OptionId}", optionId);

                string deleteQuery = "DELETE FROM BotOptions WHERE Id = @Id";
                int result = await _repository.SaveDataAsync(deleteQuery, new { Id = optionId });

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bot option with ID: {OptionId}", optionId);
                throw;
            }
        }

        /// <summary>
        /// Deletes all options for a specific state
        /// </summary>
        public async Task<bool> DeleteOptionsForStateAsync(int stateId)
        {
            try
            {
                _logger.LogInformation("Deleting all options for state ID: {StateId}", stateId);

                string deleteQuery = "DELETE FROM BotOptions WHERE stateId = @StateId";
                await _repository.SaveDataAsync(deleteQuery, new { StateId = stateId });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting options for state ID: {StateId}", stateId);
                throw;
            }
        }

        #endregion

        #region Admin Category Management

        /// <summary>
        /// Creates a new category
        /// </summary>
        public async Task<int> CreateCategoryAsync(Categories category)
        {
            try
            {
                _logger.LogInformation("Creating new category");

                // Validate the category
                if (string.IsNullOrEmpty(category.categoryName))
                {
                    throw new ArgumentException("Category name is required");
                }
               

                    // Insert the category with image if provided
                    string insertQuery = string.IsNullOrEmpty(category.image)
                       ? "INSERT INTO Categories (categoryName, categoryNameEN, categoryNameAR) VALUES (@CategoryName, @categoryNameEN, @categoryNameAR)"
                       : "INSERT INTO Categories (categoryName, categoryName, categoryNameEN, categoryNameAR, image) VALUES (@CategoryName, @categoryNameEN, @categoryNameAR, @Image)";

                    int newCategoryId = await _repository.InsertReturnIdAsync(insertQuery, new
                    {
                        CategoryName = category.categoryName,
                        categoryNameEN = category.categoryNameEN,
                        categoryNameAR = category.categoryNameAR,
                        Image = category.image
                    });
                    return newCategoryId;

                
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(Categories category)
        {
            try
            {
                _logger.LogInformation("Updating category with ID: {CategoryId}", category.ID);

                // Validate the category
                if (category.ID <= 0)
                {
                    throw new ArgumentException("Invalid category ID");
                }

                if (string.IsNullOrEmpty(category.categoryName))
                {
                    throw new ArgumentException("Category name is required");
                }

                // Update the category
                string updateQuery = "UPDATE Categories SET categoryName = @CategoryName,categoryNameEN = @categoryNameEN, categoryNameAR = @categoryNameAR, image = @image WHERE ID = @ID";
                int result = await _repository.SaveDataAsync(updateQuery, new { ID = category.ID, CategoryName = category.categoryName, categoryNameEN = category.categoryNameEN, categoryNameAR = category.categoryNameAR, image = category.image });

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", category.ID);
                throw;
            }
        }

        /// <summary>
        /// Deletes a category
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Deleting category with ID: {CategoryId}", categoryId);

                // Check if there are questions associated with this category
                string checkQuery = "SELECT COUNT(*) FROM Questions WHERE categoryId = @CategoryId";
                int questionCount = await _repository.ExecuteScalarAsync<int>(checkQuery, new { CategoryId = categoryId });

                if (questionCount > 0)
                {
                    _logger.LogWarning("Cannot delete category with ID {CategoryId} because it has {QuestionCount} associated questions",
                        categoryId, questionCount);
                    throw new InvalidOperationException($"Cannot delete category with ID {categoryId} because it has {questionCount} associated questions");
                }

                // Delete the category
                string deleteQuery = "DELETE FROM Categories WHERE ID = @ID";
                int result = await _repository.SaveDataAsync(deleteQuery, new { ID = categoryId });

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", categoryId);
                throw;
            }
        }

        #endregion

        #region Admin Question Management

        /// <summary>
        /// Creates a new question
        /// </summary>
        public async Task<int> CreateQuestionAsync(Questions question)
        {
            try
            {
                _logger.LogInformation("Creating new question for category ID: {CategoryId}", question.categoryId);

                // Validate the question
                if (string.IsNullOrEmpty(question.questionText) || string.IsNullOrEmpty(question.answerText))
                {
                    throw new ArgumentException("Question text and answer text are required");
                }

                if (question.categoryId <= 0)
                {
                    throw new ArgumentException("Invalid category ID");
                }

                // Verify the category exists
                var categoryExists = await _repository.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Categories WHERE ID = @CategoryId",
                    new { CategoryId = question.categoryId });

                if (categoryExists == 0)
                {
                    throw new ArgumentException($"Category with ID {question.categoryId} does not exist");
                }

                // Insert the question
                string insertQuery = @"
    INSERT INTO Questions (
        questionText, 
        answerText, 
        categoryId, 
        nextStep, 
        pdfPath,
        pdfPathEN,
        pdfPathAR,
        questionTextEN,
        answerTextEN,
        nextStepEN,
        questionTextAR,
        answerTextAR,
        nextStepAR
    ) 
    VALUES (
        @questionText, 
        @answerText, 
        @categoryId, 
        @nextStep, 
        @pdfPath,
        @pdfPathEN,
        @pdfPathAR,
        @questionTextEN,
        @answerTextEN,
        @nextStepEN,
        @questionTextAR,
        @answerTextAR,
        @nextStepAR
    )";

                int newQuestionId = await _repository.InsertReturnIdAsync(insertQuery, new
                {
                    questionText = question.questionText,
                    answerText = question.answerText,
                    categoryId = question.categoryId,
                    nextStep = question.nextStep,
                    pdfPath = question.pdfPath,
                    pdfPathEN = question.pdfPathEN,
                    pdfPathAR = question.pdfPathAR,
                    questionTextEN = question.questionTextEN,
                    answerTextEN = question.answerTextEN,
                    nextStepEN = question.nextStepEN,
                    questionTextAR = question.questionTextAR,
                    answerTextAR = question.answerTextAR,
                    nextStepAR = question.nextStepAR
                });

                return newQuestionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing question
        /// </summary>
        public async Task<bool> UpdateQuestionAsync(Questions question)
        {
            try
            {
                _logger.LogInformation("Updating question with ID: {QuestionId}", question.id);

                // Validate the question
                if (question.id <= 0)
                {
                    throw new ArgumentException("Invalid question ID");
                }

                if (string.IsNullOrEmpty(question.questionText) || string.IsNullOrEmpty(question.answerText))
                {
                    throw new ArgumentException("Question text and answer text are required");
                }

                if (question.categoryId <= 0)
                {
                    throw new ArgumentException("Invalid category ID");
                }

                // Update the question
                string updateQuery = @"
    UPDATE Questions 
    SET questionText = @questionText, 
        answerText = @answerText, 
        categoryId = @categoryId, 
        nextStep = @nextStep, 
        pdfPath = @pdfPath,
        pdfPathEN = @pdfPathEN,
        pdfPathAR = @pdfPathAR,
        questionTextEN = @questionTextEN,
        answerTextEN = @answerTextEN,
        nextStepEN = @nextStepEN,
        questionTextAR = @questionTextAR,
        answerTextAR = @answerTextAR,
        nextStepAR = @nextStepAR
    WHERE Id = @Id";

                int result = await _repository.SaveDataAsync(updateQuery, new
                {
                    Id = question.id,
                    questionText = question.questionText,
                    answerText = question.answerText,
                    categoryId = question.categoryId,
                    nextStep = question.nextStep,
                    pdfPath = question.pdfPath,
                    pdfPathEN = question.pdfPathEN,
                    pdfPathAR = question.pdfPathAR,
                    questionTextEN = question.questionTextEN,
                    answerTextEN = question.answerTextEN,
                    nextStepEN = question.nextStepEN,
                    questionTextAR = question.questionTextAR,
                    answerTextAR = question.answerTextAR,
                    nextStepAR = question.nextStepAR
                });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question with ID: {QuestionId}", question.id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a question
        /// </summary>
        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            try
            {
                _logger.LogInformation("Deleting question with ID: {QuestionId}", questionId);

                string deleteQuery = "DELETE FROM Questions WHERE Id = @Id";
                int result = await _repository.SaveDataAsync(deleteQuery, new { Id = questionId });

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question with ID: {QuestionId}", questionId);
                throw;
            }
        }


        #endregion
    }
}
