using Microsoft.EntityFrameworkCore;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionDetails> TransactionDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionDetails>()
            .HasOne<Transaction>()  // Navigation property olmadan ilişki tanımı
            .WithMany()
            .HasForeignKey(td => td.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }


}
