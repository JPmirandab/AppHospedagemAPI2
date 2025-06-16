using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI.Endpoints
{
    public static class ResumoEndpoints
    {
        public static void MapResumoEndpoints(this WebApplication app)
        {
            app.MapGet("/dashboard/resumo", async (AppDbContext db) =>
            {
                try
                {
                    var dataAtual = DateTime.Today;
                    Console.WriteLine($"Calculando resumo para {dataAtual}"); // Log no servidor

                    var quartosOcupados = await db.Locacoes
                        .CountAsync(l => l.DataEntrada <= dataAtual &&
                                       l.DataSaida >= dataAtual &&
                                       l.Status == "Ativa");
                    Console.WriteLine($"Quartos ocupados: {quartosOcupados}");

                    var reservasHoje = await db.Locacoes
                        .CountAsync(l => l.DataEntrada == dataAtual &&
                                       l.Status == "Reservada");
                    Console.WriteLine($"Reservas hoje: {reservasHoje}");

                    var clientesAtivos = await db.Locacoes
                        .Where(l => l.DataEntrada <= dataAtual &&
                                   l.DataSaida >= dataAtual &&
                                   l.Status == "Ativa")
                        .Select(l => l.ClienteId)
                        .Distinct()
                        .CountAsync();
                    Console.WriteLine($"Clientes ativos: {clientesAtivos}");

                    var totalQuartos = await db.Quartos.CountAsync();
                    var quartosDisponiveis = totalQuartos - quartosOcupados;
                    Console.WriteLine($"Quartos disponíveis: {quartosDisponiveis}");

                    return new
                    {
                        QuartosOcupados = quartosOcupados,
                        ReservasHoje = reservasHoje,
                        ClientesAtivos = clientesAtivos,
                        QuartosDisponiveis = quartosDisponiveis,
                        TotalQuartos = totalQuartos
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro no endpoint /dashboard/resumo: {ex}");
                    throw;
                }
            });

        }
    }
}
