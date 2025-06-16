using AppHospedagemAPI.Data;
using AppHospedagemAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI.Endpoints;

public static class OcupacaoEndpoints
{
    public static void MapOcupacaoEndpoints(this WebApplication app)
    {
        app.MapGet("/ocupacao", async (AppDbContext db, string? grupo, string? status) =>
        {
            var hoje = DateTime.Today;

            var quartos = await db.Quartos
                .Include(q => q.Locacoes)
                .Select(q => new QuartoOcupacaoDTO
                {
                    Numero = q.Numero,
                    Grupo = q.Grupo,
                    TotalCamas = q.QuantidadeCamas,
                    CamasOcupadas = q.Locacoes
                        .Count(l => l.DataEntrada <= hoje && (l.DataSaida == null || l.DataSaida >= hoje)),
                    Status = ""
                })
                .ToListAsync();

            // Calcular o status com base nas camas ocupadas
            foreach (var quarto in quartos)
            {
                if (quarto.CamasOcupadas == 0)
                    quarto.Status = "Disponível";
                else if (quarto.CamasOcupadas >= quarto.TotalCamas)
                    quarto.Status = "Totalmente Ocupado";
                else
                    quarto.Status = "Parcialmente Ocupado";
            }

            // Filtros opcionais
            if (!string.IsNullOrEmpty(grupo))
                quartos = quartos.Where(q => q.Grupo.Equals(grupo, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(status))
                quartos = quartos.Where(q => q.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            return Results.Ok(quartos);
        });
    }
}
