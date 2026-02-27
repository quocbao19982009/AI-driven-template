using Backend.Common.Models;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Scheduling;

[ApiController]
[Route("api/scheduling")]
public class SchedulingController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SchedulingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-person")]
    public async Task<ActionResult<ApiResponse<List<PersonScheduleDto>>>> ByPerson(
        CancellationToken cancellationToken = default)
    {
        var reservationPersonnel = await _context.ReservationPersonnel
            .AsNoTracking()
            .Include(rp => rp.Reservation)
            .ToListAsync(cancellationToken);

        var grouped = reservationPersonnel
            .GroupBy(rp => new { rp.PersonId, rp.PersonDisplayName })
            .Select(g =>
            {
                var reservations = g.Select(rp => new ReservationSummaryDto(
                    rp.ReservationId,
                    rp.Reservation.StartTime,
                    rp.Reservation.EndTime,
                    (rp.Reservation.EndTime - rp.Reservation.StartTime).TotalHours,
                    rp.Reservation.FactoryDisplayName
                )).OrderBy(r => r.StartTime).ToList();

                return new PersonScheduleDto(
                    g.Key.PersonId,
                    g.Key.PersonDisplayName,
                    reservations,
                    reservations.Sum(r => r.DurationHours)
                );
            })
            .OrderBy(p => p.PersonName)
            .ToList();

        return Ok(ApiResponse<List<PersonScheduleDto>>.Ok(grouped));
    }

    [HttpGet("by-factory")]
    public async Task<ActionResult<ApiResponse<List<FactoryScheduleDto>>>> ByFactory(
        CancellationToken cancellationToken = default)
    {
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Include(r => r.ReservationPersonnel)
            .ToListAsync(cancellationToken);

        var grouped = reservations
            .GroupBy(r => new { r.FactoryId, r.FactoryDisplayName })
            .Select(g =>
            {
                // Total hours = sum of (duration * person count) per reservation
                double totalHours = g.Sum(r =>
                    (r.EndTime - r.StartTime).TotalHours * r.ReservationPersonnel.Count);

                return new FactoryScheduleDto(
                    g.Key.FactoryId,
                    g.Key.FactoryDisplayName,
                    totalHours,
                    g.Count()
                );
            })
            .OrderBy(f => f.FactoryName)
            .ToList();

        return Ok(ApiResponse<List<FactoryScheduleDto>>.Ok(grouped));
    }
}
