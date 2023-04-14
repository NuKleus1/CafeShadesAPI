using AutoMapper;
using CafeShades.Models.Dtos;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CafeShades.Controllers
{
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

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CategoryController>
        [HttpPost]
        public void PostCategory([FromBody] CategoryDto categoryDto)
        {

        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public void PutCategory(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public void DeleteCategory(int id)
        {

        }
    }
}
