﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using dal.Model;

namespace dal.Migrations
{
    [DbContext(typeof(MoneyboardContext))]
    [Migration("20201114121051_manual_tags")]
    partial class manual_tags
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("dal.Model.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<decimal>("Balance")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric")
                        .HasDefaultValue(0m);

                    b.Property<int>("Currency")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<string>("Iban")
                        .HasColumnType("text");

                    b.Property<decimal>("InitialBalance")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric")
                        .HasDefaultValue(0m);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Iban")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("Number")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("dal.Model.ImportedFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'1', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ImportDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("FileName");

                    b.ToTable("ImportedFiles");
                });

            modelBuilder.Entity("dal.Model.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("Caption")
                        .HasColumnType("text");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("ParentTagId")
                        .HasColumnType("integer");

                    b.Property<string>("TypeKey")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ParentTagId");

                    b.HasIndex("TypeKey", "Key")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("dal.Model.TagRecognition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("RecognizedTagKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RecognizedTagTypeKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TargetTagId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TargetTagId");

                    b.ToTable("TagRecognitions");
                });

            modelBuilder.Entity("dal.Model.TagType", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<string>("Caption")
                        .HasColumnType("text");

                    b.HasKey("Key");

                    b.ToTable("TagTypes");
                });

            modelBuilder.Entity("dal.Model.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Amount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric")
                        .HasDefaultValue(0m);

                    b.Property<string>("Caption")
                        .HasColumnType("text");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<DateTime?>("UserDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("transaction_type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("Transactions");

                    b.HasDiscriminator<int>("transaction_type").HasValue(0);
                });

            modelBuilder.Entity("dal.Model.TransactionBalance", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("TransactionBalances");
                });

            modelBuilder.Entity("dal.Model.TransactionRecognitionRule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<bool>("UseOrConditions")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("TransactionRecognitionRules");
                });

            modelBuilder.Entity("dal.Model.TransactionRecognitionRuleAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("Field")
                        .HasColumnType("text");

                    b.Property<int>("TransactionRecognitionRuleId")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TransactionRecognitionRuleId");

                    b.ToTable("TransactionRecognitionRuleAction");
                });

            modelBuilder.Entity("dal.Model.TransactionRecognitionRuleCondition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:IdentitySequenceOptions", "'', '1', '', '', 'False', '1'")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("FieldName")
                        .HasColumnType("text");

                    b.Property<int>("FieldType")
                        .HasColumnType("integer");

                    b.Property<int>("TransactionRecognitionRuleId")
                        .HasColumnType("integer");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.Property<int>("ValueOperator")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TransactionRecognitionRuleId");

                    b.ToTable("TransactionRecognitionRuleCondition");
                });

            modelBuilder.Entity("dal.Model.TransactionTag", b =>
                {
                    b.Property<int>("TransactionId")
                        .HasColumnType("integer");

                    b.Property<int>("TagId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsManual")
                        .HasColumnType("boolean");

                    b.HasKey("TransactionId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("TransactionTag");
                });

            modelBuilder.Entity("dal.Model.ImportedTransaction", b =>
                {
                    b.HasBaseType("dal.Model.Transaction");

                    b.Property<string>("ImportCaption")
                        .HasColumnType("text");

                    b.Property<string>("ImportComment")
                        .HasColumnType("text");

                    b.Property<int>("ImportFileId")
                        .HasColumnType("integer");

                    b.Property<string>("ImportHash")
                        .HasColumnType("text");

                    b.Property<string>("ImportNumber")
                        .HasColumnType("text");

                    b.HasIndex("ImportFileId");

                    b.HasIndex("ImportHash");

                    b.HasDiscriminator().HasValue(1);
                });

            modelBuilder.Entity("dal.Model.Tag", b =>
                {
                    b.HasOne("dal.Model.Tag", "ParentTag")
                        .WithMany("SubTags")
                        .HasForeignKey("ParentTagId");

                    b.HasOne("dal.Model.TagType", "Type")
                        .WithMany("Tags")
                        .HasForeignKey("TypeKey");
                });

            modelBuilder.Entity("dal.Model.TagRecognition", b =>
                {
                    b.HasOne("dal.Model.Tag", "TargetTag")
                        .WithMany()
                        .HasForeignKey("TargetTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("dal.Model.Transaction", b =>
                {
                    b.HasOne("dal.Model.Account", "Account")
                        .WithMany("Transactions")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("dal.Model.TransactionBalance", b =>
                {
                    b.HasOne("dal.Model.Transaction", null)
                        .WithOne("BalanceData")
                        .HasForeignKey("dal.Model.TransactionBalance", "Id");
                });

            modelBuilder.Entity("dal.Model.TransactionRecognitionRuleAction", b =>
                {
                    b.HasOne("dal.Model.TransactionRecognitionRule", "Rule")
                        .WithMany("Actions")
                        .HasForeignKey("TransactionRecognitionRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("dal.Model.TransactionRecognitionRuleCondition", b =>
                {
                    b.HasOne("dal.Model.TransactionRecognitionRule", "Rule")
                        .WithMany("Conditions")
                        .HasForeignKey("TransactionRecognitionRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("dal.Model.TransactionTag", b =>
                {
                    b.HasOne("dal.Model.Tag", "Tag")
                        .WithMany("TransactionTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dal.Model.Transaction", "Transaction")
                        .WithMany("TransactionTags")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("dal.Model.ImportedTransaction", b =>
                {
                    b.HasOne("dal.Model.ImportedFile", "ImportFile")
                        .WithMany("Transactions")
                        .HasForeignKey("ImportFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
