using Microsoft.EntityFrameworkCore;
using DigitalWallet.Models;

namespace DigitalWallet
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Point> Points { get; set; }
        
    }
}
