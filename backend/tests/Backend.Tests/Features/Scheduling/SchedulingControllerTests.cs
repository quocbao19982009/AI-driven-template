using Backend.Common.Models;
using Backend.Data;
using Backend.Features.Factories;
using Backend.Features.Personnel;
using Backend.Features.Reservations;
using Backend.Features.Scheduling;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Features.Scheduling;

public class SchedulingControllerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Seeds two persons, two factories, and two reservations with personnel, then returns the context.
    /// </summary>
    private static async Task<ApplicationDbContext> CreateSeededContext()
    {
        var context = CreateContext();

        var factoryA = new Factory { Id = 1, Name = "Alpha Factory", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        var factoryB = new Factory { Id = 2, Name = "Beta Factory", TimeZone = "UTC+1", CreatedAt = DateTime.UtcNow };
        context.Factories.AddRange(factoryA, factoryB);

        var personAlice = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [factoryA], CreatedAt = DateTime.UtcNow
        };
        var personBob = new Person
        {
            Id = 2, PersonalId = "P002", FullName = "Bob Jones",
            Email = "bob@test.com", AllowedFactories = [factoryA, factoryB], CreatedAt = DateTime.UtcNow
        };
        context.Personnel.AddRange(personAlice, personBob);

        // Reservation 1: Factory A, 8 hours, 2 personnel → 8 * 2 = 16 factory hours
        var start1 = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var end1 = new DateTime(2026, 6, 1, 16, 0, 0, DateTimeKind.Utc);
        var reservation1 = new Reservation
        {
            Id = 1,
            FactoryId = 1,
            FactoryDisplayName = "Alpha Factory",
            StartTime = start1,
            EndTime = end1,
            ReservationPersonnel =
            [
                new ReservationPerson { Id = 1, PersonId = 1, PersonDisplayName = "Alice Smith", ReservationId = 1 },
                new ReservationPerson { Id = 2, PersonId = 2, PersonDisplayName = "Bob Jones", ReservationId = 1 }
            ]
        };

        // Reservation 2: Factory B, 4 hours, 1 personnel (Bob) → 4 * 1 = 4 factory hours
        var start2 = new DateTime(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc);
        var end2 = new DateTime(2026, 6, 2, 13, 0, 0, DateTimeKind.Utc);
        var reservation2 = new Reservation
        {
            Id = 2,
            FactoryId = 2,
            FactoryDisplayName = "Beta Factory",
            StartTime = start2,
            EndTime = end2,
            ReservationPersonnel =
            [
                new ReservationPerson { Id = 3, PersonId = 2, PersonDisplayName = "Bob Jones", ReservationId = 2 }
            ]
        };

        context.Reservations.AddRange(reservation1, reservation2);
        await context.SaveChangesAsync();

        return context;
    }

    // --- ByPerson ---

    [Fact]
    public async Task ByPerson_ReturnsGroupedByPersonOrderedByName()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByPerson();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonScheduleDto>>>().Subject;
        response.Success.Should().BeTrue();

        var grouped = response.Data!;
        grouped.Should().HaveCount(2);
        // Ordered by PersonName: Alice < Bob
        grouped[0].PersonName.Should().Be("Alice Smith");
        grouped[1].PersonName.Should().Be("Bob Jones");
    }

    [Fact]
    public async Task ByPerson_AliceHasOneReservationWithCorrectDuration()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByPerson();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonScheduleDto>>>().Subject;

        var alice = response.Data!.First(p => p.PersonName == "Alice Smith");
        alice.Reservations.Should().HaveCount(1);
        alice.TotalHours.Should().Be(8.0);
        alice.Reservations[0].FactoryName.Should().Be("Alpha Factory");
        alice.Reservations[0].DurationHours.Should().Be(8.0);
    }

    [Fact]
    public async Task ByPerson_BobHasTwoReservationsWithCorrectTotalHours()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByPerson();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonScheduleDto>>>().Subject;

        var bob = response.Data!.First(p => p.PersonName == "Bob Jones");
        bob.Reservations.Should().HaveCount(2);
        // 8h (reservation 1) + 4h (reservation 2) = 12h
        bob.TotalHours.Should().Be(12.0);
    }

    [Fact]
    public async Task ByPerson_ReservationsSortedByStartTime()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByPerson();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonScheduleDto>>>().Subject;

        var bob = response.Data!.First(p => p.PersonName == "Bob Jones");
        bob.Reservations[0].StartTime.Should().BeBefore(bob.Reservations[1].StartTime);
    }

    [Fact]
    public async Task ByPerson_EmptyDatabase_ReturnsEmptyList()
    {
        var context = CreateContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByPerson();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonScheduleDto>>>().Subject;
        response.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task ByPerson_NulledPersonId_StillGroupedByDisplayName()
    {
        var context = CreateContext();

        // Simulate a soft-deleted person: PersonId is null but PersonDisplayName is preserved
        var reservation = new Reservation
        {
            Id = 1, FactoryId = null, FactoryDisplayName = "Old Factory",
            StartTime = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            ReservationPersonnel =
            [
                new ReservationPerson { Id = 1, PersonId = null, PersonDisplayName = "Deleted Person", ReservationId = 1 }
            ]
        };
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var controller = new SchedulingController(context);

        var result = await controller.ByPerson();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonScheduleDto>>>().Subject;
        response.Data!.Should().HaveCount(1);
        response.Data![0].PersonName.Should().Be("Deleted Person");
        response.Data![0].PersonId.Should().BeNull();
    }

    // --- ByFactory ---

    [Fact]
    public async Task ByFactory_ReturnsGroupedByFactoryOrderedByName()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByFactory();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<FactoryScheduleDto>>>().Subject;
        response.Success.Should().BeTrue();

        var grouped = response.Data!;
        grouped.Should().HaveCount(2);
        // Ordered by FactoryName: Alpha < Beta
        grouped[0].FactoryName.Should().Be("Alpha Factory");
        grouped[1].FactoryName.Should().Be("Beta Factory");
    }

    [Fact]
    public async Task ByFactory_AlphaFactoryTotalHoursAccountsForPersonnelCount()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByFactory();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<FactoryScheduleDto>>>().Subject;

        var alpha = response.Data!.First(f => f.FactoryName == "Alpha Factory");
        // 1 reservation × 8 hours × 2 personnel = 16 total hours
        alpha.TotalHours.Should().Be(16.0);
        alpha.ReservationCount.Should().Be(1);
    }

    [Fact]
    public async Task ByFactory_BetaFactoryTotalHoursAccountsForPersonnelCount()
    {
        var context = await CreateSeededContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByFactory();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<FactoryScheduleDto>>>().Subject;

        var beta = response.Data!.First(f => f.FactoryName == "Beta Factory");
        // 1 reservation × 4 hours × 1 personnel = 4 total hours
        beta.TotalHours.Should().Be(4.0);
        beta.ReservationCount.Should().Be(1);
    }

    [Fact]
    public async Task ByFactory_EmptyDatabase_ReturnsEmptyList()
    {
        var context = CreateContext();
        var controller = new SchedulingController(context);

        var result = await controller.ByFactory();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<FactoryScheduleDto>>>().Subject;
        response.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task ByFactory_MultipleReservationsSameFactory_AggregatesCorrectly()
    {
        var context = CreateContext();

        // Two reservations for same factory, different personnel counts
        var r1 = new Reservation
        {
            Id = 1, FactoryId = 1, FactoryDisplayName = "Alpha Factory",
            StartTime = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc), // 2h
            ReservationPersonnel =
            [
                new ReservationPerson { Id = 1, PersonId = 1, PersonDisplayName = "Alice", ReservationId = 1 },
                new ReservationPerson { Id = 2, PersonId = 2, PersonDisplayName = "Bob", ReservationId = 1 }
            ]
        };
        var r2 = new Reservation
        {
            Id = 2, FactoryId = 1, FactoryDisplayName = "Alpha Factory",
            StartTime = new DateTime(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 6, 2, 12, 0, 0, DateTimeKind.Utc), // 3h
            ReservationPersonnel =
            [
                new ReservationPerson { Id = 3, PersonId = 1, PersonDisplayName = "Alice", ReservationId = 2 }
            ]
        };
        context.Reservations.AddRange(r1, r2);
        await context.SaveChangesAsync();

        var controller = new SchedulingController(context);

        var result = await controller.ByFactory();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<FactoryScheduleDto>>>().Subject;

        var alpha = response.Data!.Single();
        alpha.FactoryName.Should().Be("Alpha Factory");
        alpha.ReservationCount.Should().Be(2);
        // (2h × 2 persons) + (3h × 1 person) = 4 + 3 = 7
        alpha.TotalHours.Should().Be(7.0);
    }
}
