using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalkToApi.V1.Models;

namespace TalkToApi.Database
{
	public class TalkToApiContext : IdentityDbContext<ApplicationUser>
	{
		public TalkToApiContext(DbContextOptions<TalkToApiContext> options) : base(options)
		{

		}

		public DbSet<Mensagem> Mensagem { get; set; }
		public DbSet<Token> Tokens { get; set; }
	}
}
