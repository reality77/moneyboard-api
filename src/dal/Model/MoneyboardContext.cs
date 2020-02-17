using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace dal.Model
{
    public partial class MoneyboardContext : DbContext
    {
        IConfiguration _config;

        public MoneyboardContext()
        {
        }

        public MoneyboardContext(DbContextOptions<MoneyboardContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }

        public virtual DbSet<ImportedFile> ImportedFiles { get; set; }
        public virtual DbSet<ImportedTransaction> ImportedTransactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_config.GetConnectionString("Moneyboard"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImportedFile>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasIdentityOptions(startValue: 1);

                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.ImportDate).IsRequired();

                entity.HasIndex(e => e.FileName);
            });

            modelBuilder.Entity<ImportedTransaction>(entity =>
            {
                entity.HasIndex(e => e.FileId);

                entity.HasIndex(e => e.Hash);

                entity.HasIndex(e => e.Date);

                entity.HasOne(d => d.File)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.FileId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    
        public void SeedData()
        {
            this.Database.Migrate();
        }
    }
}
