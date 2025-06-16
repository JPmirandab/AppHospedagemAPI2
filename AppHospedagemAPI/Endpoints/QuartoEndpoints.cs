//using AppHospedagemAPI.Models;
//using AppHospedagemAPI.Data;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Mvc;

//namespace AppHospedagemAPI.Endpoints
//{
//    public static class QuartoEndpoints
//    {
//        public static void MapQuartoEndpoints(this WebApplication app)
//        {
//            // 👉 Cadastrar um novo quarto
//            app.MapPost("/quartos", async (Quarto quarto, AppDbContext db) =>
//            {
//                db.Quartos.Add(quarto);
//                await db.SaveChangesAsync();
//                return Results.Created($"/quartos/{quarto.Id}", quarto);
//            });

//            // ✅ Listar quartos com ou sem filtro (único GET /quartos)
//            //    Permite usar: /quartos, /quartos?grupo=São José, /quartos?status=ocupado
//            app.MapGet("/quartos", async (
//                [FromQuery] string? grupo,
//                [FromQuery] string? status,
//                AppDbContext db) =>
//            {
//                // Consulta inicial com as locações incluídas
//                var query = db.Quartos
//                    .Include(q => q.Locacoes)
//                    .AsQueryable();

//                // Filtra por grupo, se informado
//                if (!string.IsNullOrEmpty(grupo))
//                {
//                    query = query.Where(q => q.Grupo == grupo);
//                }

//                var quartos = await query.ToListAsync();

//                // Mapeia o resultado para incluir o status calculado
//                var resultado = quartos.Select(q =>
//                {
//                    // Ocupações ativas: locações com status reservado ou ocupado
//                    var ocupacoesAtivas = q.Locacoes
//                        .Where(l => l.Status == "ocupado" || l.Status == "reservado")
//                        .ToList();

//                    // Status do quarto (disponível ou ocupado)
//                    string statusQuarto = ocupacoesAtivas.Any() ? "ocupado" : "disponível";

//                    return new
//                    {
//                        q.Id,
//                        q.Numero,
//                        q.QuantidadeCamas,
//                        q.Grupo,
//                        Status = statusQuarto
//                    };
//                });

//                // Filtra por status, se informado
//                if (!string.IsNullOrEmpty(status))
//                {
//                    resultado = resultado.Where(q => q.Status == status);
//                }

//                return Results.Ok(resultado);
//            });

//            // 👉 Atualizar um quarto existente
//            app.MapPut("/quartos/{id}", async (int id, Quarto input, AppDbContext db) =>
//            {
//                var quarto = await db.Quartos.FindAsync(id);

//                if (quarto is null)
//                    return Results.NotFound();

//                quarto.Numero = input.Numero;
//                quarto.QuantidadeCamas = input.QuantidadeCamas;
//                quarto.Grupo = input.Grupo;

//                await db.SaveChangesAsync();
//                return Results.Ok(quarto);
//            });

//            // 👉 Deletar um quarto
//            app.MapDelete("/quartos/{id}", async (int id, AppDbContext db) =>
//            {
//                var quarto = await db.Quartos.FindAsync(id);

//                if (quarto is null)
//                    return Results.NotFound();

//                db.Quartos.Remove(quarto);
//                await db.SaveChangesAsync();
//                return Results.NoContent();
//            });
//        }
//    }
//}


