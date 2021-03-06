using TalkToApi.Database;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;
using System.Linq;

namespace TalkToApi.V1.Repositories
{
	public class TokenRepository : ITokenRepository
	{
		private readonly TalkToApiContext _banco;
		public TokenRepository(TalkToApiContext banco)
		{
			_banco = banco;
		}

		public Token Obter(string refreshToken)
		{
			return _banco.Tokens.FirstOrDefault(a => a.RefreshToken == refreshToken && a.Utilizado == false);
		}

		public void Cadastrar(Token token)
		{
			_banco.Tokens.Add(token);
			_banco.SaveChanges();
		}

		public void Atualizar(Token token)
		{
			_banco.Tokens.Update(token);
			_banco.SaveChanges();
		}
	}
}
