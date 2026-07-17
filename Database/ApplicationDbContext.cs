using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Auth;

namespace Database;
public class ApplicationDbContext : IdentityDbContext<User, user_role, Guid>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
  {
  }
  /* The modern way to declare your database tables
    - it eliminates null warnings cleanly without resorting to dummy initialization tricks like = null!;*/
  public DbSet<RefreshToken> refresh_tokens => Set<RefreshToken>();
  public DbSet<blog_post> blog_posts => Set<blog_post>();
  public DbSet<Comment> comments => Set<Comment>();
  public DbSet<Like> likes => Set<Like>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<blog_post>(entity =>
    {
      entity.ToTable("blog_posts");
      entity.Property(x => x.id).ValueGeneratedOnAdd();
      entity.Property(x => x.title).HasMaxLength(255).IsRequired();
      entity.Property(x => x.content).IsRequired();
      entity.HasOne(x => x.author)
        .WithMany(x => x.posts)
        .HasForeignKey(x => x.author_id)
        .OnDelete(DeleteBehavior.Restrict);
      entity.HasIndex(x => x.author_id);
    });

    modelBuilder.Entity<Comment>(entity =>
    {
      entity.ToTable("comments");
      entity.Property(x => x.id).ValueGeneratedOnAdd();
      entity.Property(x => x.content).IsRequired();
      entity.HasOne(x => x.author)
        .WithMany(x => x.comments)
        .HasForeignKey(x => x.author_id)
        .OnDelete(DeleteBehavior.Restrict);
      entity.HasOne(x => x.post)
        .WithMany(x => x.comments)
        .HasForeignKey(x => x.post_id)
        .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(x => x.parent_comment)
        .WithMany(x => x.replies)
        .HasForeignKey(x => x.parent_comment_id)
        .OnDelete(DeleteBehavior.Cascade);
      entity.HasIndex(x => x.author_id);
      entity.HasIndex(x => x.post_id);
      entity.HasIndex(x => x.parent_comment_id);
    });

    modelBuilder.Entity<Like>(entity =>
    {
      entity.ToTable("likes", table =>
      {
        table.HasCheckConstraint(
          "chk_like_single_target",
          "(\"post_id\" IS NOT NULL AND \"comment_id\" IS NULL) OR (\"post_id\" IS NULL AND \"comment_id\" IS NOT NULL)"
        );
      });
      entity.Property(x => x.id).ValueGeneratedOnAdd();
      entity.HasOne(x => x.user)
        .WithMany(x => x.likes)
        .HasForeignKey(x => x.user_id)
        .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(x => x.comment)
        .WithMany(x => x.likes)
        .HasForeignKey(x => x.comment_id)
        .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(x => x.post)
        .WithMany(x => x.likes)
        .HasForeignKey(x => x.post_id)
        .OnDelete(DeleteBehavior.Cascade);
      entity.HasIndex(x => x.user_id)
        .HasDatabaseName("IX_likes_user_id");
      entity.HasIndex(x => x.comment_id)
        .HasDatabaseName("IX_likes_comment_id");
      entity.HasIndex(x => x.post_id)
        .HasDatabaseName("IX_likes_post_id");
      entity.HasIndex(x => new { x.user_id, x.post_id })
        .HasFilter("\"post_id\" IS NOT NULL")
        .HasDatabaseName("uq_like_user_post");
      entity.HasIndex(x => new { x.user_id, x.comment_id })
        .HasFilter("\"comment_id\" IS NOT NULL")
        .HasDatabaseName("uq_like_user_comment");
    });
  }

}
