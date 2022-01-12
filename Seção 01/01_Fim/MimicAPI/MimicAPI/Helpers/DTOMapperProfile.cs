using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MimicAPI.Models;
using MimicAPI.Models.DTO;

namespace MimicAPI.Helpers
{
	public class DTOMapperProfile : Profile
	{
		public DTOMapperProfile()
		{
			CreateMap<Palavra, PalavraDTO>();
		}
	}
}
