using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Extensions.DependencyInjection;


namespace AutoPartsServiceWebApi.Data
{
    public class DatabaseCreator
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public DatabaseCreator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void CreateDatabase(string ip, string login, string password, string databaseName)
        {
            var connectionString = $"Server={ip};Database={databaseName};User Id={login};Password={password};TrustServerCertificate=True;";

            using (var scope = _serviceProvider.CreateScope())
            {
                var options = new DbContextOptionsBuilder<AutoDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                var context = scope.ServiceProvider.GetRequiredService<AutoDbContext>();
                context.Database.Migrate();
            }

            // Load configuration
            var json = File.ReadAllText("appsettings.json");
            var jsonObj = JObject.Parse(json);

            // Make sure "ConnectionStrings" section exists
            if (jsonObj["ConnectionStrings"] == null)
            {
                jsonObj["ConnectionStrings"] = new JObject();
            }

            // Update configuration
            jsonObj["ConnectionStrings"]["DefaultConnection"] = connectionString;

            // Save configuration
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }


        public void CreateLocalDatabase(string databaseName)
        {
            var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;";

            using (var scope = _serviceProvider.CreateScope())
            {
                var options = new DbContextOptionsBuilder<AutoDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                var context = scope.ServiceProvider.GetRequiredService<AutoDbContext>();
                context.Database.Migrate();
            }

            // Load configuration
            var json = File.ReadAllText("appsettings.json");
            var jsonObj = JObject.Parse(json);

            // Make sure "ConnectionStrings" section exists
            if (jsonObj["ConnectionStrings"] == null)
            {
                jsonObj["ConnectionStrings"] = new JObject();
            }

            // Update configuration
            jsonObj["ConnectionStrings"]["DefaultConnection"] = connectionString;

            // Save configuration
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }
    }
}
