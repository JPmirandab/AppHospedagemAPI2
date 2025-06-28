namespace AppHospedagemAPI.DTOs;

public class QuartoResponse
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public int QuantidadeCamas { get; set; }
    public string Grupo { get; set; } = string.Empty;
    public bool EstaOcupado { get; set; }

}