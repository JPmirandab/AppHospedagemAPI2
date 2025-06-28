using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Adicione este using

namespace AppHospedagemAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Documento é obrigatório")]
        [StringLength(14, MinimumLength = 11, ErrorMessage = "Documento deve ter 11 ou 14 dígitos.")]
        // Recomendado: Adicione um índice único no DbContext para garantir unicidade no BD.
        public string Documento { get; set; } = string.Empty; // Armazenar apenas os dígitos

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Telefone deve ter 10 ou 11 dígitos.")]
        public string Telefone { get; set; } = string.Empty; // Armazenar apenas os dígitos

        [JsonIgnore] // Impede loop de serialização JSON
        public ICollection<Locacao>? Locacoes { get; set; } // Navegação para Locações
    }
}