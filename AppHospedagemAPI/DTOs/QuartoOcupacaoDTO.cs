namespace AppHospedagemAPI.DTOs;

public class QuartoOcupacaoDTO
{
    public int Numero { get; set; }
    public string Grupo { get; set; } = string.Empty;
    public int TotalCamas { get; set; }
    public int CamasOcupadas { get; set; }
    public string Status { get; set; } = string.Empty;
}
