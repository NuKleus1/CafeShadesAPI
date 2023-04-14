using AutoMapper;
using Cafeshades.Models.Dtos;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CafeShades.Controllers
{
    // TODO : Implement Add and Update Requests
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IMapper mapper, IGenericRepository<Category> productRepo, ILogger<CategoryController> logger)
        {
            _mapper = mapper;
            _categoryRepo = productRepo;
            _logger = logger;
        }


        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Category record;
            try
            {
                record = await _categoryRepo.GetByIdAsync(id, includeExpression:null);
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
        public void AddCategory([FromBody] string categoryName, [FromForm] IFormFile imageFile)
        {
            Category category = new Category();
            category.Name = categoryName;

            _categoryRepo.Add(category);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] string categoryName, [FromForm] IFormFile imageFile)
        {
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
