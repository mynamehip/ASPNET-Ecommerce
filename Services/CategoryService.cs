using ASPNET_Ecommerce.Data;
using ASPNET_Ecommerce.Models.Catalog;
using ASPNET_Ecommerce.Models.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace ASPNET_Ecommerce.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Include(category => category.Products)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .Include(category => category.Products.Where(p => p.Status == ProductStatus.Active))
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new CategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task CreateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueNameAsync(model.Name, model.Id, cancellationToken);

        var category = new Category
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories.FindAsync([model.Id], cancellationToken);
        if (category is null)
        {
            throw new InvalidOperationException("Category not found.");
        }

        await EnsureUniqueNameAsync(model.Name, model.Id, cancellationToken);

        category.Name = model.Name.Trim();
        category.Description = model.Description?.Trim();
        category.DisplayOrder = model.DisplayOrder;
        category.IsActive = model.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(item => item.Products)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (category.Products.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete a category that still contains products.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueNameAsync(string name, int currentId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        var exists = await _dbContext.Categories
            .AnyAsync(category => category.Id != currentId && category.Name == normalizedName, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Category name already exists.");
        }
    }
}