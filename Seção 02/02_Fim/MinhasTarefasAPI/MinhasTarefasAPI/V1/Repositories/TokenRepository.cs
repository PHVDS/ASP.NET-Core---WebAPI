using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.V1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.V1.Repositories
{
	public class TokenRepository : ITokenRepository
	{
		private readonly MinhasTarefasContext _banco;
		public TokenRepository(MinhasTarefasContext banco)
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
