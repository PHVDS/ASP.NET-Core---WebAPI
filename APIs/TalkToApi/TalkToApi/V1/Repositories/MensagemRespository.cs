using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.Database;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Repositories
{
	public class MensagemRespository : IMensagemRepository
	{
		private readonly TalkToApiContext _banco;
		public MensagemRespository(TalkToApiContext banco)
		{
			_banco = banco;
		}

		public void Atualizar(Mensagem mensagem)
		{
			_banco.Mensagem.Update(mensagem);
			_banco.SaveChanges();
		}

		public void Cadastrar(Mensagem mensagem)
		{
			_banco.Mensagem.Add(mensagem);
			_banco.SaveChanges();
		}

		public Mensagem Obter(int id)
		{
			return _banco.Mensagem.Find(id);
		}

		public List<Mensagem> ObterMensagem(string usuarioUmId, string usuarioDoisId)
		{
			return _banco.Mensagem.Where(a => (a.DeId == usuarioUmId || a.DeId == usuarioDoisId) && (a.ParaId == usuarioUmId || a.ParaId == usuarioDoisId)).ToList();
		}
	}
}
