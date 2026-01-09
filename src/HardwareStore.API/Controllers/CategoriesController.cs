using HardwareStore.Application.Categories;
using HardwareStore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });

            return Ok(categoryDtos);
        }
    }
}
