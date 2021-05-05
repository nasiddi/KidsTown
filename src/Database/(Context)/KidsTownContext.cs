using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace KidsTown.Database
{
    public partial class KidsTownContext
    {
        private readonly IConfiguration _configuration;

        public KidsTownContext(DbContextOptions<KidsTownContext> options, IConfiguration configuration)
            : base(options: options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString: _configuration.GetConnectionString(name: "Database"));
            }
        }
    }
}