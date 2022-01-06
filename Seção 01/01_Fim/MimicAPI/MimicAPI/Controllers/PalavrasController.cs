using Microsoft.AspNetCore.Mvc;
using MimicAPI.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.Controllers
{
	public class PalavrasController : ControllerBase
	{
		private readonly MimicContext _banco;

		public PalavrasController(MimicContext banco)
		{
			_banco = banco;
		}

		public ActionResult ObterPalavras()
		{
			//retorna um Json ou Xml
			return Ok(_banco.Palavras);
		}

	}
}
