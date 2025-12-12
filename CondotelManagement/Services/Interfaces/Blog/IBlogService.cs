using CondotelManagement.DTOs.Blog;
using CondotelManagement.Models;

namespace CondotelManagement.Services.Interfaces.Blog
{
    public interface IBlogService
    {
        // === Public Methods ===
        Task<IEnumerable<BlogPostSummaryDto>> GetPublishedPostsAsync();
        Task<BlogPostDetailDto?> GetPostBySlugAsync(string slug);
        Task<IEnumerable<BlogCategoryDto>> GetCategoriesAsync();

        // === Admin Post Methods ===
        Task<BlogPostDetailDto?> AdminGetPostByIdAsync(int postId); // THÊM MỚI
        Task<BlogPostDetailDto?> AdminCreatePostAsync(AdminBlogCreateDto dto, int authorUserId);
        Task<BlogPostDetailDto?> AdminUpdatePostAsync(int postId, AdminBlogCreateDto dto);
        Task<bool> AdminDeletePostAsync(int postId);

        // === Admin Category Methods ===
        Task<BlogCategoryDto?> AdminCreateCategoryAsync(BlogCategoryDto dto);
        Task<BlogCategoryDto?> AdminUpdateCategoryAsync(int categoryId, BlogCategoryDto dto);
        Task<bool> AdminDeleteCategoryAsync(int categoryId);
        Task<IEnumerable<BlogPostSummaryDto>> AdminGetAllPostsAsync(bool includeDrafts = true);
    }
}
