using StackExchange.Redis;

namespace Reader
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
                                var conn = builder.Configuration["Redis:Replica"];
                                return ConnectionMultiplexer.Connect(conn);
                        });

                        var app = builder.Build();

                        app.UseSwagger();
                        app.UseSwaggerUI();

                        app.MapGet("/get", async (string key, IConnectionMultiplexer conn) =>
                        {
                                var db = conn.GetDatabase();
                                var value = await db.StringGetAsync(key);

                                return Results.Ok(new
                                {
                                        Target = "REPLICA",
                                        Key = key,
                                        Value = value.HasValue ? value.ToString() : null
                                });
                        });

                        app.MapGet("/set", async (SetRequest req, IConnectionMultiplexer redis) =>
                        {
                                try
                                {
                                        var db = redis.GetDatabase();
                                        await db.StringSetAsync(req.key, req.value);

                                        return Results.Ok("ERROR");
                                }
                                catch (RedisException ex)
                                {
                                        return Results.Problem(
                                                title: "REPLICA IS READ-ONLY",
                                                detail: ex.Message,
                                                statusCode: 403
                                        );
                                }
                        });

                        app.Run();
                }
        }
        record SetRequest(string key, string value);
}