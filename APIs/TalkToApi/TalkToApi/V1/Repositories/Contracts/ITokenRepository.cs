using TalkToApi.V1.Models;

namespace TalkToApi.V1.Repositories.Contracts
{
	public interface ITokenRepository
	{
		void Cadastrar(Token token);
		Token Obter(string refreshToken);
		void Atualizar(Token token);
	}
}
