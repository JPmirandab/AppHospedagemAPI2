using System.ComponentModel.DataAnnotations;

namespace AppHospedagemAPI.DTOs;

public class ClienteCreateRequest
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O documento (CPF/CNPJ) é obrigatório.")]
    // Regex para validar 11 ou 14 dígitos (CPF ou CNPJ). Pode ser ajustado.
    [RegularExpression(@"^\d{11}$|^\d{14}$", ErrorMessage = "Documento deve conter 11 (CPF) ou 14 (CNPJ) dígitos numéricos.")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone deve conter 10 ou 11 dígitos numéricos (com DDD).")]
    public string Telefone { get; set; } = string.Empty;
}