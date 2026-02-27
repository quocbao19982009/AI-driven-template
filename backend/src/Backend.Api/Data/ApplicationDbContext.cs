using Backend.Common.Models;
using Backend.Features.Users;
using Backend.Features._FeatureTemplate;
using Backend.Features.Factories;
using Backend.Features.Personnel;
using Backend.Features.Reservations;
using Backend.Features.Todos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    // TODO: Mock remove this when we have real features
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Person> Personnel => Set<Person>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationPerson> ReservationPersonnel => Set<ReservationPerson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Factory>(entity =>
        {
            entity.HasIndex(f => f.Name).IsUnique();
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasIndex(p => p.PersonalId).IsUnique();
            entity.HasIndex(p => p.Email).IsUnique();
            entity.HasMany(p => p.AllowedFactories)
                  .WithMany()
                  .UsingEntity("PersonFactory");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasOne(r => r.Factory)
                  .WithMany()
                  .HasForeignKey(r => r.FactoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReservationPerson>(entity =>
        {
            entity.HasOne(rp => rp.Reservation)
                  .WithMany(r => r.ReservationPersonnel)
                  .HasForeignKey(rp => rp.ReservationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Person)
                  .WithMany()
                  .HasForeignKey(rp => rp.PersonId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
