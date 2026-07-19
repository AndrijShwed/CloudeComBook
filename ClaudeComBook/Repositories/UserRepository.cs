using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _db;
    public UserRepository(DbConnectionFactory db) => _db = db;

    public async Task<User?> GetByLoginAsync(string login)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            @"SELECT 
            id AS Id,
            login AS Login,
            password_hash AS PasswordHash,
            full_name AS FullName,
            role AS Role,
            is_active AS IsActive,
            created_at AS CreatedAt
          FROM users 
          WHERE login = @login AND is_active = 1",
            new { login });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE id = @id", new { id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            "SELECT * FROM users ORDER BY login");
    }

    public async Task<int> CreateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO users (login, password_hash, full_name, role, is_active, created_at)
              VALUES (@Login, @PasswordHash, @FullName, @Role, @IsActive, @CreatedAt);
              SELECT LAST_INSERT_ID();", user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE users SET full_name=@FullName, role=@Role WHERE id=@Id", user);
        return rows > 0;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE users SET is_active=@isActive WHERE id=@id", new { id, isActive });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM users WHERE id=@id", new { id });
        return rows > 0;
    }
}
