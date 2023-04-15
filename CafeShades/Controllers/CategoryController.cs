using AutoMapper;
using Cafeshades.Models.Dtos;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CafeShades.Controllers
{
    // TODO : Implement Add and Update Requests
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            IMapper mapper,
            IGenericRepository<Category> productRepo,
            ILogger<CategoryController> logger,
            IWebHostEnvironment env)
        {
            _mapper = mapper;
            _categoryRepo = productRepo;
            _logger = logger;
            _env = env;
        }


        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Category record;
            try
            {
                record = await _categoryRepo.GetByIdAsync(id, includeExpression: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Product");
                return BadRequest();
            }

            if (record == null) return NotFound();

            return Ok(_mapper.Map<CategoryDto>(record));
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
                _logger.LogError(ex, "Error Occured while Deleting Product");
                return BadRequest();
            }

            if (record.IsNullOrEmpty()) return NotFound();

            return Ok(_mapper.Map<IEnumerable<CategoryDto>>(record));
        }

        // POST api/<CategoryController>
        [HttpPost()]
        public IActionResult AddCategory([FromBody] string categoryName, [FromForm] IFormFile imageFile)
        {

            if (imageFile == null || imageFile.Length <= 0)
                return BadRequest("No image file was provided.");

            string fileName;
            
            try
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                string filePath = Path.Combine(_env.WebRootPath, "Category", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    imageFile.CopyTo(stream);

            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Image");
                return BadRequest();
            }

            // Update the category with the file name
            // ...
            try
            {
                Category category = new Category();
                category.Name = categoryName;
                category.ImageUrl = fileName;

                _categoryRepo.AddAsync(category);
            }catch(DBConcurrencyException ex)
            {
                _logger.LogError(ex, "Id Error");
                return BadRequest("Id Error");
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Unkown Error");
                return BadRequest();
            }

            return Ok();
        }


        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] string categoryName, [FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length <= 0)
                return BadRequest("No image file was provided.");

            var cat = await _categoryRepo.GetByIdAsync(id);

            if (cat == null)
                return NotFound("Category Not found with Id : " + id);

            string oldImagePath = Path.Combine(_env.WebRootPath, "Category", cat.ImageUrl);

            string newImageFileName;
            string newImageFilePath;

            try
            {
                newImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                newImageFilePath = Path.Combine(_env.WebRootPath, "Category", newImageFileName);

                using (var stream = new FileStream(newImageFilePath, FileMode.Create))
                    imageFile.CopyTo(stream);

            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Image");
                return BadRequest();
            }

            try
            {
                Category category = new Category();
                category.Id = id;
                category.Name = categoryName;
                category.ImageUrl = newImageFileName;

                _categoryRepo.Update(category);
                _categoryRepo.SaveChanges();

                System.IO.File.Replace(oldImagePath, newImageFilePath, category.Id+"_"+categoryName);

            }catch (DBConcurrencyException ex)
            {
                _logger.LogError(ex, "Id Error");
                return BadRequest("Id Error");
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Unkown Error");
                return BadRequest();
            }

            return Ok();
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var record = await _categoryRepo.GetByIdAsync(id);

                if (record == null) return NotFound();

                _categoryRepo.Delete(record);
                _categoryRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Product");
                return BadRequest();
            }
            return Ok();
        }


    }
}
