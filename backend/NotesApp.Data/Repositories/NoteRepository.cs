using Dapper;
using NotesApp.Core.Models;

namespace NotesApp.Data.Repositories;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetUserNotesAsync(int userId);
    Task<Note?> GetNoteByIdAsync(int id, int userId);
    Task<int> CreateNoteAsync(Note note);
    Task<int> UpdateNoteAsync(Note note);
    Task<int> DeleteNoteAsync(int id, int userId);
}

public class NoteRepository : INoteRepository
{
    private readonly DapperContext _context;

    public NoteRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Note>> GetUserNotesAsync(int userId)
    {
        var query = @"
            SELECT Id, Title, Content, CreatedAt, UpdatedAt, UserId
            FROM Notes
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Note>(query, new { UserId = userId });
    }

    public async Task<Note?> GetNoteByIdAsync(int id, int userId)
    {
        var query = @"
            SELECT Id, Title, Content, CreatedAt, UpdatedAt, UserId
            FROM Notes
            WHERE Id = @Id AND UserId = @UserId";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Note>(query, new { Id = id, UserId = userId });
    }

    public async Task<int> CreateNoteAsync(Note note)
    {
        var query = @"
            INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt, UserId)
            VALUES (@Title, @Content, @CreatedAt, @UpdatedAt, @UserId);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, note);
    }

    public async Task<int> UpdateNoteAsync(Note note)
    {
        var query = @"
            UPDATE Notes 
            SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND UserId = @UserId";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(query, note);
    }

    public async Task<int> DeleteNoteAsync(int id, int userId)
    {
        var query = "DELETE FROM Notes WHERE Id = @Id AND UserId = @UserId";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(query, new { Id = id, UserId = userId });
    }
}
