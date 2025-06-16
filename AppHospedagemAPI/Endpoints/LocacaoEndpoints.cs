using AppHospedagemAPI.Models;
using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI.Endpoints
{
    public static class LocacaoEndpoints
    {
        public static void MapLocacaoEndpoints(this WebApplication app)
        {
            // ➕ Criar nova locação (reserva)
            app.MapPost("/locacoes", async (Locacao locacao, AppDbContext db) =>
            {
                // Buscar locações conflitantes (mesmo quarto, datas que se sobrepõem e ainda ativas)
                var conflitos = await db.Locacoes
                    .Where(l => l.QuartoId == locacao.QuartoId &&
                                l.Status != "finalizado" &&
                                locacao.DataEntrada < l.DataSaida &&
                                locacao.DataSaida > l.DataEntrada)
                    .ToListAsync();

                // Verificar conflito para locação de quarto inteiro
                if (locacao.TipoLocacao == "quarto" && conflitos.Count > 0)
                {
                    return Results.BadRequest("Quarto já reservado para o período.");
                }

                // Verificar disponibilidade de camas na locação por cama
                if (locacao.TipoLocacao == "cama")
                {
                    var camasOcupadas = conflitos.Sum(c => c.QuantidadeCamas);
                    var quarto = await db.Quartos.FindAsync(locacao.QuartoId);

                    if (quarto == null)
                        return Results.BadRequest("Quarto não encontrado.");

                    if (camasOcupadas + locacao.QuantidadeCamas > quarto.QuantidadeCamas)
                        return Results.BadRequest("Não há camas disponíveis suficientes.");
                }

                // Criar locação
                db.Locacoes.Add(locacao);
                await db.SaveChangesAsync();
                return Results.Created($"/locacoes/{locacao.Id}", locacao);
            });
            // 🟢 Realizar check-in
            app.MapPost("/locacoes/checkin/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes.FindAsync(id);
                if (locacao == null)
                    return Results.NotFound("Locação não encontrada.");

                // ✅ Verifica se já foi feito check-in (com bool?)
                if (locacao.CheckInRealizado == true)
                    return Results.BadRequest("Check-in já foi realizado.");

                locacao.CheckInRealizado = true;
                await db.SaveChangesAsync();

                return Results.Ok("Check-in realizado com sucesso.");
            });

            // 🔴 Realizar check-out
            app.MapPost("/locacoes/checkout/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes.FindAsync(id);
                if (locacao == null)
                    return Results.NotFound("Locação não encontrada.");

                // ✅ Verifica se o check-in foi feito
                if (locacao.CheckInRealizado != true)
                    return Results.BadRequest("Check-in ainda não foi realizado.");

                // ✅ Verifica se já foi feito check-out
                if (locacao.CheckOutRealizado == true)
                    return Results.BadRequest("Check-out já foi realizado.");

                locacao.CheckOutRealizado = true;
                await db.SaveChangesAsync();

                return Results.Ok("Check-out realizado com sucesso.");
            });


            // 📋 Listar todas as locações com dados do cliente e quarto
            app.MapGet("/locacoes", async (AppDbContext db) =>
            {
                var locacoes = await db.Locacoes
                    .Include(l => l.Cliente)
                    .Include(l => l.Quarto)
                    .ToListAsync();

                return Results.Ok(locacoes);
            });

            // 🔍 Buscar locação por ID com detalhes
            app.MapGet("/locacoes/{id}", async (int id, AppDbContext db) =>
            {
                var locacao = await db.Locacoes
                    .Include(l => l.Cliente)
                    .Include(l => l.Quarto)
                    .FirstOrDefaultAsync(l => l.Id == id);

                return locacao is null ? Results.NotFound() : Results.Ok(locacao);
            });
        }
    }
}