using AppHospedagemAPI.Models;
using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace AppHospedagemAPI.Endpoints
{
    public static class QuartoEndpoints
    {
        public static void MapQuartoEndpoints(this WebApplication app)
        {
            app.MapGet("/quartos", async (
    [FromQuery] string? grupo,
    [FromQuery] bool? disponivel,
    AppDbContext db) =>
            {
                var hoje = DateTime.Today;

                // Obter lista de quartos ocupados
                var quartosOcupados = await db.Locacoes
                    .Where(l => l.DataEntrada <= hoje &&
                               l.DataSaida >= hoje &&
                               l.Status == "Ativa")
                    .Select(l => l.QuartoId)
                    .Distinct()
                    .ToListAsync();

                var query = db.Quartos.AsQueryable();

                if (!string.IsNullOrEmpty(grupo))
                {
                    query = query.Where(q => q.Grupo == grupo);
                }

                if (disponivel.HasValue)
                {
                    query = disponivel.Value
                        ? query.Where(q => !quartosOcupados.Contains(q.Id))
                        : query.Where(q => quartosOcupados.Contains(q.Id));
                }

                // Projetar o resultado incluindo a informação de disponibilidade
                var result = await query
                    .Select(q => new
                    {
                        q.Id,
                        q.Numero,
                        q.QuantidadeCamas,
                        q.Grupo,
                        Disponivel = !quartosOcupados.Contains(q.Id)
                    })
                    .ToListAsync();

                return result;
            });

            // Endpoint para obter um quarto específico
            app.MapGet("/quartos/{id}", async (int id, AppDbContext db) =>
            {
                var quarto = await db.Quartos.FindAsync(id);
                if (quarto == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(quarto);
            });


            app.MapPost("/quartos", async ([FromBody] Quarto quarto, AppDbContext db) =>
            {
                // Validação básica do objeto
                if (quarto == null)
                    return Results.BadRequest("Dados do quarto inválidos");

                // Validação do número do quarto (agora como int)
                if (quarto.Numero <= 0)
                    return Results.BadRequest("Número do quarto deve ser positivo");

                // Validação da quantidade de camas
                if (quarto.QuantidadeCamas <= 0)
                    return Results.BadRequest("Quantidade de camas deve ser maior que zero");

                // Validação do grupo
                if (string.IsNullOrWhiteSpace(quarto.Grupo))
                    return Results.BadRequest("Grupo é obrigatório");

                // Verificar duplicação
                if (await db.Quartos.AnyAsync(q => q.Numero == quarto.Numero))
                    return Results.BadRequest($"Já existe um quarto com o número {quarto.Numero}");

                try
                {
                    db.Quartos.Add(quarto);
                    await db.SaveChangesAsync();

                    return Results.Created($"/quartos/{quarto.Id}", quarto);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Erro ao criar quarto: {ex.Message}");
                }
            });

            app.MapPut("/quartos/{id}", async (int id, [FromBody] Quarto inputQuarto, AppDbContext db) =>
            {
                // Validar ID
                if (id <= 0)
                    return Results.BadRequest("ID inválido");

                // Encontrar quarto existente
                var quarto = await db.Quartos.FindAsync(id);
                if (quarto == null)
                    return Results.NotFound($"Quarto com ID {id} não encontrado");

                // Validar número
                if (inputQuarto.Numero <= 0)
                    return Results.BadRequest("Número do quarto deve ser positivo");

                // Verificar se número está sendo alterado
                if (quarto.Numero != inputQuarto.Numero)
                {
                    // Verificar duplicação
                    if (await db.Quartos.AnyAsync(q => q.Numero == inputQuarto.Numero))
                        return Results.BadRequest($"Já existe um quarto com o número {inputQuarto.Numero}");
                }

                // Atualizar propriedades
                quarto.Numero = inputQuarto.Numero;
                quarto.QuantidadeCamas = inputQuarto.QuantidadeCamas;
                quarto.Grupo = inputQuarto.Grupo;

                try
                {
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Erro ao atualizar quarto: {ex.Message}");
                }
            });

            app.MapDelete("/quartos/{id}", async (int id, AppDbContext db) =>
            {
                // Verificar se o ID é válido
                if (id <= 0)
                    return Results.BadRequest("ID inválido");

                // Encontrar o quarto
                var quarto = await db.Quartos
                    .Include(q => q.Locacoes) // Inclui as locações relacionadas
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quarto == null)
                    return Results.NotFound($"Quarto com ID {id} não encontrado");

                // Verificar se há locações ativas
                var hoje = DateTime.Today;
                var temLocacoesAtivas = quarto.Locacoes.Any(l =>
                    l.DataEntrada <= hoje &&
                    l.DataSaida >= hoje &&
                    l.Status == "Ativa");

                if (temLocacoesAtivas)
                    return Results.BadRequest("Não é possível excluir quarto com locações ativas");

                try
                {
                    db.Quartos.Remove(quarto);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Erro ao excluir quarto: {ex.Message}");
                }
            });
        }
    }
}