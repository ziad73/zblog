-- ============================================
-- Benchmark Seed Data for zblog
-- 100 posts · 500 comments · 1000 likes
-- Uses existing users (does NOT insert users)
-- ============================================

BEGIN;

-- Clean existing benchmark data while preserving users
TRUNCATE likes, comments, blog_posts RESTART IDENTITY CASCADE;

-- ============================================
-- 100 blog posts
-- ============================================
-- Author distribution: author1=50, author2=30, alice=20
-- created_at spread over 90 days
INSERT INTO blog_posts (id, title, content, is_deleted, created_at, updated_at, author_id)
SELECT
  gen_random_uuid(),
  'Benchmark Post #' || n,
  'This is the full content of benchmark post number ' || n || '. It contains multiple sentences to simulate a realistic blog post body. Readers will find insights about software development, best practices, and architectural patterns discussed in detail across several paragraphs.',
  false,
  now() - (interval '1 day' * (91 - n)),
  now(),
  CASE
    WHEN n <= 50 THEN '22222222-2222-2222-2222-222222222222'
    WHEN n <= 80 THEN '33333333-3333-3333-3333-333333333333'
    ELSE '11111111-1111-1111-1111-111111111111'
  END
FROM generate_series(1, 100) AS n;

-- ============================================
-- 400 top-level comments (4 per post)
-- ============================================
-- Authors: bob=40%, charlie=35%, alice=15%, author1=10%
INSERT INTO comments (id, content, is_deleted, created_at, updated_at, post_id, author_id, parent_comment_id)
SELECT
  gen_random_uuid(),
  'Comment on post. This provides feedback and discussion about the blog post content and its implications for real-world projects.',
  false,
  now() - ((random() * 0.8 + 0.1) * interval '85 days'),
  now(),
  p.id,
  (ARRAY[
    '44444444-4444-4444-4444-444444444444',
    '55555555-5555-5555-5555-555555555555',
    '11111111-1111-1111-1111-111111111111',
    '22222222-2222-2222-2222-222222222222'
  ])[ceil(random() * 4)::int]::uuid,
  NULL
FROM blog_posts p
CROSS JOIN generate_series(1, 4) AS n
WHERE p.is_deleted = false;

-- ============================================
-- 100 nested replies (1 reply per top-level comment)
-- ============================================
-- Authors: bob=40%, charlie=35%, alice=25%
INSERT INTO comments (id, content, is_deleted, created_at, updated_at, post_id, author_id, parent_comment_id)
SELECT
  gen_random_uuid(),
  'Reply to comment. This adds further discussion and expands on the points raised in the parent comment.',
  false,
  c.created_at + interval '1 hour',
  now(),
  c.post_id,
  (ARRAY[
    '44444444-4444-4444-4444-444444444444',
    '55555555-5555-5555-5555-555555555555',
    '11111111-1111-1111-1111-111111111111'
  ])[ceil(random() * 3)::int]::uuid,
  c.id
FROM (
  SELECT id, post_id, created_at
  FROM comments
  WHERE parent_comment_id IS NULL
  ORDER BY random()
  LIMIT 100
) c;

-- ============================================
-- 500 post likes (every user likes every post)
-- ============================================
INSERT INTO likes (id, created_at, user_id, post_id, comment_id)
SELECT
  gen_random_uuid(),
  now() - (random() * interval '90 days'),
  u."Id",
  p.id,
  NULL
FROM "AspNetUsers" u
CROSS JOIN blog_posts p
WHERE p.is_deleted = false;

-- ============================================
-- 500 comment likes (each user likes 100 comments)
-- ============================================
INSERT INTO likes (id, created_at, user_id, post_id, comment_id)
SELECT
  gen_random_uuid(),
  now() - (random() * interval '90 days'),
  u."Id",
  NULL,
  c.id
FROM "AspNetUsers" u
CROSS JOIN (
  SELECT id FROM comments ORDER BY id LIMIT 100
) c;

COMMIT;

-- ============================================
-- Verify seed data
-- ============================================
SELECT 'posts' AS tbl, COUNT(*) FROM blog_posts
UNION ALL
SELECT 'comments', COUNT(*) FROM comments
UNION ALL
SELECT 'likes', COUNT(*) FROM likes;
