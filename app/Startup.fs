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
open Microsoft.IdentityModel.Logging
open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    // see https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-aspnet-core-web-api for more
    member _.ConfigureServices(services: IServiceCollection) =
        IdentityModelEventSource.ShowPII <- true
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun jwtOptions ->
                jwtOptions.TokenValidationParameters <-
                    new Microsoft.IdentityModel.Tokens.TokenValidationParameters(
                        ValidateAudience = true,
                        ValidateIssuer = true
                        )
                jwtOptions.Authority <- "https://login.microsoftonline.com/common"
                jwtOptions.Audience <- "https://wilsonsoft.onmicrosoft.com/HelloWeather"
                jwtOptions.TokenValidationParameters.IssuerValidator <-
                    fun issuer jwt tokenValidationParams ->
                        // In order to support multi-tenant auth, replace {tenantid} placeholder with actual tenantId,
                        // e.g. "https://sts.windows.net/{tenantid}/" becomes "https://sts.windows.net/{c4568757-6752-4ed0-a24a-b5ab2df02011}/"
                        // See https://thomaslevesque.com/2018/12/24/multitenant-azure-ad-issuer-validation-in-asp-net-core/ for more
                        let validatedIssuer =
                            match jwt with
                            | :? JwtSecurityToken as jwt ->
                                match jwt.Payload.TryGetValue("tid") with
                                | true, (:? string as tenantId) ->
                                    let validIssuers =
                                        (Seq.append tokenValidationParams.ValidIssuers [tokenValidationParams.ValidIssuer])
                                        |> Seq.filter (System.String.IsNullOrEmpty >> not)
                                        |> Seq.map (fun i -> i.Replace("{tenantid}", tenantId))
                                    validIssuers |> Seq.tryFind (fun i -> i = issuer)
                                | _ -> None
                            | _ -> None
                        match validatedIssuer with
                        | Some i -> i
                        | None ->
                            $"IDX10205: Issuer validation failed. Issuer: '{issuer}'. Did not match: validationParameters.ValidIssuer: '{tokenValidationParams.ValidIssuer}' or validationParameters.ValidIssuers: '{tokenValidationParams.ValidIssuers}'."
                            |> SecurityTokenInvalidIssuerException
                            |> raise
                )
            .Services.AddAuthorization(fun authzOptions ->
                // only allow specific appIds (from trusted issuers) access to this app
                authzOptions.AddPolicy(
                    "ACL",
                    (fun policy ->
                        policy.RequireAssertion(fun ctx ->
                            ctx.User.HasClaim(fun claim ->
                                claim.Type = "appid" && claim.Value = "083d3ba2-ed4e-4e11-b7ef-d8cc46ffe346") // appId
                            &&
                            ctx.User.HasClaim(fun claim ->
                                claim.Type = "iss" && claim.Value = "https://sts.windows.net/4510468d-3790-4a1a-8209-84281b2d1596/") // tenant-of-origin
                            )
                        |> ignore
                    ))
                )
            .AddControllers() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        app.UseHttpsRedirection()
           .UseRouting()
           .UseAuthentication()
           .UseAuthorization()
           .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
            ) |> ignore
