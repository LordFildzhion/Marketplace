using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var emailObj = new Email(email);
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == emailObj, ct);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
    {
        var emailObj = new Email(email);
        var query = _context.Users.Where(u => u.Email == emailObj);
        if (excludeId.HasValue) query = query.Where(u => u.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Users.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Users.ToListAsync(ct);

    public async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(User entity, CancellationToken ct = default)
    {
        _context.Users.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User entity, CancellationToken ct = default)
    {
        _context.Users.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Users.AnyAsync(u => u.Id == id, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
