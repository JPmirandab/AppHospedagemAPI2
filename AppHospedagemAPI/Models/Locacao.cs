using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Adicionar para [JsonIgnore]

namespace AppHospedagemAPI.Models
{
    public class Locacao
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        [JsonIgnore] // Evita ciclo de referência na serialização
        public Cliente? Cliente { get; set; } // Propriedade de navegação

        public int QuartoId { get; set; }
        [JsonIgnore] // Evita ciclo de referência na serialização
        public Quarto? Quarto { get; set; } // Propriedade de navegação

        [Required]
        public DateTime DataEntrada { get; set; }

        [Required]
        public DateTime DataSaida { get; set; }

        [Required]
        [StringLength(10)] // "quarto" ou "cama"
        public string TipoLocacao { get; set; } = string.Empty;

        // Quantidade de camas ocupadas por esta locação (0 se for locação de quarto inteiro)
        public int QuantidadeCamas { get; set; }

        [Required]
        [StringLength(20)] // "reservado", "ativo", "finalizado", "cancelado"
        public string Status { get; set; } = "reservado"; // Status inicial padrão

        public bool CheckInRealizado { get; set; } = false;
        public bool CheckOutRealizado { get; set; } = false;

        // Preço por noite para o tipo de locação (pode vir de um lookup ou ser fixo/baseado no quarto)
        //public decimal PrecoTotal { get; set; } // Valor total da locação

        // Informações sobre quem criou/modificou a locação (se necessário para auditoria)
        public int? UsuarioId { get; set; } // FK para o usuário que realizou a locação
        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    }
}