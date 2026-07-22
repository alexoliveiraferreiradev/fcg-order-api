

using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Fcg.Orders.API.Extentions
{
    public static class SwaggerExtensions
    {
        public static WebApplicationBuilder AddOpenApiExtension(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FIAP Cloud Games - API de Pedidos",
                    Version = "v1",
                    Description = "API de Pedidos. Responsável pelo processamento de pedidos de compra de cada usuário do ecossistema FCG.",
                    Contact = new OpenApiContact
                    {
                        Name = "Alex Oliveira Ferreira",
                        Email = "alexoliveiraferreiradev@gmail.com",
                        Url = new Uri("https://github.com/alexoliveiraferreiradev")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                var xmlFileApp = "Fcg.Catalog.Application.xml";
                var xmlPathApp = Path.Combine(AppContext.BaseDirectory, xmlFileApp);

                if (File.Exists(xmlPathApp))
                {
                    options.IncludeXmlComments(xmlPathApp);
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT: Bearer {Seu Token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                             }
                         },
                         Array.Empty<string>()
                     }
                 });

            });
            return builder;
        }
    }
}
