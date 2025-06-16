namespace AppHospedagemAPI.Models
{
    public class Locacao
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int QuartoId { get; set; }
        public Quarto Quarto { get; set; }

        public string TipoLocacao { get; set; } = string.Empty; // "quarto" ou "cama"
        public int QuantidadeCamas { get; set; } // Se TipoLocacao for "cama", usamos isso
        public DateTime DataEntrada { get; set; }
        public DateTime DataSaida { get; set; }
        public string Status { get; set; } = "reservado"; // reservado, ocupado, finalizado
        public bool? CheckInRealizado { get; set; }
        public bool? CheckOutRealizado { get; set; }

    }
}
