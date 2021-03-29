namespace app

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.Identity.Web

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    // see https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-aspnet-core-web-api for more
    member _.ConfigureServices(services: IServiceCollection) =
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun o ->

                configuration.Bind(o)
                o.TokenValidationParameters <- new Microsoft.IdentityModel.Tokens.TokenValidationParameters(ValidateAudience=true)
                )
            .Services.AddControllers() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        app.UseHttpsRedirection()
           .UseRouting()
           .UseAuthentication()
           .UseAuthorization()
           //.UseAuthentication(fun op -> op.RequireAuthorization(
           .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
            ) |> ignore
