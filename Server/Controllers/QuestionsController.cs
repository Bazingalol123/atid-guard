using AuthWithAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthWithAdmin.Server.Data;

namespace AuthWithAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly DbRepository _repository;

        public QuestionsController(DbRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Questions>>> GetAllQuestions()
        {
            try
            {
                string query = "SELECT Id, questionText, answerText, categoryId FROM Questions";
                var questions = await _repository.GetRecordsAsync<Questions>(query);

                if (questions == null || !questions.Any())
                    return NotFound("No questions found");

                return Ok(questions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching questions: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Questions>> GetQuestion(int id)
        {
            try
            {
                string query = "SELECT Id, questionText, answerText, categoryId FROM Questions WHERE Id = @id";
                var questions = await _repository.GetRecordsAsync<Questions>(query, new { id = id });

                var question = questions.FirstOrDefault();
                if (question == null)
                    return NotFound($"Question with ID '{id}' not found");

                return Ok(question);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching question: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("nextSteps/{questionId}")]
        public async Task<IActionResult> GetNextSteps(int questionId)
        {
            string query = "SELECT nextStep FROM Questions WHERE ID = @questionId";
            var nextSteps = await _repository.ExecuteScalarAsync<string>(query, new { questionId = questionId });
            if (nextSteps == null)
            {
                return NotFound("Next steps not found for this question.");
            }
            
            return Ok(nextSteps);  

        }

        // GET: api/Questions/Category/1
        [HttpGet("Category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Questions>>> GetQuestionsByCategory(int categoryId)
        {
            try
            {
                string query = "SELECT Id, questionText, answerText, categoryId FROM Questions WHERE categoryId = @CategoryId";
                var questions = await _repository.GetRecordsAsync<Questions>(query, new { CategoryId = categoryId });

                if (questions == null || !questions.Any())
                    return NotFound($"No questions found for category ID '{categoryId}'");

                return Ok(questions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching questions by category: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Questions
        [HttpPost]
        public async Task<IActionResult> AddQuestion(Questions questionToAdd)
        {
            try
            {
                string query = "INSERT INTO Questions (questionText, answerText, categoryId) VALUES (@questionText, @answerText, @categoryId)";

                object parameters = new
                {
                    questionText = questionToAdd.questionText,
                    answerText = questionToAdd.answerText,
                    categoryId = questionToAdd.categoryId
                };

                int newQuestionId = await _repository.InsertReturnIdAsync(query, parameters);

                if (newQuestionId > 0)
                {
                    string getQuery = "SELECT Id, questionText, answerText, categoryId FROM Questions WHERE Id = @Id";
                    var newQuestion = await _repository.GetRecordsAsync<Questions>(getQuery, new { Id = newQuestionId });
                    return Ok(newQuestion.FirstOrDefault());
                }

                return BadRequest("Question not added");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding question: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Questions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, Questions questionToUpdate)
        {
            try
            {
                if (id != questionToUpdate.id)
                {
                    return BadRequest("ID mismatch");
                }

                string checkQuery = "SELECT COUNT(1) FROM Questions WHERE Id = @Id";
                int count = await _repository.ExecuteScalarAsync<int>(checkQuery, new { Id = id });

                if (count == 0)
                {
                    return NotFound($"Question with ID '{id}' not found");
                }

                string updateQuery = "UPDATE Questions SET questionText = @questionText, answerText = @answerText, categoryId = @categoryId WHERE Id = @Id";
                await _repository.SaveDataAsync(updateQuery, new
                {
                    Id = id,
                    questionText = questionToUpdate.questionText,
                    answerText = questionToUpdate.answerText,
                    categoryId = questionToUpdate.categoryId
                });

                return Ok("Question updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating question: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Questions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                string deleteQuery = "DELETE FROM Questions WHERE Id = @Id";
                int rowsAffected = await _repository.SaveDataAsync(deleteQuery, new { Id = id });

                if (rowsAffected > 0)
                {
                    return Ok("Question deleted successfully");
                }
                else
                {
                    return NotFound($"Question with ID '{id}' not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting question: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
