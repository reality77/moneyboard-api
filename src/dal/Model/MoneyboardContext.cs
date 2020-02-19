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

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<ImportedFile> ImportedFiles { get; set; }
        public virtual DbSet<ImportedTransaction> ImportedTransactions { get; set; }
        public virtual DbSet<TagType> TagTypes { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_config.GetConnectionString("Moneyboard"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasIdentityOptions(startValue: 1);
            });

            modelBuilder.Entity<TagType>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).IsRequired();
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired();

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Tags)
                    .HasForeignKey(d => d.TagTypeKey);

                entity.HasIndex(e => new { e.TagTypeKey, e.Key } )
                    .IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasIdentityOptions(startValue: 1);

                entity.HasDiscriminator<int>("transaction_type")
                    .HasValue<Transaction>(0)
                    .HasValue<ImportedTransaction>(1);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.AccountId);
            });
            
            modelBuilder.Entity<TransactionTag>(entity =>
            {
                entity.HasKey(e => new { e.TransactionId, e.TagId } );

                entity.Property(e => e.TransactionId).IsRequired();
                entity.Property(e => e.TagId).IsRequired();

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.TransactionTags)
                    .HasForeignKey(d => d.TagId);

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.TransactionTags)
                    .HasForeignKey(d => d.TransactionId);
            });

            modelBuilder.Entity<ImportedFile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasIdentityOptions(startValue: 1);

                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.ImportDate).IsRequired();

                entity.HasIndex(e => e.FileName);
            });

            modelBuilder.Entity<ImportedTransaction>(entity =>
            {
                entity.HasBaseType<Transaction>();

                entity.HasIndex(e => e.ImportFileId);

                entity.HasIndex(e => e.ImportHash);

                entity.HasOne(d => d.ImportFile)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.ImportFileId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    
        public void SeedData()
        {
            //this.Database.Migrate();

            this.TagTypes.Add(new TagType { Key = "payee", Caption = "Tiers" });
            this.TagTypes.Add(new TagType { Key = "mode", Caption = "Mode" });
            this.TagTypes.Add(new TagType { Key = "category", Caption = "Catégorie" });

            // --- DEBUG
            this.Accounts.Add(new Account
            {
                Id = 1,
                Name = "Demo",
            });

            this.SaveChanges();
        }
    }
}
