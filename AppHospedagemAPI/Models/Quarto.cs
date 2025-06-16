namespace AppHospedagemAPI.Models
{
    public class Quarto
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int QuantidadeCamas { get; set; }
        public string Grupo { get; set; } = string.Empty; // Ex: São José, Santa Tereza, Nossa Senhora Auxiliadora

        public List<Locacao> Locacoes { get; set; } = new();
    }
}
