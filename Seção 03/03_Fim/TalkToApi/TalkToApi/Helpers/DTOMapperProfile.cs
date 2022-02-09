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

			//convertendo uma lista de ApplionUser em uma List de UsuarioDTO
			CreateMap<List<ApplicationUser>, List<UsuarioDTO>>();
			
			//CreateMap<PaginationList<Palavra>, PaginationList<PalavraDTO>>();
		}
	}
}
