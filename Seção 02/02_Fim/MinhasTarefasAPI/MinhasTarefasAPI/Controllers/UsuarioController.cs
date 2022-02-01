using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuarioController : ControllerBase
	{
		private readonly IUsuarioRepository _usuarioRepository;
		private readonly SignInManager<ApplicationUser> _signInManager;
		public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager)
		{
			_usuarioRepository = usuarioRepository;
			_signInManager = signInManager;
		}

		public ActionResult Login([FromBody]UsuarioDTO usuarioDTO)
		{
			ModelState.Remove("ConfirmacaoSenha");
			ModelState.Remove("Nome");
			if (ModelState.IsValid)
			{
				ApplicationUser usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);
				if (usuario != null)
				{
					//Login no Identity
					_signInManager.SignInAsync(usuario, false);

					//Retorna o Token (JWT)
					return Ok();
				}
				else
				{
					return NotFound("Usuario não localizado");
				}
			}
			else
			{
				return UnprocessableEntity(ModelState);
			}
		}
	}
}
