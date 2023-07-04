using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Services;
using AutoPartsServiceWebApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using AutoMapper;
using AutoPartsServiceWebApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
        builder.Services.AddControllers().AddJsonOptions(x =>
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
        //
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddDbContext<AutoDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddSingleton<DatabaseCreator>();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ISmsService, SmsService>();
        builder.Services.AddScoped<IRequestService, RequestService>();
        builder.Services.AddScoped<IServiceService, ServiceService>();
        builder.Services.AddScoped<ICarService, CarService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        });

        app.Run();
    }
}
