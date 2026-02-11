using StackExchange.Redis;

namespace Writer
{
        public class Program
        {
                public static void Main(string[] args)
                {
                        var builder = WebApplication.CreateBuilder(args);
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();

                        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                        {
                                var conn = builder.Configuration["Redis:Master"];
                                return ConnectionMultiplexer.Connect(conn);
                        });

                        var app = builder.Build();

                        app.Run();
                }
        }
}
