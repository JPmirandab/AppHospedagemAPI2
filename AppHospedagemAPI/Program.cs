using AppHospedagemAPI.Data;
using AppHospedagemAPI.Endpoints;
using AppHospedagemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            // Permitir CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });



            // Add services to the container.System.IO.InvalidDataException: 'Failed to load configuration from file 'C:\JoaoPedro\Projetos\AppHospedagemAPI\AppHospedagemAPI\appsettings.json'.'

            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseCors(policy =>
           policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());


            app.MapClienteEndpoints();
            app.MapQuartoEndpoints();
            app.MapLocacaoEndpoints();
            app.MapOcupacaoEndpoints();
            app.MapUsuarioEndpoints();
            app.MapResumoEndpoints();




            app.MapPost("/login", async (Usuario credenciais, AppDbContext db) =>
            {
                var usuario = await db.Usuarios
                    .FirstOrDefaultAsync(u => u.Login == credenciais.Login && u.Senha == credenciais.Senha);

                if (usuario == null)
                    return Results.Unauthorized();

                return Results.Ok("Login realizado com sucesso");
            });

            app.MapPost("/resetar-dados", async (AppDbContext db) =>
            {
                db.Locacoes.RemoveRange(db.Locacoes);
                db.Clientes.RemoveRange(db.Clientes);
                db.Quartos.RemoveRange(db.Quartos);
                await db.SaveChangesAsync();
                return Results.Ok("Todos os dados foram apagados com sucesso.");
            });

            app.MapPost("/popular-dados", async (AppDbContext db) =>
            {
                var cliente1 = new Cliente { Nome = "João Silva", Telefone = "11999999999" };
                var cliente2 = new Cliente { Nome = "Maria Souza", Telefone = "11988888888" };

                var quarto1 = new Quarto { Numero = 101, QuantidadeCamas = 3, Grupo = "São José" };
                var quarto2 = new Quarto { Numero = 102, QuantidadeCamas = 2, Grupo = "Santa Tereza" };

                db.Clientes.AddRange(cliente1, cliente2);
                db.Quartos.AddRange(quarto1, quarto2);
                await db.SaveChangesAsync();

                var locacao1 = new Locacao
                {
                    ClienteId = cliente1.Id,
                    QuartoId = quarto1.Id,
                    TipoLocacao = "quarto",
                    QuantidadeCamas = 0,
                    DataEntrada = DateTime.Today,
                    DataSaida = DateTime.Today.AddDays(2),
                    Status = "ocupado",
                    CheckInRealizado = true,
                    CheckOutRealizado = false
                };

                var locacao2 = new Locacao
                {
                    ClienteId = cliente2.Id,
                    QuartoId = quarto2.Id,
                    TipoLocacao = "cama",
                    QuantidadeCamas = 1,
                    DataEntrada = DateTime.Today.AddDays(1),
                    DataSaida = DateTime.Today.AddDays(3),
                    Status = "reservado",
                    CheckInRealizado = false,
                    CheckOutRealizado = false
                };

                db.Locacoes.AddRange(locacao1, locacao2);
                await db.SaveChangesAsync();

                return Results.Ok("Dados de teste adicionados com sucesso.");
            });



            app.Run();
        }
    }
}
