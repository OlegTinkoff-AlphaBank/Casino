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
                DART = "def5020024f66918065eb5213933ca45e060721b886b6bb9cc6fbbf640d88bb5758e21403a24f302ad39b74f0b612deca588bbf5229409836db381afdc4f3941bedaa5577613f53ac144c79837c7c051772b71a775a0ff5f8de5ad2cb83d0ce59921bfe38c987634488dda3951f3e7922097af88bb525474bf373c367627eef21fdb3e3f5eab24cb1af5beeeb018527f81d75551696462e085f8dc8a711164439b44debfe44756f70b3b8a180c243a4b1a76e71e4ea871c058827a670e28251d0ad9e43adabaef948febbc3c6a663abfd4c397fac32a71f1d06ec62cace055ed00ef45dd817cd739874bdec472c73aa99facd9a273c80ac2fb82e2661b52b2f1f7727d0ee256c64d2048061b3d3543d23f64d38de5fb2293fdd27617eb8002c70f13fa8b66bb081256ddeea2144c0906a30c9f4eaf928d127b2f71d1be723310e004cf4039205e82a50cc64ec5024d725b98ccc7699e9163bb0146971bfb05eed9a5bb90436f829ef55538e01dcb6e6a4d8dfa083466642e05cc9dc20e223b814df6",
                DAEI = 31536000
            }
        );
    }
}