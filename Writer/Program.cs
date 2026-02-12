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
                                Console.WriteLine($"Redis conn = '{conn}'");
                                return ConnectionMultiplexer.Connect(conn);
                        });

                        var app = builder.Build();

                        app.UseSwagger();
                        app.UseSwaggerUI();

                        app.MapPost("/set", async (SetRequest req, IConnectionMultiplexer redis) =>
                        {
                                var db = redis.GetDatabase();
                                await db.StringSetAsync(req.key, req.value);

                                return Results.Ok(new
                                {
                                        Target = "MASTER",
                                        req.key,
                                        req.value
                                });
                        });

                        app.Run();
                }
        }

        record SetRequest(string key, string value);
}