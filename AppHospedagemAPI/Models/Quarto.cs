using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Adicione este using

namespace AppHospedagemAPI.Models
{
    public class Quarto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Número do quarto é obrigatório")]
        [Range(1, 1000, ErrorMessage = "Número deve ser entre 1 e 1000")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "Quantidade de camas é obrigatória")]
        [Range(1, 6, ErrorMessage = "O quarto deve ter de 1 a 6 camas")]
        public int QuantidadeCamas { get; set; }

        [Required(ErrorMessage = "Grupo do quarto é obrigatório")]
        [CustomValidation(typeof(QuartoValidations), nameof(QuartoValidations.ValidarGrupo))]
        public string Grupo { get; set; } = string.Empty;

        // Relacionamento com locações - Use ICollection para melhor performance com EF Core
        [JsonIgnore] // IMPORTANTE: Ignorar na serialização para evitar loops infinitos
        public ICollection<Locacao>? Locacoes { get; set; } // Pode ser 'List<Locacao>' ou 'HashSet<Locacao>'

        // Propriedade calculada - Certifique-se de que ela está lá e com [NotMapped]
        [NotMapped]
        public bool EstaOcupado => Locacoes?.Any(l =>
            l.DataEntrada <= DateTime.Today &&
            l.DataSaida >= DateTime.Today) ?? false;
    }
    public static class QuartoValidations
    {
        private static readonly string[] GruposValidos =
        {
            "São José",
            "Santa Tereza",
            "Nossa Senhora Auxiliadora"
        };

        public static ValidationResult ValidarGrupo(string grupo)
        {
            return GruposValidos.Contains(grupo)
                ? ValidationResult.Success
                : new ValidationResult($"Grupo inválido. Use: {string.Join(", ", GruposValidos)}");
        }
    }
}