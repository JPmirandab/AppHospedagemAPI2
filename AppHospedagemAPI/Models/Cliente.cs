namespace AppHospedagemAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty; // CPF ou RG
        public string Telefone { get; set; } = string.Empty;  // Apenas telefone
    }
}
