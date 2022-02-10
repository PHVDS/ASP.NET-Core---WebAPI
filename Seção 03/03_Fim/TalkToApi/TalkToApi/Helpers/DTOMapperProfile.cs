using AutoMapper;
using System.Collections.Generic;
using TalkToApi.V1.Models;
using TalkToApi.V1.Models.DTO;

namespace TalkToApi.Helpers
{
	public class DTOMapperProfile : Profile
	{
		public DTOMapperProfile()
		{
			CreateMap<ApplicationUser, UsuarioDTO>()
				.ForMember(destino => destino.Nome, origem => origem.MapFrom(src => src.FullName));

			CreateMap<Mensagem, MensagemDTO>();
		}
	}
}
