using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthWithAdmin.Models;
using AuthWithAdmin.Server.Data;
using AuthWithAdmin.Server.Models;

namespace AuthWithAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DbRepository _repository;

        public CategoriesController(DbRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<Categories>> GetUser()
        {
            try
            {
                // Get all categories
                string query = "SELECT ID as Id, CategoryName FROM Categories";
                var categories = await _repository.GetRecordsAsync<Categories>(query);

                if (categories == null || !categories.Any())
                    return NotFound("No categories found");

                // For each category, fetch its related questions
                List<Categories> result = new List<Categories>();
                foreach (var category in categories)
                {
                    string questionsQuery = "SELECT ID as Id, questionText, categoryId FROM Questions WHERE categoryId = @CategoryId";
                    var questions = await _repository.GetRecordsAsync<Questions>(questionsQuery, new { CategoryId = category.ID });

                    category.questions = questions.ToList();
                    result.Add(category);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getCategory(int id)
        {
            try
            {
                string query = "SELECT ID, categoryName FROM Categories WHERE ID=@id";
                var categories = await _repository.GetRecordsAsync<Categories>(query, new { id = id });
                var category = categories.FirstOrDefault();
                if (categories == null) return NotFound($"Category with ID '{id}' not found.");

                string questionsQuery = "SELECT id, questionText, answerText, categoryId FROM Questions WHERE categoryId = @categoryId";
                var questions = await _repository.GetRecordsAsync<Questions>(questionsQuery, new { categoryId = id });

                category.questions = questions.ToList();

                return Ok(category);
            } catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category: {ex.Message}");
                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }


        public async Task<IActionResult> GetCategoryQuestions(int id)
        {
            try
            {
                // Get the category
                string categoryQuery = "SELECT ID, categoryName FROM Categories WHERE ID = @Id";
                var categories = await _repository.GetRecordsAsync<Categories>(categoryQuery, new { Id = id });
                var category = categories.FirstOrDefault();

                if (category == null)
                    return NotFound($"Category with ID '{id}' not found");

                // Get questions for this category
                string questionsQuery = "SELECT Id, questionText, answerText, categoryId FROM Questions WHERE categoryId = @CategoryId";
                var questions = await _repository.GetRecordsAsync<Questions>(questionsQuery, new { CategoryId = id });

                // Return both category and its questions
                return Ok(new
                {
                    category = category,
                    questions = questions
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category questions: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





        [HttpPost]
        public async Task<IActionResult> AddCategory(Categories categoryToAdd)
        {
            try
            {
                // Don't include ID in the insert - let SQLite auto-generate it
                string query = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)";

                object newCategoryParam = new
                {
                    CategoryName = categoryToAdd.categoryName
                };

                int newCategoryId = await _repository.InsertReturnIdAsync(query, newCategoryParam);

                if (newCategoryId > 0)
                {
                    // Just fetch the newly created category
                    string categoryQuery = "SELECT ID as Id, CategoryName FROM Categories WHERE ID = @ID";
                    var categoryResult = await _repository.GetRecordsAsync<Categories>(categoryQuery, new { ID = newCategoryId });
                    Categories newCategory = categoryResult.FirstOrDefault();

                    // Initialize empty questions list
                    newCategory.questions = new List<Questions>();

                    return Ok(newCategory);
                }

                return BadRequest("Category not added");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            object param = new
            {
                ID = id
            };

            string delQuery = "DELETE FROM Categories WHERE ID=@ID";
            int rowsAffected = await _repository.SaveDataAsync(delQuery, param);
            if (rowsAffected > 0 )
            {
                return Ok("Category is Deleted successfully");
            } else
            {
                return NotFound("Category is not found, Hence was not deleted");
            }
        }
    }
    }
