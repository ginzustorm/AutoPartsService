using Microsoft.EntityFrameworkCore;

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

            using (var context = new AutoDbContext(optionsBuilder.Options))
            {
                context.Database.EnsureCreated();
            }

            var json = File.ReadAllText("appsettings.json");
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj["ConnectionStrings"]["DefaultConnection"] = connectionString;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }

        public void CreateLocalDatabase(string databaseName)
        {
            var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true";
            var optionsBuilder = new DbContextOptionsBuilder<AutoDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new AutoDbContext(optionsBuilder.Options))
            {
                context.Database.EnsureCreated();
            }

            var json = File.ReadAllText("appsettings.json");
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj["ConnectionStrings"]["DefaultConnection"] = connectionString;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }

    }
}
