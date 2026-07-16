using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Database;
using Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Services.Auth;
using Services.Auth.Contracts;
using Services.Blog_post;
using Services.Blog_post.Contracts;
using zblog.Models.Auth;
using zblog.Services.Auth;
using zblog.Services.Auth.Contracts;


var builder = WebApplication.CreateBuilder(args);

// Register services into DI container
builder.Services.AddControllers();

// IOptions mapps structured JSON to C# classes.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() 
    ?? throw new InvalidOperationException("JWT Settings are missing from configuration.");

// Authservices
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBlogPostServices, BlogPostServices>();

// db as a service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Logging: Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

 loggerConfiguration
 .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
 .ReadFrom.Services(services); //read out current app's services and make them available to serilog
} );

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer(); //generates description for all endpoints
builder.Services.AddSwaggerGen( options => 
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "zblog API",
        Version = "v1",
        Description = "API for the zblog backend"
    });

    // 1. Define the XML file name (usually matches your project name)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    
    // 2. Combine it with the application's base directory
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    // 3. Tell Swagger to include those comments
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
}); //generates OpenAPI specification (swagger.json) and Swagger UI

// CORS (cross-origin resource sharing) Policy Configuration
builder.Services.AddCors(options =>
{
    /* Custom CORS policy applied:
        - To specific endpoints using [EnableCors("Frontend")] attribute
        - Or apply it globally if most endpoints share the same rules by app.UseCors("Frontend");
        */
    options.AddPolicy("Frontend", policy =>
    {
        // Read allowed domains from appsettings.json file
        var allowedOrigins = "*";//builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

});


// Register identity services into DI Container
builder.Services.AddIdentity<User, user_role>(options => {

options.Password.RequiredLength = 4; 
options.Password.RequireNonAlphanumeric = false; 
options.Password.RequireUppercase = false; 
options.Password.RequireLowercase = false; 
options.Password.RequireDigit = false; 
options.Password.RequiredUniqueChars = 2;
   
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
/* userManager uses UserStore and RoleStore internally.
   CreateAsync() uses UserStore to create a new user in the database, so on. 
   initialize repositories for user and role management.*/
.AddUserStore<UserStore<User, user_role, ApplicationDbContext, Guid>>()
.AddRoleStore<RoleStore<user_role, ApplicationDbContext, Guid>>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
    {
        // Read & validate JWT tokens from the request header 'Authorization: Bearer <token>'
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        // retruns 401 if not authenticated
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Keep the claim names exactly as they appear in the token (no surprise remapping).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };
    });

// register the authorization services (like policies and role-checks) into DI container
builder.Services.AddAuthorization();
// options =>{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//       // Require an authenticated user for all endipoints, unless you explicitly specified [AllowAnonymous].
//       .RequireAuthenticatedUser() 
//       .Build();
//});

// --------------------------------------------------------------------------------

// cookie authentication configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ZBlogCookie";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});


var app = builder.Build();


// ideal order of middleware

// Exception handling middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Gives you detailed JSON error details locally
}
else
{
    app.UseExceptionHandler("/Error"); // Production friendly error fallback
}
// app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
// logging
app.UseSerilogRequestLogging();
app.UseRouting();

/*Before auth middleware?!
    - Swagger UI public to open, but not all endpoints are public to execute, it depends on the [AllowAnonymous]/[Authorize] attribute of endpoint*/
app.UseSwagger(); // Use Swagger to generate OpenAPI specification (swagger.json) and Swagger UI
app.UseSwaggerUI( options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "zblog API v1");
    options.RoutePrefix = "swagger"; // set swagger UI route
});

// app.UseCors();
app.UseCors("Frontend");// Apply CORS policy globally on all endpoints

app.UseAuthentication(); // Reads identity cookie.
app.UseAuthorization(); // validates access permissions for the current authenticated user.

// Custom middleware
app.MapControllers();

await Database.IdentityRoleSeeder.SeedAsync(app.Services);

app.Run();
