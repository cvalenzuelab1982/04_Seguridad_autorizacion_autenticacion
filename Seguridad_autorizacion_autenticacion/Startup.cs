using Seguridad_autorizacion_autenticacion.Filtros;
using Seguridad_autorizacion_autenticacion.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Seguridad_autorizacion_autenticacion.Servicios;

namespace Seguridad_autorizacion_autenticacion
{

    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            //limpiar mapeos de los tipos de los claims cuando se requiera en alguna peticion
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            //Evitar ciclos repetitivos en tablas relacionadas pasadas a JSON
            services.AddControllers(opciones => {
                //Registrar filtros global
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
            }).AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            //Cadena de cnx db
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"));
            });

            //hablitando autorizacion para recursos o peticiones a usuarios
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew=TimeSpan.Zero
                });

            //Open API(Swagger)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Seguridad_autorizacion_autenticacion", Version = "v1" });


                //Habilitando en Swagger el campo para insertar token ya generado
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[]{}
                    }
                });
            });

            //registrando AutoMapper
            services.AddAutoMapper(typeof(Startup));

            //Habilitando servicios de Identity para autorizaciones a usuarios
            services
                .Configure<IdentityOptions>(options => { 
                    options.Password.RequiredLength = 3;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                })
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores< ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Habilitando autorizaciones basada en claim(roles de usuarios)
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("esAdmin", politica => politica.RequireClaim("esAdmin"));
            });

            services.AddDataProtection();

            //registrando el servicio de Hash
            services.AddTransient<HashService>();

            //Hablitando CORS
            services.AddCors(opciones => {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("").AllowAnyMethod().AllowAnyHeader();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Middlewares
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {

            //Middleware que guarda todas las respuestas http
            app.UseLoguearRespuestaHTTP();

            //Valida si el sistema en esta en produccion o mode desarrollo de ser asi mostrar la interfaz de Swagger
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Seguridad_autorizacion_autenticacion v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
