using System.ComponentModel.DataAnnotations;

namespace AppHospedagemAPI.DTOs;

public class QuartoCreateRequest // Ou simplesmente QuartoRequest se for usado para criar e atualizar
{
    [Required(ErrorMessage = "Número do quarto é obrigatório")]
    [Range(1, 1000, ErrorMessage = "Número deve ser entre 1 e 1000")]
    public int Numero { get; set; }

    [Required(ErrorMessage = "Quantidade de camas é obrigatória")]
    [Range(1, 6, ErrorMessage = "O quarto deve ter de 1 a 6 camas")]
    public int QuantidadeCamas { get; set; }

    [Required(ErrorMessage = "Grupo do quarto é obrigatório")]
    // Validação de grupo aqui, se quiser centralizar. No modelo Quarto já temos.
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Grupo deve ter entre 3 e 50 caracteres.")]
    public string Grupo { get; set; } = string.Empty;
}