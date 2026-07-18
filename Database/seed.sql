-- PostgreSQL seed data for zblog
-- Clears existing data and inserts fresh test data with predictable UUIDs.
-- Password for all users: P@ssw0rd123

-- Data inserted:
-- Table	   Rows	Details
-- Roles	    3	  Admin, Author, Member
-- Users	    5   alice (admin), author1, author2, bob (member), charlie (member)
-- Blog posts	6	  5 active + 1 soft-deleted
-- Comments	  9   6 top-level + 3 nested replies (up to 2 levels deep)
-- Likes	    14	9 on posts, 5 on comments


BEGIN;

-- Disable triggers temporarily for clean truncation
TRUNCATE TABLE likes, comments, blog_posts, "AspNetUserRoles", "AspNetUsers", "AspNetRoles" CASCADE;

-- ── ROLES ──────────────────────────────────────────────────────────
-- Names must match user_type_option enum (ToString() → lowercase)
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES
  ('99999999-0000-0000-0000-000000000001', 'admin',  'ADMIN',  'c1'),
  ('88888888-0000-0000-0000-000000000002', 'author', 'AUTHOR', 'c2'),
  ('77777777-0000-0000-0000-000000000003', 'member', 'MEMBER', 'c3');

-- ── USERS ──────────────────────────────────────────────────────────
INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "Name", "created_at", "updated_at")
VALUES
  -- Admin
  ('11111111-1111-1111-1111-111111111111', 'alice',   'ALICE',   'alice@example.com',   'ALICE@EXAMPLE.COM',
   TRUE, 'AQAAAAIAAYagAAAAELTB2XjS1Ba1wJCpIaFGiopgiXN2D9XZ0heGQRWqmFUQQ8MT8sVueu3XILEGe7fMpw==', 'ss1', 'cs1', FALSE, FALSE, TRUE, 0,
   'Alice Admin',   '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z'),
  -- Authors
  ('22222222-2222-2222-2222-222222222222', 'author1', 'AUTHOR1', 'author1@example.com', 'AUTHOR1@EXAMPLE.COM',
   TRUE, 'AQAAAAIAAYagAAAAEByly0F1amJ0Mc5dCn89KpU9S9HT7sj/8BuWxJOknaf9BykcfpzKX/U9x0TxpAXckg==', 'ss2', 'cs2', FALSE, FALSE, TRUE, 0,
   'Author One',   '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z'),
  ('33333333-3333-3333-3333-333333333333', 'author2', 'AUTHOR2', 'author2@example.com', 'AUTHOR2@EXAMPLE.COM',
   TRUE, 'AQAAAAIAAYagAAAAEJ2gNq4C5PQ7ApCE9t1X8NZ4VA4vhrPOhTBowHbqzajKgWZjC4TFnUm0Bhqsbo9UnQ==', 'ss3', 'cs3', FALSE, FALSE, TRUE, 0,
   'Author Two',   '2026-07-14T00:00:00Z', '2026-07-14T00:00:00Z'),
  -- Members
  ('44444444-4444-4444-4444-444444444444', 'bob',     'BOB',     'bob@example.com',     'BOB@EXAMPLE.COM',
   TRUE, 'AQAAAAIAAYagAAAAENlX/sXYDha910gW+9w9jFn/fBClRlHchL4A8XySJtAnBUTYocF6GhRGan5ycJKyDw==', 'ss4', 'cs4', FALSE, FALSE, TRUE, 0,
   'Bob Member',   '2026-07-13T00:00:00Z', '2026-07-13T00:00:00Z'),
  ('55555555-5555-5555-5555-555555555555', 'charlie', 'CHARLIE', 'charlie@example.com', 'CHARLIE@EXAMPLE.COM',
   TRUE, 'AQAAAAIAAYagAAAAEGTjAnmaZVF813SOGgIyZ7p1BatZ0thhc1XPavpen2SV5TQtbbNKjzZSoHrudSB5gg==', 'ss5', 'cs5', FALSE, FALSE, TRUE, 0,
   'Charlie Member', '2026-07-15T00:00:00Z', '2026-07-15T00:00:00Z');

-- ── USER ROLES ─────────────────────────────────────────────────────
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES
  ('11111111-1111-1111-1111-111111111111', '99999999-0000-0000-0000-000000000001'), -- alice -> admin
  ('22222222-2222-2222-2222-222222222222', '88888888-0000-0000-0000-000000000002'), -- author1 -> author
  ('33333333-3333-3333-3333-333333333333', '88888888-0000-0000-0000-000000000002'), -- author2 -> author
  ('44444444-4444-4444-4444-444444444444', '77777777-0000-0000-0000-000000000003'), -- bob -> member
  ('55555555-5555-5555-5555-555555555555', '77777777-0000-0000-0000-000000000003'); -- charlie -> member

