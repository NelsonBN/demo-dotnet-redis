using Demo.Api.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);
{
       builder.Services
              .AddControllers();

       builder.Services
              .AddEndpointsApiExplorer()
              .AddSwaggerGen();

       builder.Services
              .AddDbContext<DatabaseContext>()
              .AddStackExchangeRedisCache(options =>
              {
                     options.Configuration = builder.Configuration.GetConnectionString("Redis");
                     options.InstanceName = nameof(Demo.Api);
              });
}

var app = builder.Build();
{
       app.UseSwagger()
          .UseSwaggerUI();

       app.MapControllers();
}

app.Run();
