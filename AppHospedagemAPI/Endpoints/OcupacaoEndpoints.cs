using AppHospedagemAPI.Data;
using AppHospedagemAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc; // Necessário para [FromQuery]

namespace AppHospedagemAPI.Endpoints;

public static class OcupacaoEndpoints
{
    public static void MapOcupacaoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/ocupacao") // Criando um grupo de rotas
            .WithTags("Ocupação de Quartos") // Tag para Swagger
            .RequireAuthorization(); // Qualquer usuário autenticado pode ver o status de ocupação

        // 📋 Listar quartos com status de ocupação e filtros
        group.MapGet("/", async (
            [FromQuery] string? grupo,
            [FromQuery] string? status, // "Livre", "Parcialmente Ocupado", "Totalmente Ocupado"
            AppDbContext db) =>
        {
            var hoje = DateTime.Today;

            // Começa com a query base e inclui locações
            var query = db.Quartos.Include(q => q.Locacoes).AsQueryable();

            // Mapeia para DTOs e calcula CamasOcupadas e Status em memória
            // Isso é necessário porque a lógica de CamasOcupadas e Status é complexa para ser traduzida diretamente para SQL por 'Select'
            var quartosOcupacao = await query.ToListAsync(); // Carrega tudo para calcular em memória

            var resultados = new List<QuartoOcupacaoDTO>();

            foreach (var quarto in quartosOcupacao)
            {
                // Calcula camas ocupadas considerando ambos os tipos de locação
                int camasOcupadas = quarto.Locacoes?
                    .Where(l => l.DataEntrada <= hoje && l.DataSaida >= hoje && l.Status != "finalizado" && l.Status != "cancelado")
                    .Sum(l => l.TipoLocacao == "quarto" ? quarto.QuantidadeCamas : l.QuantidadeCamas) ?? 0;

                string statusCalculado;
                if (camasOcupadas == 0)
                {
                    statusCalculado = "Livre";
                }
                else if (camasOcupadas >= quarto.QuantidadeCamas)
                {
                    statusCalculado = "Totalmente Ocupado";
                }
                else
                {
                    statusCalculado = "Parcialmente Ocupado";
                }

                resultados.Add(new QuartoOcupacaoDTO
                {
                    Numero = quarto.Numero,
                    Grupo = quarto.Grupo,
                    TotalCamas = quarto.QuantidadeCamas,
                    CamasOcupadas = camasOcupadas,
                    Status = statusCalculado
                });
            }

            // Aplicar filtros após o cálculo, em memória
            IEnumerable<QuartoOcupacaoDTO> resultadosFiltrados = resultados;

            if (!string.IsNullOrEmpty(grupo))
            {
                resultadosFiltrados = resultadosFiltrados.Where(q => q.Grupo.Equals(grupo, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status))
            {
                resultadosFiltrados = resultadosFiltrados.Where(q => q.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return Results.Ok(resultadosFiltrados.OrderBy(q => q.Numero));
        })
        .WithSummary("Lista o status de ocupação atual de todos os quartos.")
        .WithDescription("Permite filtrar por grupo do quarto e status de ocupação (Livre, Parcialmente Ocupado, Totalmente Ocupado).")
        .Produces<IEnumerable<QuartoOcupacaoDTO>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}