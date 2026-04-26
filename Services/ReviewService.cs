using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.Orders;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using ASPNET_Ecommerce.Models.ViewModels.Products;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _dbContext;

    public ReviewService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductReview>> GetByProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ProductReviewStatus.Approved)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductReview?> GetUserReviewAsync(int productId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductReviews
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
    }

    public async Task<(bool CanReview, string? Message)> GetReviewEligibilityAsync(int productId, string userId, CancellationToken cancellationToken = default)
    {
        var alreadyReviewed = await _dbContext.ProductReviews
            .AsNoTracking()
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);

        if (alreadyReviewed)
        {
            return (false, "You have already reviewed this product.");
        }

        var hasVerifiedPurchase = await _dbContext.Orders
            .AsNoTracking()
            .AnyAsync(order =>
                order.UserId == userId &&
                order.Status == OrderStatus.Completed &&
                order.Items.Any(item => item.ProductId == productId),
                cancellationToken);

        if (!hasVerifiedPurchase)
        {
            return (false, "Only verified customers with a completed purchase can review this product.");
        }

        return (true, null);
    }

    public async Task CreateAsync(string userId, ReviewFormViewModel model, CancellationToken cancellationToken = default)
    {
        var eligibility = await GetReviewEligibilityAsync(model.ProductId, userId, cancellationToken);
        if (!eligibility.CanReview)
        {
            throw new InvalidOperationException(eligibility.Message ?? "You cannot review this product.");
        }

        var productExists = await _dbContext.Products
            .AnyAsync(p => p.Id == model.ProductId && p.Status == ProductStatus.Active, cancellationToken);

        if (!productExists)
        {
            throw new InvalidOperationException("Product not found.");
        }

        var review = new ProductReview
        {
            ProductId = model.ProductId,
            UserId = userId,
            Rating = model.Rating,
            Comment = string.IsNullOrWhiteSpace(model.Comment) ? null : model.Comment.Trim(),
            Status = ProductReviewStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.ProductReviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(double AverageRating, int TotalCount)> GetStatsAsync(int productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _dbContext.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == ProductReviewStatus.Approved)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
            return (0, 0);

        return (reviews.Average(), reviews.Count);
    }

    public async Task<ReviewAdminListViewModel> GetAdminListAsync(string? search, ProductReviewStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProductReviews
            .AsNoTracking()
            .Include(review => review.Product)
            .Include(review => review.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(review =>
                review.Product!.Name.Contains(term) ||
                review.User!.FullName.Contains(term) ||
                review.User.Email!.Contains(term) ||
                (review.Comment != null && review.Comment.Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(review => review.Status == status.Value);
        }

        var reviews = await query
            .OrderBy(review => review.Status)
            .ThenByDescending(review => review.CreatedAtUtc)
            .Select(review => new ReviewAdminListItemViewModel
            {
                ReviewId = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product != null ? review.Product.Name : "Unknown product",
                UserId = review.UserId,
                UserFullName = review.User != null ? review.User.FullName : "Unknown user",
                Rating = review.Rating,
                CommentPreview = string.IsNullOrWhiteSpace(review.Comment)
                    ? null
                    : review.Comment.Length <= 120 ? review.Comment : review.Comment.Substring(0, 120) + "...",
                Status = review.Status,
                CreatedAtUtc = review.CreatedAtUtc,
                UpdatedAtUtc = review.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new ReviewAdminListViewModel
        {
            Search = search,
            Status = status,
            Reviews = reviews
        };
    }

    public async Task<ReviewAdminDetailsViewModel?> GetAdminDetailsAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _dbContext.ProductReviews
            .AsNoTracking()
            .Include(item => item.Product)
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.Id == reviewId, cancellationToken);

        if (review is null)
        {
            return null;
        }

        return new ReviewAdminDetailsViewModel
        {
            ReviewId = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product?.Name ?? "Unknown product",
            UserId = review.UserId,
            UserFullName = review.User?.FullName ?? "Unknown user",
            UserEmail = review.User?.Email,
            Rating = review.Rating,
            Comment = review.Comment,
            AdminReply = review.AdminReply,
            Status = review.Status,
            CreatedAtUtc = review.CreatedAtUtc,
            AdminRepliedAtUtc = review.AdminRepliedAtUtc,
            UpdatedAtUtc = review.UpdatedAtUtc,
            StatusForm = new ReviewStatusUpdateViewModel
            {
                ReviewId = review.Id,
                Status = review.Status,
                AdminReply = review.AdminReply
            }
        };
    }

    public async Task UpdateStatusAsync(int reviewId, ReviewStatusUpdateViewModel model, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(model.Status))
        {
            throw new InvalidOperationException("Invalid review status.");
        }

        if (model.Status == ProductReviewStatus.Pending)
        {
            throw new InvalidOperationException("Pending approval is no longer used. Choose Approved or Hidden.");
        }

        var review = await _dbContext.ProductReviews
            .SingleOrDefaultAsync(item => item.Id == reviewId, cancellationToken);

        if (review is null)
        {
            throw new InvalidOperationException("Review not found.");
        }

        var normalizedReply = string.IsNullOrWhiteSpace(model.AdminReply)
            ? null
            : model.AdminReply.Trim();

        var statusChanged = review.Status != model.Status;
        var replyChanged = !string.Equals(review.AdminReply, normalizedReply, StringComparison.Ordinal);

        if (!statusChanged && !replyChanged)
        {
            return;
        }

        var now = DateTime.UtcNow;

        review.Status = model.Status;
        review.AdminReply = normalizedReply;
        review.UpdatedAtUtc = now;

        if (replyChanged)
        {
            review.AdminRepliedAtUtc = normalizedReply is null ? null : now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _dbContext.ProductReviews
            .SingleOrDefaultAsync(item => item.Id == reviewId, cancellationToken);

        if (review is null)
        {
            throw new InvalidOperationException("Review not found.");
        }

        _dbContext.ProductReviews.Remove(review);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
