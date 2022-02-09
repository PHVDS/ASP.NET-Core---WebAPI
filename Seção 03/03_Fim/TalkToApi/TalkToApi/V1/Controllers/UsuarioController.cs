﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TalkToApi.V1.Models;
using System.Text;
using System.Security.Claims;
using TalkToApi.V1.Repositories.Contracts;
using System.IdentityModel.Tokens.Jwt;
using TalkToApi.V1.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.Linq;

namespace TalkToApi.V1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[ApiVersion("1.0")]
	public class UsuarioController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUsuarioRepository _usuarioRepository;
		private readonly ITokenRepository _tokenRepository;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		public UsuarioController(IMapper mapper, IUsuarioRepository usuarioRepository, ITokenRepository tokenRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
		{
			_mapper = mapper;
			_usuarioRepository = usuarioRepository;
			_tokenRepository = tokenRepository;
			_signInManager = signInManager;
			_userManager = userManager;
		}

		[Authorize]
		[HttpGet("")]
		public ActionResult ObterTodos()
		{
			var usuariosAppUser = _userManager.Users.ToList();

			var listaUsuarioDTO = _mapper.Map<List<ApplicationUser>, List<UsuarioDTO>>(usuariosAppUser);

			foreach (var usuarioDTO in listaUsuarioDTO)
			{
				usuarioDTO.Links.Add(new LinkDTO("_self", Url.Link("ObterUsuario", 
									 new { id = usuarioDTO.Id }), "GET"));
			}
			return Ok(listaUsuarioDTO);
		}

		[HttpGet("{id}", Name = "ObterUsuario")]
		public ActionResult ObterUsuario(string id)
		{
			var usuario = _userManager.FindByIdAsync(id).Result;
			if (usuario == null)
				return NotFound();

			return Ok(usuario);
		}

		[HttpPost("login")]
		public ActionResult Login([FromBody] UsuarioDTO usuarioDTO)
		{
			ModelState.Remove("ConfirmacaoSenha");
			ModelState.Remove("Nome");
			if (ModelState.IsValid)
			{
				ApplicationUser usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);
				if (usuario != null)
				{
					//Login no Identity
					//_signInManager.SignInAsync(usuario, false);

					//Retorna o Token (JWT)
					return GerarToken(usuario);
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

		[HttpPost("renovar")]
		public ActionResult Renovar([FromBody] TokenDTO tokenDTO)
		{
			var refreshTokenDB = _tokenRepository.Obter(tokenDTO.RefreshToken);

			if (refreshTokenDB == null)
				return NotFound();

			//RefreshToken antigo - Atualizar - Desativar esse refreshToken
			refreshTokenDB.Atualizado = DateTime.Now;
			refreshTokenDB.Utilizado = true;
			_tokenRepository.Atualizar(refreshTokenDB);

			//Gerar um novo Token/Refresh Token - Salvar.
			var usuario = _usuarioRepository.Obter(refreshTokenDB.UsuarioId);

			return GerarToken(usuario);

		}

		[HttpPost("")]
		public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO)
		{
			if (ModelState.IsValid)
			{
				ApplicationUser usuario = new ApplicationUser();
				usuario.FullName = usuarioDTO.Nome;
				usuario.UserName = usuarioDTO.Email;
				usuario.Email = usuarioDTO.Email;
				var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

				//Tratando erros
				if (!resultado.Succeeded)
				{
					List<string> erros = new List<string>();
					foreach (var erro in resultado.Errors)
					{
						erros.Add(erro.Description);
					}
					return UnprocessableEntity(erros);
				}
				else
				{
					return Ok(usuario);
				}

			}
			else
			{
				return UnprocessableEntity(ModelState);
			}
		}

		[Authorize]
		[HttpPut("{id}")]
		public ActionResult Atualizar(string id, [FromBody] UsuarioDTO usuarioDTO)
		{
			ApplicationUser usuario = _userManager.GetUserAsync(HttpContext.User).Result;
			if (usuario.Id != id) 
				return Forbid();

			if (ModelState.IsValid)
			{
				usuario.FullName = usuarioDTO.Nome;
				usuario.UserName = usuarioDTO.Email;
				usuario.Email = usuarioDTO.Email;
				usuario.Slogan = usuarioDTO.Slogan;

				var resultado = _userManager.UpdateAsync(usuario).Result;
				_userManager.RemovePasswordAsync(usuario);
				_userManager.AddPasswordAsync(usuario, usuarioDTO.Senha);

				//Tratando erros
				if (!resultado.Succeeded)
				{
					List<string> erros = new List<string>();
					foreach (var erro in resultado.Errors)
					{
						erros.Add(erro.Description);
					}
					return UnprocessableEntity(erros);
				}
				else
				{
					return Ok(usuario);
				}

			}
			else
			{
				return UnprocessableEntity(ModelState);
			}
		}

		private TokenDTO BuildToken(ApplicationUser usuario)
		{
			var claims = new[] {
				new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
				new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas")); //recomendado ser feito no(appsettings.json)
			var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var exp = DateTime.UtcNow.AddHours(1);

			JwtSecurityToken token = new JwtSecurityToken(
				issuer: null,
				audience: null,
				claims: claims,
				expires: exp,
				signingCredentials: sign
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			var refreshToken = Guid.NewGuid().ToString();
			var expRefreshToken = DateTime.UtcNow.AddHours(2);
			var tokenDTO = new TokenDTO { Token = tokenString, Expiration = exp, RefreshToken = refreshToken, ExpirationRefreshToken = expRefreshToken };
			return tokenDTO;
		}

		private ActionResult GerarToken(ApplicationUser usuario)
		{
			var token = BuildToken(usuario);

			//Salvar o token no banco
			var tokenModel = new Token()
			{
				RefreshToken = token.RefreshToken,
				ExpirationToken = token.Expiration,
				ExpirationRefreshToken = token.ExpirationRefreshToken,
				Usuario = usuario,
				Criado = DateTime.Now,
				Utilizado = false
			};

			_tokenRepository.Cadastrar(tokenModel);
			return Ok(token);
		}

	}
}
