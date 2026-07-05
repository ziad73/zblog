using Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register services into DI container
builder.Services.AddControllers();
// db as a service
builder.Services.AddDbContext<ZBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



var app = builder.Build();


// ideal order of middleware

// Exception handling middleware
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
// app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// app.UseCors();
// app.UseAuthentication();
// app.UseAuthorization();
// Custom middleware 
app.MapControllers();
app.Run();
