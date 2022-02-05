using System.ComponentModel.DataAnnotations;

namespace TalkToApi.V1.Models
{
	public class UsuarioDTO
	{
		[Required]
		public string Nome { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Senha { get; set; }

		[Required]
		[Compare("Senha")]
		public string ConfirmacaoSenha { get; set; }
		public string Slogan { get; set; }
	}
}
