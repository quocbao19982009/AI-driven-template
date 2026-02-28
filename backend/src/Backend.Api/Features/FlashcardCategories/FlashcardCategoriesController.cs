using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.FlashcardCategories;

[ApiController]
[Route("api/flashcard-categories")]
public class FlashcardCategoriesController : ControllerBase
{
    private readonly IFlashcardCategoriesService _flashcardCategoriesService;

    public FlashcardCategoriesController(IFlashcardCategoriesService flashcardCategoriesService)
    {
        _flashcardCategoriesService = flashcardCategoriesService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FlashcardCategoryDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _flashcardCategoriesService.GetAllAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<FlashcardCategoryDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<FlashcardCategoryDto>>>> GetAllUnpaged(
        CancellationToken cancellationToken = default)
    {
        var result = await _flashcardCategoriesService.GetAllUnpagedAsync(cancellationToken);
        return Ok(ApiResponse<List<FlashcardCategoryDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FlashcardCategoryDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardCategoriesService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<FlashcardCategoryDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FlashcardCategoryDto>>> Create(
        CreateFlashcardCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardCategoriesService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<FlashcardCategoryDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FlashcardCategoryDto>>> Update(
        int id, UpdateFlashcardCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _flashcardCategoriesService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FlashcardCategoryDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _flashcardCategoriesService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
