using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zblog.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blog_posts_AspNetUsers_authorId",
                table: "blog_posts");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_AspNetUsers_authorId",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_blog_posts_postid",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_comments_parent_commentid",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_AspNetUsers_userId",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_blog_posts_postid",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_comments_commentid",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_commentid",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_postid",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_userId",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_comments_authorId",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_parent_commentid",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_postid",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_blog_posts_authorId",
                table: "blog_posts");

            migrationBuilder.DropColumn(
                name: "commentid",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "postid",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "authorId",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "parent_commentid",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "postid",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "authorId",
                table: "blog_posts");

            migrationBuilder.AlterColumn<Guid>(
                name: "post_id",
                table: "likes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "comment_id",
                table: "likes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_likes_comment_id",
                table: "likes",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_likes_post_id",
                table: "likes",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_likes_user_id",
                table: "likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_like_user_comment",
                table: "likes",
                columns: new[] { "user_id", "comment_id" },
                filter: "\"comment_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "uq_like_user_post",
                table: "likes",
                columns: new[] { "user_id", "post_id" },
                filter: "\"post_id\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "chk_like_single_target",
                table: "likes",
                sql: "(\"post_id\" IS NOT NULL AND \"comment_id\" IS NULL) OR (\"post_id\" IS NULL AND \"comment_id\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_comments_author_id",
                table: "comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_parent_comment_id",
                table: "comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_post_id",
                table: "comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_author_id",
                table: "blog_posts",
                column: "author_id");

            migrationBuilder.AddForeignKey(
                name: "FK_blog_posts_AspNetUsers_author_id",
                table: "blog_posts",
                column: "author_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_AspNetUsers_author_id",
                table: "comments",
                column: "author_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_blog_posts_post_id",
                table: "comments",
                column: "post_id",
                principalTable: "blog_posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_comments_parent_comment_id",
                table: "comments",
                column: "parent_comment_id",
                principalTable: "comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_likes_AspNetUsers_user_id",
                table: "likes",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_likes_blog_posts_post_id",
                table: "likes",
                column: "post_id",
                principalTable: "blog_posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_likes_comments_comment_id",
                table: "likes",
                column: "comment_id",
                principalTable: "comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blog_posts_AspNetUsers_author_id",
                table: "blog_posts");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_AspNetUsers_author_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_blog_posts_post_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_comments_parent_comment_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_AspNetUsers_user_id",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_blog_posts_post_id",
                table: "likes");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_comments_comment_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_comment_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_post_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_user_id",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "uq_like_user_comment",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "uq_like_user_post",
                table: "likes");

            migrationBuilder.DropCheckConstraint(
                name: "chk_like_single_target",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_comments_author_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_parent_comment_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_post_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_blog_posts_author_id",
                table: "blog_posts");

            migrationBuilder.AlterColumn<Guid>(
                name: "post_id",
                table: "likes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "comment_id",
                table: "likes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "commentid",
                table: "likes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "postid",
                table: "likes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "userId",
                table: "likes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "authorId",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "parent_commentid",
                table: "comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "postid",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "authorId",
                table: "blog_posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_likes_commentid",
                table: "likes",
                column: "commentid");

            migrationBuilder.CreateIndex(
                name: "IX_likes_postid",
                table: "likes",
                column: "postid");

            migrationBuilder.CreateIndex(
                name: "IX_likes_userId",
                table: "likes",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_authorId",
                table: "comments",
                column: "authorId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_parent_commentid",
                table: "comments",
                column: "parent_commentid");

            migrationBuilder.CreateIndex(
                name: "IX_comments_postid",
                table: "comments",
                column: "postid");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_authorId",
                table: "blog_posts",
                column: "authorId");

            migrationBuilder.AddForeignKey(
                name: "FK_blog_posts_AspNetUsers_authorId",
                table: "blog_posts",
                column: "authorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_AspNetUsers_authorId",
                table: "comments",
                column: "authorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_blog_posts_postid",
                table: "comments",
                column: "postid",
                principalTable: "blog_posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_comments_parent_commentid",
                table: "comments",
                column: "parent_commentid",
                principalTable: "comments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_likes_AspNetUsers_userId",
                table: "likes",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_likes_blog_posts_postid",
                table: "likes",
                column: "postid",
                principalTable: "blog_posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_likes_comments_commentid",
                table: "likes",
                column: "commentid",
                principalTable: "comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
