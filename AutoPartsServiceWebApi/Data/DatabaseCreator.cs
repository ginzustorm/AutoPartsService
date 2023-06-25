using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace AutoPartsServiceWebApi.Data
{
    public class DatabaseCreator
    {
        private readonly IConfiguration _configuration;

        public DatabaseCreator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void CreateDatabase(string ip, string login, string password, string databaseName)
        {
            var connectionString = $"Server={ip};Database={databaseName};User Id={login};Password={password};TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<AutoDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new AutoDbContext(connectionString))
            {
                context.Database.Migrate();
            }

            // Load configuration
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var root = configurationBuilder.Build();

            // Update configuration
            root.GetSection("ConnectionStrings")["DefaultConnection"] = connectionString;

            // Save configuration
            File.WriteAllText("appsettings.json", JsonConvert.SerializeObject(root, Formatting.Indented));
        }
    }
}
