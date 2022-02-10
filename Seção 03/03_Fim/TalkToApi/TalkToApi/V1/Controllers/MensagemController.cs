using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;
using TalkToApi.V1.Models.DTO;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MensagemController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IMensagemRepository _mensagemRepository;

		public MensagemController(IMapper mapper, IMensagemRepository mensagemRepository)
		{
			_mapper = mapper;
			_mensagemRepository = mensagemRepository;
		}

		[Authorize]
		[HttpGet("{usuarioUmId}/{usuarioDoisId}", Name ="MensagemObter")]
		public ActionResult Obter(string usuarioUmId, string usuarioDoisId)
		{
			if (usuarioUmId == usuarioDoisId)
			{
				return UnprocessableEntity();
			}
			var mensagens = _mensagemRepository.ObterMensagem(usuarioUmId, usuarioDoisId);
			var listaMsg = _mapper.Map<List<Mensagem>, List<MensagemDTO>>(mensagens);

			var lista = new ListaDTO<MensagemDTO>() { Lista = listaMsg };
			lista.Links.Add(new LinkDTO("_self", Url.Link("MensagemObter", new { usuarioUmId = usuarioUmId, usuarioDoisId = usuarioDoisId }), "GET"));
			
			return Ok(lista);
		}

		[Authorize]
		[HttpPost("", Name ="MensagemCadastrar")]
		public ActionResult Cadastrar([FromBody] Mensagem mensagem)
		{
			if (ModelState.IsValid)
			{
				try
				{
					_mensagemRepository.Cadastrar(mensagem);

					var mensagemDTO = _mapper.Map<Mensagem, MensagemDTO>(mensagem);
					mensagemDTO.Links.Add(new LinkDTO("_self", Url.Link("MensagemCadastrar", null), "POST"));
					mensagemDTO.Links.Add(new LinkDTO("_atualizacaoParcial", Url.Link("MensagemAtualizacaoParcial", new { id = mensagem.Id }), "PATCH"));

					return Ok(mensagemDTO);
				}
				catch (Exception e)
				{

					return UnprocessableEntity();
				}
			}
			else
			{
				return UnprocessableEntity(ModelState);
			}
		}

		[Authorize]
		[HttpPatch("{id}", Name = "MensagemAtualizacaoParcial")]
		public ActionResult AtualizacaoParcial(int id, [FromBody]JsonPatchDocument<Mensagem> jsonPatch)
		{
			if (jsonPatch == null)
				return BadRequest();

			var mensagem = _mensagemRepository.Obter(id);

			jsonPatch.ApplyTo(mensagem);
			mensagem.Atualizado = DateTime.UtcNow;

			_mensagemRepository.Atualizar(mensagem);

			var mensagemDTO = _mapper.Map<Mensagem, MensagemDTO>(mensagem);
			mensagemDTO.Links.Add(new LinkDTO("_self", Url.Link("MensagemAtualizacaoParcial", new { id = mensagem.Id }), "PATCH"));
			
			return Ok(mensagemDTO);
		}
	}
}
