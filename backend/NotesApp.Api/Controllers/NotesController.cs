using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NotesApp.Api.Common;
using NotesApp.Core.Exceptions;
using NotesApp.Core.DTOs;
using NotesApp.Core.Models;
using NotesApp.Data.Repositories;

namespace NotesApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteRepository _noteRepository;

    public NotesController(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedException("Invalid user identity.");

        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        var notes = await _noteRepository.GetUserNotesAsync(userId);
        return Ok(ApiResponse.Success(notes, "Notes fetched successfully.", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            throw new BadRequestException("Id must be greater than zero.");

        var userId = GetCurrentUserId();
        var note = await _noteRepository.GetNoteByIdAsync(id, userId);
        
        if (note == null)
            throw new NotFoundException("Note not found.");
            
        return Ok(ApiResponse.Success(note, "Note fetched successfully.", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NoteCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BadRequestException("Title is required.");

        var note = new Note
        {
            Title = dto.Title.Trim(),
            Content = dto.Content?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = GetCurrentUserId()
        };

        var createdId = await _noteRepository.CreateNoteAsync(note);
        note.Id = createdId;

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdId },
            ApiResponse.Success(note, "Note created successfully.", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] NoteUpdateDto dto)
    {
        if (id <= 0)
            throw new BadRequestException("Id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BadRequestException("Title is required.");

        var userId = GetCurrentUserId();
        var existingNote = await _noteRepository.GetNoteByIdAsync(id, userId);
        
        if (existingNote == null)
            throw new NotFoundException("Note not found.");

        existingNote.Title = dto.Title.Trim();
        existingNote.Content = dto.Content?.Trim() ?? string.Empty;
        existingNote.UpdatedAt = DateTime.UtcNow;

        await _noteRepository.UpdateNoteAsync(existingNote);

        return Ok(ApiResponse.Success(existingNote, "Note updated successfully.", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            throw new BadRequestException("Id must be greater than zero.");

        var userId = GetCurrentUserId();
        var rowsAffected = await _noteRepository.DeleteNoteAsync(id, userId);
        
        if (rowsAffected == 0)
            throw new NotFoundException("Note not found.");

        return Ok(ApiResponse.Success<object?>(null, "Note deleted successfully.", HttpContext.TraceIdentifier));
    }
}