-- ── BLOG POSTS ─────────────────────────────────────────────────────
INSERT INTO blog_posts (id, title, content, is_deleted, created_at, updated_at, author_id)
VALUES
  ('aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa',
   'Getting Started with ASP.NET Core',
   'ASP.NET Core is a cross-platform framework for building modern web applications. In this post, we explore the basics of middleware, dependency injection, and configuration.',
   FALSE, '2026-07-13T10:00:00Z', '2026-07-13T10:00:00Z',
   '22222222-2222-2222-2222-222222222222'),

  ('bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb',
   'Understanding PostgreSQL with EF Core',
   'Entity Framework Core with PostgreSQL (via Npgsql) offers a powerful ORM experience. This post covers setup, migrations, and common pitfalls.',
   FALSE, '2026-07-13T14:30:00Z', '2026-07-14T09:00:00Z',
   '22222222-2222-2222-2222-222222222222'),

  ('cccccccc-3333-3333-3333-cccccccccccc',
   'C# 12 Features You Should Know',
   'C# 12 introduces primary constructors, collection expressions, and interceptors. Here is a practical walkthrough with examples.',
   FALSE, '2026-07-14T11:00:00Z', '2026-07-14T11:00:00Z',
   '33333333-3333-3333-3333-333333333333'),

  ('dddddddd-4444-4444-4444-dddddddddddd',
   'Soft Delete Pattern in Entity Framework',
   'Soft deletes are a common pattern to preserve data integrity. Learn how to implement them cleanly with EF Core query filters.',
   FALSE, '2026-07-15T08:00:00Z', '2026-07-16T10:00:00Z',
   '22222222-2222-2222-2222-222222222222'),

  ('eeeeeeee-5555-5555-5555-eeeeeeeeeeee',
   'RESTful API Design Guidelines',
   'Designing a consistent and maintainable REST API requires thoughtful decisions on naming, status codes, pagination, and error handling.',
   FALSE, '2026-07-16T16:00:00Z', '2026-07-16T16:00:00Z',
   '33333333-3333-3333-3333-333333333333'),

  ('ffffffff-6666-6666-6666-ffffffffffff',
   'Deleted Post Example',
   'This post should not appear in normal queries because it is soft-deleted.',
   TRUE, '2026-07-17T00:00:00Z', '2026-07-17T00:00:00Z',
   '22222222-2222-2222-2222-222222222222');

