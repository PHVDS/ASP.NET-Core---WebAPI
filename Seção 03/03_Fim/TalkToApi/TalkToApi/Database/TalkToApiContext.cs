using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;

namespace TalkToApi.Database
{
	public class TalkToApiContext : IdentityDbContext<ApplicationUser>
	{
		public TalkToApiContext(DbContextOptions<TalkToApiContext> options) : base(options)
		{

		}

		public DbSet<Mensagem> Mensagem { get; set; }
	}
}
