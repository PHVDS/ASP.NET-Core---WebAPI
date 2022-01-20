using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.Versao2.Controllers
{
	[ApiController]
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiVersion("2.0")]
	public class PalavrasController : ControllerBase
	{
		//APP -- /api/palavras	
		[HttpGet("", Name = "ObterTodas")]
		public string ObterTodas()
		{
			return "Versão 2.0";
		}
	}
}
