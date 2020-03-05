using Microsoft.EntityFrameworkCore.Migrations;

namespace dal.Migrations
{
    public partial class transactionsview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            create materialized view ""TransactionBalances"" as
select
  t.""Id"",
  (select sum(t2.""Amount"")
    from ""Transactions"" t2 
    where t2.""Date"" <= t.""Date"" and t2.""AccountId"" = t.""AccountId"" ) + a.""InitialBalance"" as ""Balance""
from ""Transactions"" t 
inner join ""Accounts"" a on a.""Id"" = t.""AccountId""
order by t.""AccountId"", t.""Date"" desc;

create index on ""TransactionBalances"" (""Id"");
");

            migrationBuilder.Sql(@"
create function on_transaction_insert() returns trigger
  security definer
  language plpgsql
as $$
  begin
    refresh materialized view ""TransactionBalances"";
    return null;
  end;
$$;

create trigger transaction_insert after insert on ""Transactions""
    for each row execute procedure on_transaction_insert();
");

            migrationBuilder.Sql(@"
create function on_transaction_delete() returns trigger
  security definer
  language plpgsql
as $$
  begin
    refresh materialized view ""TransactionBalances"";
    return null;
  end;
$$;

create trigger transaction_delete after delete on ""Transactions""
    for each row execute procedure on_transaction_delete();");

            migrationBuilder.Sql(@"
create function on_transaction_update() returns trigger
  security definer
  language plpgsql
as $$
  begin
      if old.""Amount"" != new.""Amount"" or old.""Date"" != new.""Date"" or old.""AccountId"" != new.""AccountId"" then
        refresh materialized view ""TransactionBalances"";
      end if;
      return null;
  end;
$$;

create trigger transaction_update after update on ""Transactions""
    for each row execute procedure on_transaction_update();");    

            migrationBuilder.Sql(@"
create function on_account_update() returns trigger
  security definer
  language plpgsql
as $$
  begin
      if old.""InitialBalance"" != new.""InitialBalance"" then
        refresh materialized view ""TransactionBalances"";
      end if;
      return null;
  end;
$$;

create trigger account_update after update on ""Accounts""
    for each row execute procedure on_account_update();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger transaction_insert on ""Transactions""");
            migrationBuilder.Sql(@"drop trigger transaction_delete on ""Transactions""");
            migrationBuilder.Sql(@"drop trigger transaction_update on ""Transactions""");
            migrationBuilder.Sql(@"drop trigger account_update on ""Accounts""");
            migrationBuilder.Sql(@"drop function on_transaction_insert");
            migrationBuilder.Sql(@"drop function on_transaction_delete");
            migrationBuilder.Sql(@"drop function on_transaction_update");
            migrationBuilder.Sql(@"drop function on_account_update");
            migrationBuilder.Sql(@"drop view ""TransactionBalances""");
        }
    }
}
