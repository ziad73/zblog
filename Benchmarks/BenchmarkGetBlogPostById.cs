using BenchmarkDotNet.Attributes;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Blog_post;
using Models.Comment;

[SimpleJob(warmupCount: 3, iterationCount: 10)]
[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class BenchmarkGetBlogPostById
{
    private ApplicationDbContext _context = null!;
    private IMemoryCache _cache = null!;
    private Guid _postId;
    private Dictionary<Guid, List<string>> _rolesDict = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=zblog;Username=my_user;Password=1234")
            .Options;
        _context = new ApplicationDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10_000 });

        _postId = _context.blog_posts
            .Where(p => !p.is_deleted)
            .Select(p => p.id)
            .First();

        _rolesDict = _context.Roles
            .Join(_context.UserRoles, r => r.Id, ur => ur.RoleId, (r, ur) => new { ur.UserId, RoleName = r.Name ?? "" })
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        var data = FetchFromDatabase(_postId).GetAwaiter().GetResult();
        _cache.Set($"blogpost:{_postId}", data, new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromMinutes(5),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });
    }

    private async Task<BlogPostDetailResponseDto?> FetchFromDatabase(Guid id)
    {
        var post = await _context.blog_posts
            .Include(p => p.author)
            .Where(p => p.id == id && p.is_deleted == false)
            .FirstOrDefaultAsync();

        if (post is null)
            return null;

        var roles = _rolesDict.GetValueOrDefault(post.author_id, new List<string>());

        var comments = await _context.comments
            .Include(c => c.author)
            .Include(c => c.likes)
            .Where(c => c.post_id == id && c.is_deleted == false)
            .ToListAsync();

        var likesCount = await _context.likes.CountAsync(l => l.post_id == id);

        var commentLookup = comments.ToLookup(c => c.parent_comment_id);

        List<CommentResponseDto> BuildTree(Guid? parentId)
        {
            return commentLookup[parentId].Select(c => new CommentResponseDto(
                c.id,
                c.content ?? string.Empty,
                c.author_id,
                c.author.UserName ?? string.Empty,
                c.created_at,
                c.likes.Count,
                commentLookup[c.id].Count(),
                BuildTree(c.id)
            )).ToList();
        }

        return new BlogPostDetailResponseDto(
            post.id,
            post.title ?? string.Empty,
            post.content ?? string.Empty,
            post.is_deleted,
            post.created_at,
            post.updated_at,
            post.author_id,
            post.author.UserName ?? string.Empty,
            post.author.Email ?? string.Empty,
            roles,
            comments.Count,
            likesCount,
            BuildTree(null)
        );
    }

    [Benchmark]
    public async Task<BlogPostDetailResponseDto?> SingleBlogPost_CacheHit()
    {
        return await _cache.GetOrCreateAsync($"blogpost:{_postId}", async entry =>
        {
            entry.Size = 1;
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await FetchFromDatabase(_postId);
        });
    }

    [Benchmark(Baseline = true)]
    public async Task<BlogPostDetailResponseDto?> SingleBlogPost_DatabaseFetch()
    {
        return await FetchFromDatabase(_postId);
    }
}
