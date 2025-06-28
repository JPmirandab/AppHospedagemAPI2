namespace AppHospedagemAPI.DTOs;

public class ClienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty; // Já virá formatado da API
    public string Telefone { get; set; } = string.Empty; // Já virá formatado da API
}