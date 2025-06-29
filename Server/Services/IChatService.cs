using AuthWithAdmin.Models;
using AuthWithAdmin.Models.Bot;
using AuthWithAdmin.Server.Models;
using AuthWithAdmin.Server.Models.Bot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthWithAdmin.Server.Services
{
    public interface IChatService
    {
        // Public methods for retrieving chat data
        Task<ChatDataModel> GetAllPublicChatDataAsync();
        Task<BotStates> GetStateByIdAsync(int stateId);
        Task<Categories> GetCategoryByIdAsync(int categoryId);
        Task<Questions> GetQuestionByIdAsync(int questionId);
        Task<List<Questions>> GetQuestionsByCategoryAsync(int categoryId);

        // Admin methods for managing bot states
        Task<int> CreateStateAsync(BotStates state);
        Task<bool> UpdateStateAsync(BotStates state);
        Task<bool> DeleteStateAsync(int stateId);

        // Admin methods for managing bot options
        Task<int> CreateOptionAsync(BotOptions option);
        Task<bool> UpdateOptionAsync(BotOptions option);
        Task<bool> DeleteOptionAsync(int optionId);
        Task<bool> DeleteOptionsForStateAsync(int stateId);

        // Admin methods for managing categories
        Task<int> CreateCategoryAsync(Categories category);
        Task<bool> UpdateCategoryAsync(Categories category);
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Admin methods for managing questions
        Task<int> CreateQuestionAsync(Questions question);
        Task<bool> UpdateQuestionAsync(Questions question);
        Task<bool> DeleteQuestionAsync(int questionId);
    }
}