using Database;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Register services into DI container
builder.Services.AddControllers();
// db as a service
builder.Services.AddDbContext<ZBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// repository as a service
// no need to register IRepository<T> as a service, just use it directly
builder.Services.AddScoped<Iblog_postRepository, blog_postRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();// to avoid exposing DbContext in services

//Logging: Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {

 loggerConfiguration
 .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
 .ReadFrom.Services(services); //read out current app's services and make them available to serilog
} );

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
// app.UseAuthentication();
// app.UseAuthorization();
// Custom middleware 
app.MapControllers();
app.Run();
