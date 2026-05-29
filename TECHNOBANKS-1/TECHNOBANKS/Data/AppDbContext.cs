using Microsoft.EntityFrameworkCore;
using TECHNOBANKS.Models;

namespace TECHNOBANKS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Solicitacao> Solicitacoes { get; set; }
    }
}