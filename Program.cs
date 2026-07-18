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
using Models.Auth;


var builder = WebApplication.CreateBuilder(args);

// Register services into DI container
builder.Services.AddControllers();

// Register IOptions<JwtSettings> in DI and IOptions mapps/binds structured JSON to C# classes.
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "zblog API",
        Version = "v1",
        Description = "API for the zblog backend"
    });

    // Include XML Comments(triple-slash (///) code comments) safely
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // JWT Security Definition (Best Practice for testing your active context)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });
    // Add JWT security requirement to the OpenAPI document
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

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
builder.Services.AddAuthorization(options =>
{
    // Any action are public by default, unless you specify [Authorize(Policy="RequireMember")] on controllers or actions

    // Admin Policy: Strictly requires the "Admin" role.
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("admin"));

    // Author Policy: Allows both Authors and Admins (since admins have full control).
    options.AddPolicy("RequireAuthor", policy => 
        policy.RequireRole("author", "admin"));

    // Member Policy: Allows Members, Authors, and Admins to access regular reader content.
    options.AddPolicy("RequireMember", policy => 
        policy.RequireRole("member", "author", "admin"));

});

var app = builder.Build();


// ideal order of middleware

// Exception handling middleware
if (app.Environment.IsDevelopment())
{
    // Gives you detailed JSON error details locally
    app.UseDeveloperExceptionPage();
    // Swagger UI public to open, but not all endpoints(public only) are public to execute
    app.UseSwagger(); // Use Swagger to generate OpenAPI specification (swagger.json) and Swagger UI
    app.UseSwaggerUI( options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "zblog API v1");
        options.RoutePrefix = "swagger"; // set swagger UI route
    });

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

// app.UseCors();
app.UseCors("Frontend");// Apply CORS policy globally on all endpoints

app.UseAuthentication();
app.UseAuthorization(); // validates access permissions for the current authenticated user.

// Custom middleware
app.MapControllers();

await Database.IdentityRoleSeeder.SeedAsync(app.Services);

app.Run();
