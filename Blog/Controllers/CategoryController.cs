using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers;

[ApiController]
public class CategoryController : ControllerBase
{
    [HttpGet("v1/categories")]
    public async Task<IActionResult> GetCategoryAsync(
        [FromServices] IMemoryCache cache,
        [FromServices] BlogDataContext context)
    {        
        try
        {
            var categories = cache.GetOrCreate("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetCategories(context);
            });

            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<List<Category>>("05XE7 - Falha interna no servidor"));
        }
    }

    private List<Category> GetCategories(BlogDataContext context)
    {
        return context.Categories.ToList();
    }

    [HttpGet("v1/categories/{id:int}")]
    public async Task<IActionResult> GetCategoryByIdAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            // Obtem a categoria do banco        
            var category = await context.Categories
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            return Ok(new ResultViewModel<Category>(category));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE8 - Falha interna no servidor"));
        }
    }

    // Cadastrar Category
    [HttpPost("v1/categories")]
    public async Task<IActionResult> PostCategoryAsync(
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        // O AspNet ja faz essa validação, quando faz REQUIRED, Data Anotations na Model, então não precisa por
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
        try
        {
            // Cria um novo obj category, o Id na criação tem q começar com 0
            var category = new Category
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower()
            };

            // Adiciona uma Category       
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            // Retornar um Created é só pra POST
            return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE9 - Não foi possível incluir a categoria"));
        }
        catch 
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE10 - Falha interna no servidor"));
        }
    }

    // Atualizar Category
    [HttpPut("v1/categories/{id:int}")]
    public async Task<IActionResult> PutCategoryAsync(
        [FromRoute] int id,
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            // O que ira modificar, o Id não vai!
            category.Name = model.Name;
            category.Slug = model.Slug;

            // Chama a category do banco, e atualiza category
            context.Categories.Update(category);
            // Salva as mudanças
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE11 - Não foi possível alterar a categoria")); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE12 - Falha interna no servidor"));
        }
    }

    // Deletar Category
    [HttpDelete("v1/categories/{id:int}")]
    public async Task<IActionResult> DeleteCategoryAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            // Obtem a categoria do banco, procura pelo id
            var category = await context.Categories
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE13 - Não foi possível excluir a categoria"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE14 - Falha interna no servidor"));
        }
    }
}  