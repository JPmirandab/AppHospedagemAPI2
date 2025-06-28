using AppHospedagemAPI.Models;
using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using AppHospedagemAPI.DTOs;
using System.Security.Claims;

namespace AppHospedagemAPI.Endpoints
{
    public static class LocacaoEndpoints
    {
        public static void MapLocacaoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/locacoes")
                .WithTags("Locações")
                .RequireAuthorization();

            // ➕ Criar nova locação (reserva)
            group.MapPost("/", async (
                [FromBody] LocacaoCreateRequest request,
                ClaimsPrincipal user,
                AppDbContext db) =>
            {
                // A validação das Data Annotations em LocacaoCreateRequest é automática.

                // Validação de existência de Cliente e Quarto
                var cliente = await db.Clientes.FindAsync(request.ClienteId);
                if (cliente == null) return Results.BadRequest("Cliente não encontrado.");

                var quarto = await db.Quartos.FindAsync(request.QuartoId);
                if (quarto == null) return Results.BadRequest("Quarto não encontrado.");

                // Validação de regras de negócio para disponibilidade
                var conflitos = await db.Locacoes
                    .Where(l => l.QuartoId == request.QuartoId &&
                                l.Status != "finalizado" &&
                                l.DataEntrada < request.DataSaida &&
                                l.DataSaida > l.DataEntrada)
                    .ToListAsync();

                // 1. Verificar conflito para locação de quarto inteiro
                if (request.TipoLocacao == "quarto")
                {
                    if (conflitos.Any())
                    {
                        return Results.BadRequest("Quarto já reservado para o período selecionado.");
                    }
                    request.QuantidadeCamas = quarto.QuantidadeCamas;
                }
                // 2. Verificar disponibilidade de camas na locação por cama
                else if (request.TipoLocacao == "cama")
                {
                    var camasOcupadasConflito = conflitos
                        .Sum(c => c.TipoLocacao == "quarto" ? quarto.QuantidadeCamas : c.QuantidadeCamas);

                    if (camasOcupadasConflito + request.QuantidadeCamas > quarto.QuantidadeCamas)
                    {
                        return Results.BadRequest("Não há camas disponíveis suficientes para este quarto no período.");
                    }
                    if (request.QuantidadeCamas == null || request.QuantidadeCamas <= 0)
                    {
                        return Results.BadRequest("Quantidade de camas deve ser informada e maior que zero para locação por cama.");
                    }
                }
                else
                {
                    return Results.BadRequest("Tipo de locação inválido. Deve ser 'quarto' ou 'cama'.");
                }

                // Obter o ID do usuário logado
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var usuarioId))
                {
                    return Results.Unauthorized();
                }

                // Cálculo do preço total REMOVIDO


                // Criar locação
                var locacao = new Locacao
                {
                    ClienteId = request.ClienteId,
                    QuartoId = request.QuartoId,
                    DataEntrada = request.DataEntrada.Date,
                    DataSaida = request.DataSaida.Date,
                    TipoLocacao = request.TipoLocacao,
                    QuantidadeCamas = request.QuantidadeCamas ?? 0,
                    Status = "reservado",
                    CheckInRealizado = false,
                    CheckOutRealizado = false,
                    // PrecoTotal REMOVIDO
                    UsuarioId = usuarioId
                };

                db.Locacoes.Add(locacao);
                await db.SaveChangesAsync();

