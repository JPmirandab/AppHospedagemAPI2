using AppHospedagemAPI.Models;
using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using AppHospedagemAPI.DTOs; // Importe os novos DTOs

namespace AppHospedagemAPI.Endpoints
{
    public static class QuartoEndpoints
    {
        public static void MapQuartoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/quartos")
                .WithTags("Quartos")
                .RequireAuthorization(); // Autorização padrão para todos os endpoints de quarto

            // 📋 Listar quartos com filtros
            group.MapGet("/", async (
                [FromQuery] string? grupo,
                [FromQuery] int? capacidadeMinima,
                [FromQuery] bool? disponivel,
                AppDbContext db) =>
            {
                var query = db.Quartos.AsQueryable();

                if (!string.IsNullOrEmpty(grupo))
                {
                    query = query.Where(q => q.Grupo == grupo);
                }

                if (capacidadeMinima.HasValue)
                {
                    query = query.Where(q => q.QuantidadeCamas >= capacidadeMinima.Value);
                }

                // Lógica de disponibilidade aprimorada para usar a propriedade EstaOcupado
                // Carrega Locacoes apenas se o filtro 'disponivel' for usado
                if (disponivel.HasValue)
                {
                    query = query.Include(q => q.Locacoes); // Carrega as locações para usar EstaOcupado

                    query = disponivel.Value
                        ? query.Where(q => !q.EstaOcupado) // Filtrar por quartos NÃO ocupados
                        : query.Where(q => q.EstaOcupado); // Filtrar por quartos ocupados
                }

                var quartos = await query.OrderBy(q => q.Numero).ToListAsync();

                // Projeta para QuartoResponse
                return Results.Ok(quartos.Select(q => new QuartoResponse
                {
                    Id = q.Id,
                    Numero = q.Numero,
                    QuantidadeCamas = q.QuantidadeCamas,
                    Grupo = q.Grupo,
                    EstaOcupado = q.EstaOcupado // Usa a propriedade calculada do modelo
                }));
            })
            .WithSummary("Listar quartos com filtros")
            .WithDescription("Filtra quartos por grupo, capacidade mínima e status de disponibilidade.")
            .Produces<IEnumerable<QuartoResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

