using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

LoadConfiguration(app);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

void LoadConfiguration(WebApplication app)
{
    Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
    Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
    Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");

    var smtp = new Configuration.SmtpConfiguration();
    app.Configuration.GetSection("Smtp").Bind(smtp);
    Configuration.Smtp = smtp;
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    // Obtem a chave de Bytes na Configuration
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
    // Esquema de Autenticação padrão do AspNet, AddJwtBearer é a autorização pra usar o Token
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Validar chave de assinatura
            IssuerSigningKey = new SymmetricSecurityKey(key), // Como valida? através de uma chave Simétrica
            ValidateIssuer = false, // Esses dois é false, pq só está usando uma API nesse projeto
            ValidateAudience = false
        };
    });
}

void ConfigureMvc(WebApplicationBuilder builder)
{
    // Abaixo em .ConfigureApiBehaviorOptions, com isso não usa mais o !ModelState.IsValid na controller
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    // Adicionado para disponibilizar para todos os controllers
    builder.Services.AddDbContext<BlogDataContext>();
    builder.Services.AddTransient<TokenService>(); // Sempre cria um novo
    //builder.Services.AddScoped();    // Requisição, enquanto a request durar vai usar o token dela
    //builder.Services.AddSingleton(); // 1 por App, usa sempre a mesma instancia
}