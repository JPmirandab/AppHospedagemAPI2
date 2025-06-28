namespace AppHospedagemAPI.DTOs;

public class DashboardResumoResponse
{
    public int TotalQuartos { get; set; }
    public int QuartosLivres { get; set; } // Renomeado para mais clareza
    public int QuartosOcupadosTotalmente { get; set; } // Novo: se todos os camas/quarto inteiro estiver ocupado
    public int QuartosParcialmenteOcupados { get; set; } // Novo: se algumas camas estiverem ocupadas
    public int ReservasHoje { get; set; } // Check-ins esperados hoje
    public int CheckOutsHoje { get; set; } // Check-outs esperados hoje
    public int ClientesAtivosHoje { get; set; }
}