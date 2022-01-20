using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MimicAPI.Versao1.Models;
using MimicAPI.Versao1.Models.DTO;

namespace MimicAPI.Helpers
{
	public class DTOMapperProfile : Profile
	{
		public DTOMapperProfile()
		{
			CreateMap<Palavra, PalavraDTO>();
			CreateMap<PaginationList<Palavra>, PaginationList<PalavraDTO>>();

		}
	}
}
