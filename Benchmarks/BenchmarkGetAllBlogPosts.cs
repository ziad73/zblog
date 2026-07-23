using BenchmarkDotNet.Attributes;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Blog_post;

[SimpleJob(warmupCount: 3, iterationCount: 10)]
[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class BenchmarkGetAllBlogPosts
{
    private ApplicationDbContext _context = null!;
    private IMemoryCache _cache = null!;
    private Dictionary<Guid, List<string>> _rolesDict = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=zblog;Username=my_user;Password=1234")
            .Options;
        _context = new ApplicationDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10_000 });

        _rolesDict = _context.Roles
            .Join(_context.UserRoles, r => r.Id, ur => ur.RoleId, (r, ur) => new { ur.UserId, RoleName = r.Name ?? "" })
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        var data = FetchFromDatabase().GetAwaiter().GetResult();
        _cache.Set("blogpost:list", data, new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromMinutes(5),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });
    }

    private async Task<List<BlogPostListResponseDto>> FetchFromDatabase()
    {
        var posts = await _context.blog_posts
            .Where(p => p.is_deleted == false)
            .Select(p => new
            {
                p.id,
                p.title,
                p.content,
                p.created_at,
                p.updated_at,
                p.author_id,
                p.author.UserName,
                p.author.Email,
                CommentsCount = p.comments.Count,
                LikesCount = p.likes.Count
            })
            .AsNoTracking()
            .ToListAsync();

        return posts.Select(p => new BlogPostListResponseDto(
            p.id,
            p.title ?? string.Empty,
            p.content ?? string.Empty,
            p.created_at,
            p.updated_at,
            p.author_id,
            p.UserName ?? string.Empty,
            p.Email ?? string.Empty,
            _rolesDict.GetValueOrDefault(p.author_id, new List<string>()),
            p.CommentsCount,
            p.LikesCount
        )).ToList();
    }

    [Benchmark]
    public async Task<List<BlogPostListResponseDto>> AllBlogPosts_CacheHit()
    {
        return (await _cache.GetOrCreateAsync("blogpost:list", async entry =>
        {
            entry.Size = 1;
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await FetchFromDatabase();
        }))!;
    }

    [Benchmark(Baseline = true)]
    public async Task<List<BlogPostListResponseDto>> AllBlogPosts_DatabaseFetch()
    {
        return await FetchFromDatabase();
    }
}
