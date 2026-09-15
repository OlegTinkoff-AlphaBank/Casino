using Microsoft.EntityFrameworkCore;

namespace serverDB;

public class ServerDbContext : DbContext
{
    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }

    public DbSet<Donation> Donations { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameRound> GameRounds { get; set; }
    public DbSet<Promocode> Promocodes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Activs> Activs { get; set; }
    public DbSet<GameOutComes> GameOutComes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.Email).HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            e.Property(u => u.DonationAlertsCode).HasMaxLength(50);
            e.Property(u => u.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            e.Property(u => u.GameRate).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);

            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.DonationAlertsCode).IsUnique();
        });

        modelBuilder.Entity<Game>(e =>
        {
            e.Property(g => g.Name).HasMaxLength(100).IsRequired();
            e.Property(g => g.MinBet).HasColumnType("decimal(18,2)");
            e.Property(g => g.MaxBet).HasColumnType("decimal(18,2)");
            e.Property(g => g.GameRate).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(g => g.IsActive).HasDefaultValue(true);
        });
        
        modelBuilder.Entity<GameOutComes>(e =>
        {
            e.Property(go => go.Weight).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(go => go.Multiplier).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(go => go.IsActive).HasDefaultValue(true);
            e.HasOne(go => go.Game)
                .WithMany(g => g.GameOutComes)
                .HasForeignKey(go => go.GameId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Activs>(e =>
        {
            e.Property(a => a.InAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.RealInAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.OnAccsAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.OnAccsAmountLast).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.OutCanAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.OnSafeAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.AllUsersRate).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
            e.Property(a => a.AllGamesRate).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
        });
        
        modelBuilder.Entity<GameRound>(e =>
        {
            e.Property(gr => gr.BetAmount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(gr => gr.Payout).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            e.Property(gr => gr.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(gr => gr.ResultData).HasColumnType("jsonb");

            e.HasIndex(gr => gr.IdempotencyKey);

            e.HasOne(gr => gr.User)
                  .WithMany(u => u.GameRounds)
                  .HasForeignKey(gr => gr.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(gr => gr.Game)
                  .WithMany(g => g.GameRounds)
                  .HasForeignKey(gr => gr.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Type).HasMaxLength(30).IsRequired();
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)").IsRequired();

            e.HasOne(t => t.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Promocode)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(t => t.RelatedPromocodeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Donation>(e =>
        {
            e.Property(d => d.ExternalId).HasMaxLength(100).IsRequired();
            e.Property(d => d.Amount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(d => d.RawComment).HasMaxLength(500);
            e.Property(d => d.Status).HasMaxLength(20).HasDefaultValue("Unmatched");

            e.HasIndex(d => d.ExternalId).IsUnique();

            e.HasOne(d => d.User)
                  .WithMany(u => u.Donations)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Promocode>(e =>
        {
            e.Property(p => p.Code).HasMaxLength(50).IsRequired();
            e.Property(p => p.Value).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(p => p.MaxUsesPerUser).HasDefaultValue(1);
            e.Property(p => p.IsActive).HasDefaultValue(true);

            e.HasIndex(p => p.Code).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.Property(rt => rt.Token).IsRequired();
            e.Property(rt => rt.IsRevoked).HasDefaultValue(false);

            e.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}