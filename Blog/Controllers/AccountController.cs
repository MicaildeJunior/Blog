using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    // Cria um Usuario
    [HttpPost("v1/accounts/")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] EmailService emailService,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        // Cadastrar o Usuario
        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-") // Na URl no lugar do @ ou . fica o -
        };

        var password = PasswordGenerator.Generate(25, includeSpecialChars: true, upperCase: false);
        // senha já está hasheada, encriptada!
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            emailService.Send(
                user.Name,
                user.Email,
                subject: "Bem vindo ao blog!",
                body: $"Sua senha é <strong>{password}</strong>");

            return Ok(new ResultViewModel<dynamic>( new
            { 
                user = user.Email, password
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("05XE15 - Este E-mail já está cadastrado!"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05XE16 - Falha interna no servidor!"));
        }
    }

    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] TokenService tokenService)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        try
        {
            var user = await context.Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);
            // A senha que está no banco está encriptada, tem que rashear ela aqui, se não rashear ela vem como null

            if (user is null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido!"));

            // Agora é o pulo do gato, PasswordHasher.Hash(senha) => Gera um novo Hash("Conjunto de caracteres")
            // Não podemos fazer essa comparação abaixo, pq um hash é gerado toda hora, dessa forma da erro

            /*var hash = PasswordHasher.Hash(model.Password);
            if (hash == user.PasswordHash)*/

            // Verifique se User.PasswordHash(usuario que está salvo no banco) não for igual a senha digitada, return senha inválida
            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido!"));

            // Se passar pelas validações acima cai aqui em baixo

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05XE17 - Falha interna no servidor!"));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>("05XE18 - Falha interna no servidor!"));
        }
    }

    // Edita o Usuario
    [HttpPut("v1/accounts/{id:int}")]
    public async Task<IActionResult> PutAccountAsync(
        [FromRoute] int id,
        [FromBody] EditorAccountViewModel model,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var user = await context.Users
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (user == null)            
                return NotFound(new ResultViewModel<User>("Conteúdo não encontrado"));

            // O que ira modificar, o Id não vai!
            user.Name = model.Name;
            user.Email = model.Email;

            // Chama o usuario do banco, e atualiza o usuario
            context.Users.Update(user);
            // Salva as mudanças
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<User>(user));
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ResultViewModel<User>("05XE17 - Não foi possível alterar o usuario"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<User>("05XE18 - Falha interna no servidor"));
        }
    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context)
    {
        var fileName = $"{Guid.NewGuid().ToString()}.jpg";
        // Receber a imagem do corpo da request
        var data = new Regex(@"^data:image\/[a-z]+;base64,")
            .Replace(model.Base64Image, "");
        // Convertendo para bytes a image
        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("05XE19 - Falha interna no servidor"));
        }

        // Buscar o usuario do banco 
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if (user == null)
            return NotFound(new ResultViewModel<User>("Usuáio não encontrado"));

        user.Image = $"https://localhost:0000/images/{fileName}";

        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>("05XE20 - Falha interna no servidor"));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));
    }
}
