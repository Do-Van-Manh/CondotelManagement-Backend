using CondotelManagement.Data;
using CondotelManagement.DTOs.Blog;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces; // Sửa Using
using CondotelManagement.Repositories.Interfaces.Admin;
using CondotelManagement.Services.Interfaces.Blog; // Sửa Using
using Microsoft.EntityFrameworkCore;
using System.Text; // Thêm
using System.Text.RegularExpressions;

namespace CondotelManagement.Services.Implementations.Blog
{
    public class BlogService : IBlogService
    {
        private readonly CondotelDbVer1Context _context;
        private readonly IRepository<BlogPost> _postRepo;
        private readonly IRepository<BlogCategory> _categoryRepo;
        private readonly IRepository<User> _userRepo; // Thêm User Repo

        public BlogService(CondotelDbVer1Context context,
                               IRepository<BlogPost> postRepo,
                               IRepository<BlogCategory> categoryRepo,
                               IRepository<User> userRepo) // Thêm
        {
            _context = context;
            _postRepo = postRepo;
            _categoryRepo = categoryRepo;
            _userRepo = userRepo; // Thêm
        }

        // === Public Methods (Giữ nguyên) ===
        public async Task<IEnumerable<BlogPostSummaryDto>> GetPublishedPostsAsync()
        {
            // ... (Code đã chuẩn) ...
            return await _context.BlogPosts
                .Where(p => p.Status == "Published")
                .Include(p => p.AuthorUser)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PublishedAt)
                .Select(p => new BlogPostSummaryDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Slug = p.Slug,
                    FeaturedImageUrl = p.FeaturedImageUrl,
                    PublishedAt = p.PublishedAt,
                    AuthorName = p.AuthorUser.FullName,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
                })
                .ToListAsync();
        }

        public async Task<BlogPostDetailDto?> GetPostBySlugAsync(string slug)
        {
            // ... (Code đã chuẩn) ...
            return await _context.BlogPosts
                .Where(p => p.Slug == slug && p.Status == "Published")
                .Include(p => p.AuthorUser)
                .Include(p => p.Category)
                .Select(p => new BlogPostDetailDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Slug = p.Slug,
                    Content = p.Content,
                    FeaturedImageUrl = p.FeaturedImageUrl,
                    PublishedAt = p.PublishedAt,
                    AuthorName = p.AuthorUser.FullName,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BlogCategoryDto>> GetCategoriesAsync()
        {
            // ... (Code đã chuẩn, dùng _context) ...
            return await _context.BlogCategories
                .Select(c => new BlogCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync();
        }

        // === Admin Post Methods ===

        // THÊM MỚI: Hàm GetById cho Admin
        public async Task<BlogPostDetailDto?> AdminGetPostByIdAsync(int postId)
        {
            return await _context.BlogPosts
               .Where(p => p.PostId == postId) // Lấy cả bài nháp
               .Include(p => p.AuthorUser)
               .Include(p => p.Category)
               .Select(p => new BlogPostDetailDto
               {
                   PostId = p.PostId,
                   Title = p.Title,
                   Slug = p.Slug,
                   Content = p.Content,
                   FeaturedImageUrl = p.FeaturedImageUrl,
                   PublishedAt = p.PublishedAt,
                   AuthorName = p.AuthorUser.FullName,
                   CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
               })
               .FirstOrDefaultAsync();
        }

        public async Task<BlogPostDetailDto?> AdminCreatePostAsync(AdminBlogCreateDto dto, int authorUserId)
        {
            // Sửa: Dùng _context thay vì _categoryRepo.GetByIdAsync
            var category = dto.CategoryId.HasValue ? await _context.BlogCategories.FindAsync(dto.CategoryId.Value) : null;
            var author = await _userRepo.GetByIdAsync(authorUserId); // Dùng User Repo
            if (author == null) return null;

            var slug = await GenerateUniqueSlugAsync(dto.Title);

            var newPost = new BlogPost
            {
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                FeaturedImageUrl = dto.FeaturedImageUrl,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                AuthorUserId = authorUserId,
                CategoryId = category?.CategoryId
            };

            if (newPost.Status == "Published")
            {
                newPost.PublishedAt = DateTime.UtcNow;
            }

            await _postRepo.AddAsync(newPost);

            return new BlogPostDetailDto
            {
                PostId = newPost.PostId,
                Title = newPost.Title,
                Slug = newPost.Slug,
                Content = newPost.Content,
                FeaturedImageUrl = newPost.FeaturedImageUrl,
                PublishedAt = newPost.PublishedAt,
                AuthorName = author.FullName,
                CategoryName = category?.Name ?? "Uncategorized"
            };
        }

        public async Task<BlogPostDetailDto?> AdminUpdatePostAsync(int postId, AdminBlogCreateDto dto)
        {
            var post = await _postRepo.GetByIdAsync(postId);
            if (post == null) return null;

            var category = dto.CategoryId.HasValue ? await _context.BlogCategories.FindAsync(dto.CategoryId.Value) : null;
            var author = await _userRepo.GetByIdAsync(post.AuthorUserId);

            if (post.Title != dto.Title)
            {
                post.Slug = await GenerateUniqueSlugAsync(dto.Title, postId); // Thêm postId để loại trừ chính nó
            }

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.FeaturedImageUrl = dto.FeaturedImageUrl;
            post.CategoryId = category?.CategoryId;

            if (post.Status != "Published" && dto.Status == "Published")
            {
                post.PublishedAt = DateTime.UtcNow;
            }
            post.Status = dto.Status;

            await _postRepo.UpdateAsync(post);

            return new BlogPostDetailDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                FeaturedImageUrl = post.FeaturedImageUrl,
                PublishedAt = post.PublishedAt,
                AuthorName = author.FullName,
                CategoryName = category?.Name ?? "Uncategorized"
            };
        }

        public async Task<bool> AdminDeletePostAsync(int postId)
        {
            var post = await _postRepo.GetByIdAsync(postId);
            if (post == null) return false;
            await _postRepo.DeleteAsync(post);
            return true;
        }

        // === HIỆN THỰC CÁC HÀM CATEGORY ===

        public async Task<BlogCategoryDto?> AdminCreateCategoryAsync(BlogCategoryDto dto)
        {
            var slug = await GenerateUniqueCategorySlugAsync(dto.Name);
            var newCategory = new BlogCategory
            {
                Name = dto.Name,
                Slug = slug
            };

            await _categoryRepo.AddAsync(newCategory);

            dto.CategoryId = newCategory.CategoryId;
            dto.Slug = newCategory.Slug;
            return dto;
        }

        public async Task<BlogCategoryDto?> AdminUpdateCategoryAsync(int categoryId, BlogCategoryDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId);
            if (category == null) return null;

            if (category.Name != dto.Name)
            {
                category.Slug = await GenerateUniqueCategorySlugAsync(dto.Name, categoryId);
            }

            category.Name = dto.Name;

            await _categoryRepo.UpdateAsync(category);

            dto.CategoryId = category.CategoryId;
            dto.Slug = category.Slug;
            return dto;
        }

        public async Task<bool> AdminDeleteCategoryAsync(int categoryId)
        {
            // Lưu ý: DB của bạn đã set ON DELETE SET NULL,
            // nên xóa Category là an toàn, bài viết sẽ tự động cập nhật CategoryID = NULL
            var category = await _categoryRepo.GetByIdAsync(categoryId);
            if (category == null) return false;

            await _categoryRepo.DeleteAsync(category);
            return true;
        }


        // === Private Helper ===

        private async Task<string> GenerateUniqueSlugAsync(string title, int? postIdToExclude = null)
        {
            var slug = GenerateSlug(title);
            var originalSlug = slug;
            int count = 1;

            var query = _context.BlogPosts.Where(p => p.Slug == slug);
            if (postIdToExclude.HasValue)
            {
                // Khi update, loại trừ chính bài viết đó
                query = query.Where(p => p.PostId != postIdToExclude.Value);
            }

            while (await query.AnyAsync())
            {
                slug = $"{originalSlug}-{count}";
                count++;

                query = _context.BlogPosts.Where(p => p.Slug == slug);
                if (postIdToExclude.HasValue)
                {
                    query = query.Where(p => p.PostId != postIdToExclude.Value);
                }
            }
            return slug;
        }

        // Helper cho Category Slug
        private async Task<string> GenerateUniqueCategorySlugAsync(string name, int? categoryIdToExclude = null)
        {
            var slug = GenerateSlug(name);
            var originalSlug = slug;
            int count = 1;

            var query = _context.BlogCategories.Where(p => p.Slug == slug);
            if (categoryIdToExclude.HasValue)
            {
                query = query.Where(p => p.CategoryId != categoryIdToExclude.Value);
            }

            while (await query.AnyAsync())
            {
                slug = $"{originalSlug}-{count}";
                count++;

                query = _context.BlogCategories.Where(p => p.Slug == slug);
                if (categoryIdToExclude.HasValue)
                {
                    query = query.Where(p => p.CategoryId != categoryIdToExclude.Value);
                }
            }
            return slug;
        }

        private string GenerateSlug(string phrase)
        {
            string str = RemoveAccents(phrase).ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        // SỬA LỖI 3: Hoàn thiện hàm RemoveAccents, xóa bỏ '...'
        private string RemoveAccents(string text)
        {
            var sb = new StringBuilder(text);
            string[,] replacements = {
                {"à","a"}, {"á","a"}, {"ạ","a"}, {"ả","a"}, {"ã","a"}, {"â","a"}, {"ầ","a"}, {"ấ","a"}, {"ậ","a"}, {"ẩ","a"}, {"ẫ","a"}, {"ă","a"}, {"ằ","a"}, {"ắ","a"}, {"ặ","a"}, {"ẳ","a"}, {"ẵ","a"},
                {"è","e"}, {"é","e"}, {"ẹ","e"}, {"ẻ","e"}, {"ẽ","e"}, {"ê","e"}, {"ề","e"}, {"ế","e"}, {"ệ","e"}, {"ể","e"}, {"ễ","e"},
                {"ì","i"}, {"í","i"}, {"ị","i"}, {"ỉ","i"}, {"ĩ","i"},
                {"ò","o"}, {"ó","o"}, {"ọ","o"}, {"ỏ","o"}, {"õ","o"}, {"ô","o"}, {"ồ","o"}, {"ố","o"}, {"ộ","o"}, {"ổ","o"}, {"ỗ","o"}, {"ơ","o"}, {"ờ","o"}, {"ớ","o"}, {"ợ","o"}, {"ở","o"}, {"ỡ","o"},
                {"ù","u"}, {"ú","u"}, {"ụ","u"}, {"ủ","u"}, {"ũ","u"}, {"ư","u"}, {"ừ","u"}, {"ứ","u"}, {"ự","u"}, {"ử","u"}, {"ữ","u"},
                {"ỳ","y"}, {"ý","y"}, {"ỵ","y"}, {"ỷ","y"}, {"ỹ","y"},
                {"đ","d"},
                {"À","A"}, {"Á","A"}, {"Ạ","A"}, {"Ả","A"}, {"Ã","A"}, {"Â","A"}, {"Ầ","A"}, {"Ấ","A"}, {"Ậ","A"}, {"Ẩ","A"}, {"Ẫ","A"}, {"Ă","A"}, {"Ằ","A"}, {"Ắ","A"}, {"Ặ","A"}, {"Ẳ","A"}, {"Ẵ","A"},
                {"È","E"}, {"É","E"}, {"Ẹ","E"}, {"Ẻ","E"}, {"Ẽ","E"}, {"Ê","E"}, {"Ề","E"}, {"Ế","E"}, {"Ệ","E"}, {"Ể","E"}, {"Ễ","E"},
                {"Ì","I"}, {"Í","I"}, {"Ị","I"}, {"Ỉ","I"}, {"Ĩ","I"},
                {"Ò","O"}, {"Ó","O"}, {"Ọ","O"}, {"Ỏ","O"}, {"Õ","O"}, {"Ô","O"}, {"Ồ","O"}, {"Ố","O"}, {"Ộ","O"}, {"Ổ","O"}, {"Ỗ","O"}, {"Ơ","O"}, {"Ờ","O"}, {"Ớ","O"}, {"Ợ","O"}, {"Ở","O"}, {"Ỡ","O"},
                {"Ù","U"}, {"Ú","U"}, {"Ụ","U"}, {"Ủ","U"}, {"Ũ","U"}, {"Ư","U"}, {"Ừ","U"}, {"Ứ","U"}, {"Ự","U"}, {"Ử","U"}, {"Ữ","U"},
                {"Ỳ","Y"}, {"Ý","Y"}, {"Ỵ","Y"}, {"Ỷ","Y"}, {"Ỹ","Y"},
                {"Đ","D"}
            };

            for (int i = 0; i < replacements.GetLength(0); i++)
            {
                sb.Replace(replacements[i, 0], replacements[i, 1]);
            }

            return sb.ToString();
        }
    }
}