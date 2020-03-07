using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using dto;

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
        public virtual DbSet<TransactionRecognitionRule> TransactionRecognitionRules { get; set; }
        public virtual DbSet<TagRecognition> TagRecognitions { get; set; }
        public virtual DbSet<TransactionBalance> TransactionBalances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_config.GetConnectionString("Moneyboard"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Utilisation de l'ancien fonctionnement UseSerialColumns plutôt que UseIdentityColumns car erreur de syntaxe à la création des tables
            modelBuilder.UseSerialColumns();

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id).HasIdentityOptions();

                entity.Property(e => e.Name)
                    .IsRequired();

                entity.Property(e => e.Number);

                entity.Property(e => e.InitialBalance)
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.Balance)
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasDefaultValue(ECurrency.EUR)
                    .IsRequired();
                
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Number).IsUnique();
                entity.HasIndex(e => e.Iban).IsUnique();
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
                    .HasForeignKey(d => d.TypeKey);

                entity.HasOne(d => d.ParentTag)
                    .WithMany(p => p.SubTags)
                    .IsRequired(false)
                    .HasForeignKey(d => d.ParentTagId);

                entity.HasIndex(e => new { e.TypeKey, e.Key } )
                    .IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasIdentityOptions();

                entity.HasDiscriminator<int>("transaction_type")
                    .HasValue<Transaction>(0)
                    .HasValue<ImportedTransaction>(1);

                entity.Property(e => e.Amount)
                    .HasDefaultValue(0m)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasDefaultValue(ETransactionType.Unknown)
                    .IsRequired();

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

            modelBuilder.Entity<TransactionRecognitionRule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasIdentityOptions();
            });

            modelBuilder.Entity<TransactionRecognitionRuleCondition>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasIdentityOptions();

                entity.HasOne(d => d.Rule)
                    .WithMany(p => p.Conditions)
                    .HasForeignKey(d => d.TransactionRecognitionRuleId);
            });

            modelBuilder.Entity<TagRecognition>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RecognizedTagTypeKey).IsRequired();
                entity.Property(e => e.RecognizedTagKey).IsRequired();

                entity.Property(e => e.TargetTagId).IsRequired();

                entity.HasOne(d => d.TargetTag)
                    .WithMany()
                    .HasForeignKey(d => d.TargetTagId);
            });            

            modelBuilder.Entity<TransactionBalance>(entity => {

                entity.HasKey(e => e.Id);
                
                entity.ToView("TransactionBalances");

                entity.HasOne<Transaction>()
                    .WithOne(p => p.BalanceData)
                    .HasForeignKey<TransactionBalance>(t => t.Id)
                    .IsRequired(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    
        public void SeedData()
        {
            this.Database.Migrate();

            if(!this.Tags.Any())
            {
                // Premiere initialisation
                this.TagTypes.Add(new TagType { Key = "payee", Caption = "Tiers" });
                this.TagTypes.Add(new TagType { Key = "mode", Caption = "Mode" });
                this.TagTypes.Add(new TagType { Key = "category", Caption = "Catégorie" });
                this.TagTypes.Add(new TagType { Key = "context", Caption = "Contexte" });

                this.SaveChanges();

                this.Tags.AddRange(new List<Tag>
                {
                    new Tag { TypeKey = "category", Key = "alimentation", Caption = "Alimentation",  },
                    new Tag { TypeKey = "category", Key = "factures", Caption = "Factures",  },

                    new Tag { TypeKey = "context", Key = "personnel", Caption = "Personnel",  },
                    new Tag { TypeKey = "context", Key = "famille", Caption = "Famille",  },
                    new Tag { TypeKey = "context", Key = "travail", Caption = "Travail",  },
                });

                this.SaveChanges();

                this.Tags.Add(new Tag { TypeKey = "category", Key = "restaurant", Caption = "Restaurant", ParentTag = this.Tags.SingleOrDefault(t => t.TypeKey == "category" && t.Key == "alimentation") });

                this.SaveChanges();

                this.Tags.Add(new Tag { TypeKey = "category", Key = "mobile", Caption = "Téléphone mobile", ParentTag = this.Tags.SingleOrDefault(t => t.TypeKey == "category" && t.Key == "factures") });
                
                this.SaveChanges();
                
                this.Tags.Add(new Tag { TypeKey = "category", Key = "mobile_perso", Caption = "Mobile personnel", ParentTag = this.Tags.SingleOrDefault(t => t.TypeKey == "category" && t.Key == "mobile") });

                this.SaveChanges();
            }

            // --- DEBUG
            if(!this.Accounts.Any())
            {
                this.Accounts.Add(new Account
                {
                    Id = 1,
                    Name = "Demo",
                });

                // Rule 1 - tirufle
                var rule = new TransactionRecognitionRule { UseOrConditions = false };

                rule.Conditions.Add(new TransactionRecognitionRuleCondition 
                { 
                    FieldType = ERecognitionRuleConditionFieldType.Tag,
                    FieldName = "payee",
                    Value = "tirufle"
                });

                rule.Actions.Add(new TransactionRecognitionRuleAction
                { 
                    Type = ERecognitionRuleActionType.AddTag,
                    Field = "category",
                    Value = "restaurant"
                });

                this.TransactionRecognitionRules.Add(rule);

                // Rule 2 - abonnement mobile perso
                rule = new TransactionRecognitionRule { UseOrConditions = false };

                rule.Conditions.Add(new TransactionRecognitionRuleCondition 
                { 
                    FieldType = ERecognitionRuleConditionFieldType.Tag,
                    FieldName = "payee",
                    Value = "TELE9"
                });

                rule.Conditions.Add(new TransactionRecognitionRuleCondition 
                { 
                    FieldType = ERecognitionRuleConditionFieldType.DataField,
                    FieldName = "ImportComment",
                    ValueOperator = ERecognitionRuleConditionOperator.Contains,
                    Value = "abonnement : 56504616"
                });

                rule.Actions.Add(new TransactionRecognitionRuleAction
                { 
                    Type = ERecognitionRuleActionType.AddTag,
                    Field = "category",
                    Value = "mobile_perso"
                });

                rule.Actions.Add(new TransactionRecognitionRuleAction
                { 
                    Type = ERecognitionRuleActionType.AddTag,
                    Field = "context",
                    Value = "personnel"
                });

                this.TransactionRecognitionRules.Add(rule);

                // Rule 3 - abonnement mobile autre
                rule = new TransactionRecognitionRule { UseOrConditions = false };

                rule.Conditions.Add(new TransactionRecognitionRuleCondition 
                { 
                    FieldType = ERecognitionRuleConditionFieldType.Tag,
                    FieldName = "payee",
                    Value = "TELE9"
                });

                rule.Conditions.Add(new TransactionRecognitionRuleCondition 
                { 
                    FieldType = ERecognitionRuleConditionFieldType.DataField,
                    FieldName = "ImportComment",
                    ValueOperator = ERecognitionRuleConditionOperator.Contains,
                    Value = "abonnement"
                });

                rule.Actions.Add(new TransactionRecognitionRuleAction
                { 
                    Type = ERecognitionRuleActionType.AddTag,
                    Field = "category",
                    Value = "mobile"
                });

                this.TransactionRecognitionRules.Add(rule);
            }
            // --- FIN DEBUG


            this.SaveChanges();
        }
    }
}
