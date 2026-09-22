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
            e.Property(a => a.DART).IsRequired().HasDefaultValue("");
            e.Property(a => a.DAEI).IsRequired().HasDefaultValue(0);
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
            e.Property(d => d.Status).IsRequired().HasDefaultValue(false);

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
        
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "testAdmin",
                Email = "testAdmin@test.com",
                PasswordHash = "testPasswordHash",
                Balance = 10000m,
                Role = "Admin",
                DonationAlertsCode = "testAdminCode",
                GameRate = 0.5m,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            new User
            {
                Id = 2,
                Username = "testUser",
                Email = "testUser@test.com",
                PasswordHash = "testPasswordHash",
                Balance = 1000m,
                Role = "User",
                DonationAlertsCode = "testUserCode",
                GameRate = 0.5m,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
        
        modelBuilder.Entity<Activs>().HasData(
            new Activs
            {
                id = 1,
                InAmount = 0m,
                RealInAmount = 0m,
                OnAccsAmount = 0m,
                OnAccsAmountLast = 0m,
                OutCanAmount = 0m,
                OnSafeAmount = 0m,
                AllUsersRate = 1.5m,
                AllGamesRate = 1.0m,
                DART = "def50200be5fe74067a9e7d712bb044a3bfa9badc245b67d9b9ff8a58df3228e2c53eea94ce3e987a6ed82cb2a5172f1bc38391e887f9f9d01f043b89dde7fb8d7db6a7459825850dd559d9c60fd4b33f9a632b4a76ac8183e0f8f941057c02758aff1783c9645e82d4e0ab6b3748b6984ee5f8a982a04d990d130f81abfe7ef36e275d637b6d18abdd210ce80969722620c96de99c5470fe4e4e0e5b808a1eee49e588f86ab65a556c4e19794c2c14dee07cf8aedc99149634174182a67a639ca974abdd381f3ccf122a45be5b7dc49de6be2a7e8f379b769f5de12e9a1f716f893780872e6c613a542dd7288bd82a6b6348ecaa389371512d99113a6ced557467e41f8f2eca6b586d70652b2a44ce46f044e131ee0681d0a77a44bde27da97e28c37637b5c98a7b84a3b7468ec0bfe0a79744d59a0f2df0906d7fbfcd359e01482c5ef82c74178150f2eb970f58dbbd3074ac4baf9db5f21efd43ad6c170996e42c586f1f60c0c61d4181e2d5b700f302386f059df941fda561c898915e62cd111",
                DAEI = 31536000
            }
        );
    }
}