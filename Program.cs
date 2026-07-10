using Database;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Services;
using Services.Contracts;


var builder = WebApplication.CreateBuilder(args);

// Register services into DI container
builder.Services.AddControllers();
// Account service
builder.Services.AddScoped<IAuthService, AuthService>();

// db as a service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Logging: Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

 loggerConfiguration
 .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
 .ReadFrom.Services(services); //read out current app's services and make them available to serilog
} );


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

// Authorization configuration
builder.Services.AddAuthorization(options =>
{
   options.FallbackPolicy = new AuthorizationPolicyBuilder()
      // Require an authenticated user for all endipoints, unless you explicitly specified [AllowAnonymous].
      .RequireAuthenticatedUser() 
      .Build();
});

// --------------------------------------------------------------------------------

// cookie authentication configuration
builder.Services.ConfigureApplicationCookie(options =>
{
   options.Cookie.Name = "ZBlog Cookie"; // change this to something more unique
   // Redirect unauthenticated users
   options.LoginPath = "/Account/Login"; 
   options.AccessDeniedPath = "/Account/AccessDenied";
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
// app.UseCors();
app.UseAuthentication(); // Reads identity cookie.
app.UseAuthorization(); // validates access permissions for the current authenticated user.
// Custom middleware 
app.MapControllers();
app.Run();
