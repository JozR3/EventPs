
using Application.Interfaces;
using EventoPS.Service;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventoPS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            var connectionString = builder.Configuration["ConnectionString"];
            //Servicios
            builder.Services.AddDbContext<EventDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<ISeatService, SeatService>();
            builder.Services.AddScoped<IReservation, ReservationService>();

            builder.Services.AddScoped<IReservedSeatService, ReservedSeatService>();
            builder.Services.AddScoped<ICheckoutService, CheckoutService>();
            //

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<EventDbContext>();

                try
                {
                    //context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Database.Migrate();

                    
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error al crear la base de datos.");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.Run();
        }

    }
}
