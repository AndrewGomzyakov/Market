using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Market.Context;
using Market.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers
{
    [Route("Category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet("GetRegistry")]
        public async Task<IActionResult> GetRegistry()
        {
            var categories = await _categoryService.GetCategories();
            return View(categories);
        }

        [HttpGet("Save")]
        public IActionResult  Save()
        {
            return View();
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save(Guid id, string name)
        {
            var file = Request.Form.Files.FirstOrDefault();
            byte[] imageData = null;
            if (file != null)
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)file.Length);
                }

            await _categoryService.SaveCategory(new DbCategory
            {
                Id = id,
                Image = imageData,
                Name = name
            });
            return Redirect("/Category/GetRegistry");
        }

        [HttpGet("Delete/{categoryId}")]
        public async Task<IActionResult> Delete(Guid categoryId)
        {
            await _categoryService.DeleteCategory(categoryId);
            return Redirect("/Category/GetRegistry");
        }
    }
}