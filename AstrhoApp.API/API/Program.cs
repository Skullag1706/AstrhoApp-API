using AstrhoApp.API.Data;
using AstrhoApp.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace AstrhoApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================
            // DbContext
            // ============================================================
            builder.Services.AddDbContext<AstrhoAppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            // ============================================================
            // Controllers + JSON Converters
            // ============================================================
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    //options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
                    //options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                });

            // ============================================================
            // JWT Authentication
            // ============================================================
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)
                    ),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,

                    ClockSkew = TimeSpan.Zero // 👈 evita desfases de tiempo
                };
            });

            // ============================================================
            // Authorization: políticas estáticas y registro de proveedor dinámico
            // ============================================================
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AgendaAccess", policy =>
                    policy.RequireClaim("permission", "Agenda"));
                options.AddPolicy("CategoriaAccess", policy =>
                    policy.RequireClaim("permission", "Categoria"));
                options.AddPolicy("ClientesAccess", policy =>
                    policy.RequireClaim("permission", "Clientes"));
                options.AddPolicy("ComprasAccess", policy =>
                    policy.RequireClaim("permission", "Compras"));
                options.AddPolicy("EmpleadosAccess", policy =>
                    policy.RequireClaim("permission", "Empleados"));
                options.AddPolicy("EntregasAccess", policy =>
                    policy.RequireClaim("permission", "Entregas"));
                options.AddPolicy("HorariosAccess", policy =>
                    policy.RequireClaim("permission", "Horarios"));
                options.AddPolicy("InsumoAccess", policy =>
                    policy.RequireClaim("permission", "Insumo"));
                options.AddPolicy("ProveedoresAccess", policy =>
                    policy.RequireClaim("permission", "Proveedores"));
                options.AddPolicy("RolesAccess", policy =>
                    policy.RequireClaim("permission", "Roles"));
                options.AddPolicy("ServiciosAccess", policy =>
                    policy.RequireClaim("permission", "Servicios"));
                options.AddPolicy("UsuariosAccess", policy =>
                    policy.RequireClaim("permission", "Usuarios"));
                options.AddPolicy("VentasAccess", policy =>
                    policy.RequireClaim("permission", "Ventas"));
            });

            // Proveedor dinámico para policies de permisos: usar [Authorize(Policy = "perm:Servicio")]
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // ============================================================
            // Servicios de la app
            // ============================================================
            // JWT Service
            builder.Services.AddScoped<JwtService>();

            // Registrar UserService (necesario para AuthController)
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<UserService>();

            // ============================================================
            // Swagger
            // ============================================================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "AstrhoApp API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Ingresa el token JWT"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // ============================================================
            // CORS (para Flutter / móvil)
            // ============================================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMobile", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // ============================================================
            // Crear carpeta wwwroot/imagenes si no existe
            // ============================================================
            var rutaImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagenes");

            if (!Directory.Exists(rutaImagenes))
            {
                Directory.CreateDirectory(rutaImagenes);
            }

            // ============================================================
            // Middleware
            // ============================================================
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AstrhoApp API v1");
                c.RoutePrefix = string.Empty;
            });

            //app.UseHttpsRedirection();

            app.UseCors("AllowMobile");

            // ⚠️ IMPORTANTE: Authentication ANTES de Authorization
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
