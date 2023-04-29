using AutoMapper;
using Cafeshades.Models.Dtos;
using Cafeshades.Models.Request;
using CafeShades.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CafeShades.Controllers
{
    // TODO : Change Responses into ApiResponse Format
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly ILogger<CategoryController> _logger;
        private readonly IGenericRepository<Product> _productRepo;

        public CategoryController(
            IMapper mapper,
            IGenericRepository<Category> categoryRepo,
            ILogger<CategoryController> logger,
            IWebHostEnvironment env,
            IGenericRepository<Product> productRepo)
        {
            _mapper = mapper;
            _categoryRepo = categoryRepo;
            _logger = logger;
            _env = env;
            _productRepo = productRepo;
        }


        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Category record;
            try
            {
                record = await _categoryRepo.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Occurred while Retrieving CAtegory with Id : {id}");
                return BadRequest(new { responseStatus = false, responseMessage = "Error Occurred" });
            }

            if (record == null) return NotFound(new { responseStatus = false, responseMessage = "Not Found!" });

            return Ok(new { responseStatus = true, category = _mapper.Map<CategoryDto>(record) });
        }

        // GET api/<CategoryController>
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Category> record;
            try
            {
                record = await _categoryRepo.ListAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Serverside Error Occurred" });
            }

            if (record.IsNullOrEmpty()) return NotFound(new { responseStatus = false, responseMessage = "No Category Found!" });

            return Ok(new { responseStatus = true, categoryList = _mapper.Map<IEnumerable<CategoryDto>>(record) });
        }

        // POST api/<CategoryController>
        [HttpPost()]
        public async Task<IActionResult> AddCategory([FromForm] CategoryRequest category)
        {
            var cat = await _categoryRepo.ListAllAsync();


            foreach (var item in cat)
                if (item.Name.Equals(category.CategoryName))
                    return Conflict(new ApiResponse("Category Exists!"));


            if (category.ImageFile == null || category.ImageFile.Length <= 0)
                return BadRequest(new { responseStatus = false, responseMessage = "No image file was provided." });

            string fileName;

            try
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(category.ImageFile.FileName);

                string filePath = Path.Combine(_env.WebRootPath, "Category", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    category.ImageFile.CopyTo(stream);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Image");
                return BadRequest(new { responseStatus = false, responseMessage = "Error While saving Image!" });
            }

            // Update the category with the file name
            // ...
            try
            {
                Category newCategory = new Category();
                newCategory.Name = category.CategoryName;
                newCategory.ImageUrl = fileName;

                _categoryRepo.Add(newCategory);
                _categoryRepo.SaveChanges();
            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogError(ex, "Id Error");
                return BadRequest(new { responseStatus = false, responseMessage = "Error Occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unkown Error");
                return BadRequest(new { responseStatus = false, responseMessage = "Unkown Server Error Occurred" });
            }

            return Ok(new { responseStatus = true, responseMessage = "Category Added!" });
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromForm] CategoryRequest category)
        {
            if (category.ImageFile == null || category.ImageFile.Length <= 0)
                return BadRequest(new { responseStatus = false, responseMessage = "No image file was provided." });

            var cat = await _categoryRepo.GetByIdAsync(id);

            if (cat == null)
                return NotFound(new { responseStatus = false, responseMessage = "Category Not found with Id : " + id });

            string oldImagePath = Path.Combine(_env.WebRootPath, "Category", cat.ImageUrl);

            string newImageFileName;
            string newImageFilePath;

            try
            {
                newImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(category.ImageFile.FileName);

                newImageFilePath = Path.Combine(_env.WebRootPath, "Category", newImageFileName);

                using (var stream = new FileStream(newImageFilePath, FileMode.Create))
                    category.ImageFile.CopyTo(stream);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Image");
                return BadRequest(new { responseStatus = false, responseMessage = "Error Occurred" });
            }

            try
            {

                cat.Id = id;
                cat.Name = category.CategoryName;
                cat.ImageUrl = newImageFileName;

                //_categoryRepo.Update(newCategory);
                _categoryRepo.SaveChanges();

                System.IO.File.Replace(oldImagePath, newImageFilePath, cat.Id + "_" + category.CategoryName);

            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogError(ex, "Id Error");
                return BadRequest(new { responseStatus = false, responseMessage = "Error Occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unkown Error");
                return BadRequest(new { responseStatus = false, responseMessage = "Unkown Server Error Occurred" });
            }

            return Ok(new
            {
                responseStatus = true,
                responseMessage = "Category Updated!"
            });
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var record = await _categoryRepo.GetByIdAsync(id);

                if (record == null) return NotFound(new { responseStatus = false, responseMessage = "Category Not Found!" });

                var products = await _productRepo.ListAllAsync(pro => pro.CategoryId == id);

                if (!products.IsNullOrEmpty()) return NotFound(new { responseStatus = false, responseMessage = "There Exists a Product with this category!" });

                _categoryRepo.Delete(record);
                _categoryRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Deleting Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Unkown Server Error Occurred" });
            }
            return Ok(new { responseStatus = true, responseMessage = "Category Deleted!" });
        }


    }
}
