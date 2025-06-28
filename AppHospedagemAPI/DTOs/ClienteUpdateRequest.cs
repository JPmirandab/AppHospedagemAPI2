using System.ComponentModel.DataAnnotations;

namespace AppHospedagemAPI.DTOs;

public class ClienteUpdateRequest
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone deve conter 10 ou 11 dígitos numéricos (com DDD).")]
    public string Telefone { get; set; } = string.Empty;

    // Nota: O documento (CPF/CNPJ) geralmente não é alterado em uma atualização simples.
    // Se a regra de negócio permitir, você pode adicioná-lo aqui com as validações necessárias.
}