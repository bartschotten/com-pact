using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComPact.ProviderTests.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ComPact.ProviderTests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepo;

        public RecipesController(IRecipeRepository recipeRepo)
        {
            _recipeRepo = recipeRepo;
        }

        // GET api/recipes/acb609ce-c5af-4391-a36f-700ac5ab5e88
        [HttpGet("{id}")]
        public ActionResult<Recipe> Get(string id)
        {
            if (!Guid.TryParse(id, out var guid))
            { 
                return BadRequest($"Id {id} is not a valid Guid");
            }
            var recipe = _recipeRepo.GetById(guid);
            if (recipe == null)
            {
                return BadRequest($"No recipe found with id {id}");
            }
            return Ok(recipe);
        }
    }
}