-- ── COMMENTS ──────────────────────────────────────────────────────
INSERT INTO comments (id, content, is_deleted, created_at, updated_at, parent_comment_id, post_id, author_id)
VALUES
  -- Top-level comments on post 1
  ('c1111111-1111-1111-1111-c11111111111',
   'Great introduction! I especially liked the DI explanation.',
   FALSE, '2026-07-13T11:00:00Z', '2026-07-13T11:00:00Z',
   NULL, 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', '44444444-4444-4444-4444-444444444444'),

  ('c2222222-2222-2222-2222-c22222222222',
   'Could you add a section on testing?',
   FALSE, '2026-07-13T12:00:00Z', '2026-07-13T12:00:00Z',
   NULL, 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', '55555555-5555-5555-5555-555555555555'),

  -- Reply to first comment
  ('c3333333-3333-3333-3333-c33333333333',
   'Glad you liked it! I will cover testing in a follow-up post.',
   FALSE, '2026-07-13T13:00:00Z', '2026-07-13T13:00:00Z',
   'c1111111-1111-1111-1111-c11111111111', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222222'),

  -- Reply to the reply (nested 2 levels)
  ('c4444444-4444-4444-4444-c44444444444',
   'Looking forward to it!',
   FALSE, '2026-07-13T14:00:00Z', '2026-07-13T14:00:00Z',
   'c3333333-3333-3333-3333-c33333333333', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', '55555555-5555-5555-5555-555555555555'),

  -- Top-level comments on post 2
  ('c5555555-5555-5555-5555-c55555555555',
   'Npgsql has come a long way. Great write-up!',
   FALSE, '2026-07-14T10:00:00Z', '2026-07-14T10:00:00Z',
   NULL, 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb', '11111111-1111-1111-1111-111111111111'),

  -- Top-level comments on post 3
  ('c6666666-6666-6666-6666-c66666666666',
   'Collection expressions are a game changer for readability.',
   FALSE, '2026-07-14T15:00:00Z', '2026-07-14T15:00:00Z',
   NULL, 'cccccccc-3333-3333-3333-cccccccccccc', '44444444-4444-4444-4444-444444444444'),

  -- Top-level comments on post 4
  ('c7777777-7777-7777-7777-c77777777777',
   'Do you use a global query filter for this?',
   FALSE, '2026-07-15T12:00:00Z', '2026-07-15T12:00:00Z',
   NULL, 'dddddddd-4444-4444-4444-dddddddddddd', '55555555-5555-5555-5555-555555555555'),

  -- Reply
  ('c8888888-8888-8888-8888-c88888888888',
   'Yes, exactly. HasQueryFilter makes it seamless.',
   FALSE, '2026-07-15T14:00:00Z', '2026-07-15T14:00:00Z',
   'c7777777-7777-7777-7777-c77777777777', 'dddddddd-4444-4444-4444-dddddddddddd', '22222222-2222-2222-2222-222222222222'),

  -- Top-level comment on post 5
  ('c9999999-9999-9999-9999-c99999999999',
   'What about versioning strategies?',
   FALSE, '2026-07-17T08:00:00Z', '2026-07-17T08:00:00Z',
   NULL, 'eeeeeeee-5555-5555-5555-eeeeeeeeeeee', '11111111-1111-1111-1111-111111111111');

-- ── LIKES ──────────────────────────────────────────────────────────
-- Likes on posts (post_id set, comment_id NULL)
INSERT INTO likes (id, created_at, user_id, post_id, comment_id)
VALUES
  ('a0000001-0000-0000-0000-000000000001', '2026-07-13T12:00:00Z', '44444444-4444-4444-4444-444444444444', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', NULL),
  ('a0000002-0000-0000-0000-000000000002', '2026-07-13T13:00:00Z', '55555555-5555-5555-5555-555555555555', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', NULL),
  ('a0000003-0000-0000-0000-000000000003', '2026-07-13T14:00:00Z', '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa', NULL),
  ('a0000004-0000-0000-0000-000000000004', '2026-07-14T11:00:00Z', '22222222-2222-2222-2222-222222222222', 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb', NULL),
  ('a0000005-0000-0000-0000-000000000005', '2026-07-14T12:00:00Z', '44444444-4444-4444-4444-444444444444', 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb', NULL),
  ('a0000006-0000-0000-0000-000000000006', '2026-07-14T16:00:00Z', '11111111-1111-1111-1111-111111111111', 'cccccccc-3333-3333-3333-cccccccccccc', NULL),
  ('a0000007-0000-0000-0000-000000000007', '2026-07-15T09:00:00Z', '33333333-3333-3333-3333-333333333333', 'dddddddd-4444-4444-4444-dddddddddddd', NULL),
  ('a0000008-0000-0000-0000-000000000008', '2026-07-16T17:00:00Z', '22222222-2222-2222-2222-222222222222', 'eeeeeeee-5555-5555-5555-eeeeeeeeeeee', NULL),
  ('a0000009-0000-0000-0000-000000000009', '2026-07-16T18:00:00Z', '55555555-5555-5555-5555-555555555555', 'eeeeeeee-5555-5555-5555-eeeeeeeeeeee', NULL);

-- Likes on comments (comment_id set, post_id NULL)
INSERT INTO likes (id, created_at, user_id, post_id, comment_id)
VALUES
  ('a0000010-0000-0000-0000-000000000010', '2026-07-13T12:30:00Z', '22222222-2222-2222-2222-222222222222', NULL, 'c1111111-1111-1111-1111-c11111111111'),
  ('a0000011-0000-0000-0000-000000000011', '2026-07-13T13:30:00Z', '44444444-4444-4444-4444-444444444444', NULL, 'c3333333-3333-3333-3333-c33333333333'),
  ('a0000012-0000-0000-0000-000000000012', '2026-07-14T11:30:00Z', '22222222-2222-2222-2222-222222222222', NULL, 'c5555555-5555-5555-5555-c55555555555'),
  ('a0000013-0000-0000-0000-000000000013', '2026-07-15T13:00:00Z', '33333333-3333-3333-3333-333333333333', NULL, 'c7777777-7777-7777-7777-c77777777777'),
  ('a0000014-0000-0000-0000-000000000014', '2026-07-17T09:00:00Z', '22222222-2222-2222-2222-222222222222', NULL, 'c9999999-9999-9999-9999-c99999999999');

COMMIT;
