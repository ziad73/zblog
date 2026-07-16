-- SQL seeder for zblog tests
-- Inserts roles, users, blog posts, comments and likes.
-- Adjust quoting/casts if your DB provider requires them (Postgres vs SQL Server).

-- Roles
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp) VALUES
('99999999-0000-0000-0000-000000000001', 'Admin', 'ADMIN', 'c1'),
('88888888-0000-0000-0000-000000000002', 'Author', 'AUTHOR', 'c2'),
('77777777-0000-0000-0000-000000000003', 'Member', 'MEMBER', 'c3');

-- Users (PasswordHash left NULL for tests that don't require authentication)
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, created_at, updated_at)
VALUES
('11111111-1111-1111-1111-111111111111', 'alice', 'ALICE', 'alice@example.com', 'ALICE@EXAMPLE.COM', 1, NULL, 'ss1', 'cs1', 0, 0, 0, 0, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z'),
('22222222-2222-2222-2222-222222222222', 'bob', 'BOB', 'bob@example.com', 'BOB@EXAMPLE.COM', 1, NULL, 'ss2', 'cs2', 0, 0, 0, 0, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z'),
('33333333-3333-3333-3333-333333333333', 'author1', 'AUTHOR1', 'author1@example.com', 'AUTHOR1@EXAMPLE.COM', 1, NULL, 'ss3', 'cs3', 0, 0, 0, 0, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z');

-- User roles
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
('11111111-1111-1111-1111-111111111111','99999999-0000-0000-0000-000000000001'),
('33333333-3333-3333-3333-333333333333','88888888-0000-0000-0000-000000000002'),
('22222222-2222-2222-2222-222222222222','77777777-0000-0000-0000-000000000003');

-- Blog posts
INSERT INTO blog_posts (id, title, content, is_deleted, deleted_at, created_at, updated_at, author_id)
VALUES
('aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', 'First Post', 'This is the first seeded post.', 0, NULL, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z', '33333333-3333-3333-3333-333333333333'),
('bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb', 'Hello World', 'Hello world content.', 0, NULL, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z', '33333333-3333-3333-3333-333333333333');

-- Comments
INSERT INTO comments (id, content, is_deleted, deleted_at, created_at, updated_at, parent_comment_id, post_id, author_id)
VALUES
('cccccccc-3333-3333-3333-cccccccccccc', 'Nice post!', 0, NULL, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z', NULL, 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111'),
('dddddddd-4444-4444-4444-dddddddddddd', 'Thanks!', 0, NULL, '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z', 'cccccccc-3333-3333-3333-cccccccccccc', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222222');

-- Likes (user likes a post and a comment)
INSERT INTO likes (id, created_at, user_id, post_id, comment_id)
VALUES
('eeeeeeee-5555-5555-5555-eeeeeeeeeeee', '2026-07-13T00:00:00Z', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', NULL),
('ffffffff-6666-6666-6666-ffffffffffff', '2026-07-13T00:00:00Z', '11111111-1111-1111-1111-111111111111', NULL, 'cccccccc-3333-3333-3333-cccccccccccc');

-- Notes:
-- - If your DB is PostgreSQL and uses UUID types, you may need to cast strings to UUID (e.g. '1111...'::uuid).
-- - If your Identity setup requires non-null PasswordHash, replace NULL with a valid hash produced by ASP.NET Identity's PasswordHasher.
-- - Run this file against your test database before running tests that depend on seeded data.
