using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Flashcards;

[ApiController]
[Route("api/flashcards")]
public class FlashcardsController : ControllerBase
{
    private readonly IFlashcardsService _flashcardsService;

    public FlashcardsController(IFlashcardsService flashcardsService)
    {
        _flashcardsService = flashcardsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FlashcardDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _flashcardsService.GetAllAsync(page, pageSize, search, category, cancellationToken);
        return Ok(ApiResponse<PagedResult<FlashcardDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FlashcardDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardsService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<FlashcardDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FlashcardDto>>> Create(
        CreateFlashcardRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<FlashcardDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FlashcardDto>>> Update(
        int id, UpdateFlashcardRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardsService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FlashcardDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _flashcardsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/review")]
    public async Task<ActionResult<ApiResponse<FlashcardDto>>> MarkReviewed(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardsService.MarkReviewedAsync(id, cancellationToken);
        return Ok(ApiResponse<FlashcardDto>.Ok(item));
    }
}
