using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace AppHospedagemAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Login é obrigatório")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Login deve ter entre 5 e 50 caracteres")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255)] // O hash de bcrypt é longo, use um tamanho maior
        public string SenhaHash { get; set; } = string.Empty;
        
        [Required] // O salt também é obrigatório
        [StringLength(255)] // O salt também será uma string longa
        public string SenhaSalt { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Role é obrigatória")]
        [StringLength(50)]
        public string Role { get; set; } = "funcionario"; // Valor padrão para novos usuários

        // --- Métodos para Hashing e Verificação ---

        // Método para definir a senha do usuário, gerando o hash e o salt
        public void SetSenha(string senha)
        {
            // Gerar um salt aleatório para esta senha
            SenhaSalt = BCrypt.Net.BCrypt.GenerateSalt();

            // Gerar o hash da senha usando o salt
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha, SenhaSalt);
        }

        // Método para verificar se uma senha fornecida corresponde ao hash armazenado
        public bool VerificarSenha(string senhaFornecida)
        {
            // Usar o salt armazenado para verificar a senha fornecida
            return BCrypt.Net.BCrypt.Verify(senhaFornecida, SenhaHash);
        }
    }
}