                // Retorna DTO de resposta completo
                return Results.Created($"/locacoes/{locacao.Id}", new LocacaoResponse
                {
                    Id = locacao.Id,
                    ClienteId = locacao.ClienteId,
                    ClienteNome = cliente.Nome,
                    QuartoId = locacao.QuartoId,
                    QuartoNumero = quarto.Numero,
                    DataEntrada = locacao.DataEntrada,
                    DataSaida = locacao.DataSaida,
                    TipoLocacao = locacao.TipoLocacao,
                    QuantidadeCamas = locacao.QuantidadeCamas,
                    Status = locacao.Status,
                    CheckInRealizado = locacao.CheckInRealizado,
                    CheckOutRealizado = locacao.CheckOutRealizado,
                    // PrecoTotal REMOVIDO
                    UsuarioResponsavelLogin = user.Identity?.Name
                });
            })
            .RequireAuthorization("admin", "funcionario")
            .WithSummary("Cria uma nova locação (reserva) para um quarto ou cama.")
            .Produces<LocacaoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


            // 🟢 Realizar check-in
            group.MapPost("/checkin/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes.FindAsync(id);
                if (locacao == null)
                    return Results.NotFound("Locação não encontrada.");

                if (locacao.CheckInRealizado)
                    return Results.BadRequest("Check-in já foi realizado para esta locação.");

                if (locacao.Status != "reservado")
                    return Results.BadRequest($"Não é possível realizar check-in para locação com status '{locacao.Status}'.");

                locacao.CheckInRealizado = true;
                locacao.Status = "ativo";
                await db.SaveChangesAsync();

                return Results.Ok("Check-in realizado com sucesso.");
            })
            .RequireAuthorization("admin", "funcionario")
            .WithSummary("Realiza o check-in de uma locação.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


            // 🔴 Realizar check-out
            group.MapPost("/checkout/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes.FindAsync(id);
                if (locacao == null)
                    return Results.NotFound("Locação não encontrada.");

                if (!locacao.CheckInRealizado)
                    return Results.BadRequest("Check-in ainda não foi realizado para esta locação.");

                if (locacao.CheckOutRealizado)
                    return Results.BadRequest("Check-out já foi realizado para esta locação.");

                if (locacao.Status != "ativo")
                    return Results.BadRequest($"Não é possível realizar check-out para locação com status '{locacao.Status}'.");

                locacao.CheckOutRealizado = true;
                locacao.Status = "finalizado";
                await db.SaveChangesAsync();

                return Results.Ok("Check-out realizado com sucesso.");
            })
            .RequireAuthorization("admin", "funcionario")
            .WithSummary("Realiza o check-out de uma locação.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


            // 📋 Listar todas as locações com filtros e detalhes
            group.MapGet("/", async (
                [FromQuery] int? clienteId,
                [FromQuery] int? quartoId,
                [FromQuery] string? status,
                [FromQuery] DateTime? dataEntradaMin,
                [FromQuery] DateTime? dataSaidaMax,
                AppDbContext db) =>
            {
                var query = db.Locacoes
                    .Include(l => l.Cliente)
                    .Include(l => l.Quarto)
                    .Include(l => l.Usuario)
                    .AsQueryable();

                if (clienteId.HasValue) query = query.Where(l => l.ClienteId == clienteId.Value);
                if (quartoId.HasValue) query = query.Where(l => l.QuartoId == quartoId.Value);
                if (!string.IsNullOrEmpty(status)) query = query.Where(l => l.Status == status);
                if (dataEntradaMin.HasValue) query = query.Where(l => l.DataEntrada >= dataEntradaMin.Value.Date);
                if (dataSaidaMax.HasValue) query = query.Where(l => l.DataSaida <= dataSaidaMax.Value.Date);

                var locacoes = await query.OrderByDescending(l => l.DataEntrada).ToListAsync();

                // Mapeia para LocacaoResponse para formatar a saída
                return Results.Ok(locacoes.Select(l => new LocacaoResponse
                {
                    Id = l.Id,
                    ClienteId = l.ClienteId,
                    ClienteNome = l.Cliente?.Nome,
                    QuartoId = l.QuartoId,
                    QuartoNumero = l.Quarto?.Numero ?? 0,
                    DataEntrada = l.DataEntrada,
                    DataSaida = l.DataSaida,
                    TipoLocacao = l.TipoLocacao,
                    QuantidadeCamas = l.QuantidadeCamas,
                    Status = l.Status,
                    CheckInRealizado = l.CheckInRealizado,
                    CheckOutRealizado = l.CheckOutRealizado,
                    // PrecoTotal REMOVIDO
                    UsuarioResponsavelLogin = l.Usuario?.Login
                }));
            })
            .WithSummary("Lista todas as locações com filtros.")
            .Produces<IEnumerable<LocacaoResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);


            // 🔍 Buscar locação por ID com detalhes
            group.MapGet("/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes
                    .Include(l => l.Cliente)
                    .Include(l => l.Quarto)
                    .Include(l => l.Usuario)
                    .FirstOrDefaultAsync(l => l.Id == id);

                return locacao is null
                    ? Results.NotFound("Locação não encontrada.")
                    : Results.Ok(new LocacaoResponse
                    {
                        Id = locacao.Id,
                        ClienteId = locacao.ClienteId,
                        ClienteNome = locacao.Cliente?.Nome,
                        QuartoId = locacao.QuartoId,
                        QuartoNumero = locacao.Quarto?.Numero ?? 0,
                        DataEntrada = locacao.DataEntrada,
                        DataSaida = locacao.DataSaida,
                        TipoLocacao = locacao.TipoLocacao,
                        QuantidadeCamas = locacao.QuantidadeCamas,
                        Status = locacao.Status,
                        CheckInRealizado = locacao.CheckInRealizado,
                        CheckOutRealizado = locacao.CheckOutRealizado,
                        // PrecoTotal REMOVIDO
                        UsuarioResponsavelLogin = locacao.Usuario?.Login
                    });
            })
            .WithSummary("Obtém detalhes de uma locação específica pelo ID.")
            .Produces<LocacaoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);


            // ✏️ Atualizar Status da locação (Cancelar/Reativar) - Apenas Admin
            group.MapPut("/{id}/status/{newStatus}", async (int id, string newStatus, AppDbContext db) =>
            {
                var locacao = await db.Locacoes.FindAsync(id);
                if (locacao == null) return Results.NotFound("Locação não encontrada.");

                newStatus = newStatus.ToLower();

                switch (newStatus)
                {
                    case "cancelado":
                        if (locacao.CheckInRealizado || locacao.CheckOutRealizado)
                        {
                            return Results.BadRequest("Não é possível cancelar uma locação que já teve check-in ou check-out.");
                        }
                        break;
                    case "reservado":
                        if (locacao.Status != "cancelado")
                        {
                             return Results.BadRequest("Só é possível mudar o status para 'reservado' a partir de uma locação cancelada.");
                        }
                        locacao.CheckInRealizado = false;
                        locacao.CheckOutRealizado = false;
                        break;
                    default:
                        return Results.BadRequest("Status inválido. Status permitidos: 'cancelado', 'reservado'.");
                }

                locacao.Status = newStatus;
                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .RequireAuthorization("admin")
            .WithSummary("Atualiza o status de uma locação (e.g., para 'cancelado' ou 'reservado').")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

            // ❌ Remover locação (Apenas Admin e se for "reservado" ou "cancelado")
            group.MapDelete("/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes.FindAsync(id);
                if (locacao == null)
                    return Results.NotFound("Locação não encontrada.");

                if (locacao.Status == "ativo" || locacao.Status == "finalizado")
                {
                    return Results.BadRequest("Não é possível excluir locações ativas ou finalizadas.");
                }

                db.Locacoes.Remove(locacao);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .RequireAuthorization("admin")
            .WithSummary("Exclui uma locação existente. Somente locações não ativas ou finalizadas podem ser excluídas.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        }
    }
}