            // 🔍 Obter detalhes de um quarto (incluindo status de ocupação)
            group.MapGet("/{id}", async (int id, AppDbContext db) =>
            {
                var quarto = await db.Quartos
                    .Include(q => q.Locacoes) // Inclui as locações para que 'EstaOcupado' funcione
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quarto == null)
                    return Results.NotFound("Quarto não encontrado.");

                // Retorna QuartoResponse, com EstaOcupado
                return Results.Ok(new QuartoResponse
                {
                    Id = quarto.Id,
                    Numero = quarto.Numero,
                    QuantidadeCamas = quarto.QuantidadeCamas,
                    Grupo = quarto.Grupo,
                    EstaOcupado = quarto.EstaOcupado
                });
            })
            .WithSummary("Obtém detalhes de um quarto")
            .Produces<QuartoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);


            // ➕ Cadastrar novo quarto (apenas admin)
            group.MapPost("/", async (
                [FromBody] QuartoCreateRequest request, // Usando DTO de criação
                AppDbContext db) =>
            {
                // A validação das Data Annotations em QuartoCreateRequest é automática.

                // Validação de negócio: Unicidade do número do quarto
                if (await db.Quartos.AnyAsync(q => q.Numero == request.Numero))
                    return Results.BadRequest("Já existe um quarto com este número.");

                // Validação CustomValidation do Grupo (executada automaticamente pelo pipeline)
                // Opcional: Chamar Validator.TryValidateObject(quarto, ...) se a validação CustomValidation for crítica aqui antes de salvar.
                // Mas, como está no modelo, ela será validada se você tiver um ModelState/ValidationFilter.
                // Para Minimal API, se você não tem um Filter explicitamente, o Validator.TryValidateObject é útil.
                // Mas geralmente, ao salvar, as Data Annotations no modelo são disparadas.

                var quarto = new Quarto
                {
                    Numero = request.Numero,
                    QuantidadeCamas = request.QuantidadeCamas,
                    Grupo = request.Grupo
                };

                db.Quartos.Add(quarto);
                await db.SaveChangesAsync();

                // Retorna QuartoResponse do quarto criado
                return Results.Created($"/quartos/{quarto.Id}", new QuartoResponse
                {
                    Id = quarto.Id,
                    Numero = quarto.Numero,
                    QuantidadeCamas = quarto.QuantidadeCamas,
                    Grupo = quarto.Grupo,
                    EstaOcupado = quarto.EstaOcupado // Será false para um quarto recém-criado
                });
            })
            .RequireAuthorization("admin") // Apenas admin pode criar quartos
            .WithSummary("Cadastra um novo quarto")
            .Produces<QuartoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


            // ✏️ Atualizar quarto (apenas admin)
            group.MapPut("/{id}", async (
                int id,
                [FromBody] QuartoUpdateRequest request, // Usando DTO de atualização
                AppDbContext db) =>
            {
                // A validação das Data Annotations em QuartoUpdateRequest é automática.

                var quarto = await db.Quartos.FindAsync(id);
                if (quarto == null)
                    return Results.NotFound("Quarto não encontrado para atualização.");

                // Validação de negócio: Unicidade do número do quarto (exclui o próprio quarto)
                if (await db.Quartos.AnyAsync(q => q.Numero == request.Numero && q.Id != id))
                    return Results.BadRequest("Já existe outro quarto com este número.");
                
                // Validação CustomValidation do Grupo (semelhante ao POST)

                quarto.Numero = request.Numero;
                quarto.QuantidadeCamas = request.QuantidadeCamas;
                quarto.Grupo = request.Grupo;

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .RequireAuthorization("admin") // Apenas admin pode atualizar quartos
            .WithSummary("Atualiza os dados de um quarto existente")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


            // ❌ Remover quarto (apenas admin)
            group.MapDelete("/{id}", async (int id, AppDbContext db) =>
            {
                var quarto = await db.Quartos
                    .Include(q => q.Locacoes) // Inclui locações para verificar se há locações ativas
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quarto == null)
                    return Results.NotFound("Quarto não encontrado para exclusão.");

                // Validação de negócio: Não permitir excluir quarto com locações futuras ou ativas
                if (quarto.Locacoes?.Any(l => l.DataSaida >= DateTime.Today) ?? false)
                    return Results.BadRequest("Não é possível excluir quarto com locações futuras ou ativas.");

                db.Quartos.Remove(quarto);
                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .RequireAuthorization("admin") // Apenas admin pode remover quartos
            .WithSummary("Exclui um quarto existente")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

            // Endpoint para QuartoOcupacaoDTO
            // Este é um endpoint separado para obter o status de ocupação para um quarto específico
            group.MapGet("/ocupacao/{id}", async (int id, AppDbContext db) =>
            {
                var quarto = await db.Quartos
                    .Include(q => q.Locacoes) // Garante que as locações sejam carregadas
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quarto == null)
                {
                    return Results.NotFound("Quarto não encontrado.");
                }

                // Mapeia para QuartoOcupacaoDTO
                var ocupacaoDto = new QuartoOcupacaoDTO
                {
                    Numero = quarto.Numero,
                    Grupo = quarto.Grupo,
                    TotalCamas = quarto.QuantidadeCamas,
                    CamasOcupadas = quarto.Locacoes?
                                        .Where(l => l.DataEntrada <= DateTime.Today && l.DataSaida >= DateTime.Today && l.TipoLocacao == "cama")
                                        .Sum(l => l.QuantidadeCamas) ?? 0, // Soma camas ocupadas apenas se for tipo "cama"
                    // O status aqui pode ser derivado da propriedade EstaOcupado ou ser mais granular
                    Status = quarto.EstaOcupado ? "Ocupado" : "Livre"
                };

                // Um quarto pode estar parcialmente ocupado se TipoLocacao for "cama" e CamasOcupadas < TotalCamas
                if (!quarto.EstaOcupado && ocupacaoDto.CamasOcupadas > 0 && ocupacaoDto.CamasOcupadas < ocupacaoDto.TotalCamas)
                {
                    ocupacaoDto.Status = "Parcialmente Ocupado";
                }
                else if (ocupacaoDto.CamasOcupadas == ocupacaoDto.TotalCamas)
                {
                     ocupacaoDto.Status = "Totalmente Ocupado";
                }
                else if (!quarto.EstaOcupado && ocupacaoDto.CamasOcupadas == 0)
                {
                    ocupacaoDto.Status = "Livre";
                }


                return Results.Ok(ocupacaoDto);
            })
            .WithSummary("Obtém o status de ocupação detalhado de um quarto")
            .Produces<QuartoOcupacaoDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}