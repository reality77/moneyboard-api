using Microsoft.EntityFrameworkCore.Migrations;

namespace dal.Migrations
{
    public partial class transaction_balance_to_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger transaction_insert on ""Transactions""");
            migrationBuilder.Sql(@"drop trigger transaction_delete on ""Transactions""");
            migrationBuilder.Sql(@"drop trigger transaction_update on ""Transactions""");
            migrationBuilder.Sql(@"drop trigger account_update on ""Accounts""");
            migrationBuilder.Sql(@"drop function on_transaction_insert");
            migrationBuilder.Sql(@"drop function on_transaction_delete");
            migrationBuilder.Sql(@"drop function on_transaction_update");
            migrationBuilder.Sql(@"drop function on_account_update");
            migrationBuilder.Sql(@"drop materialized view ""TransactionBalances""");

            migrationBuilder.CreateTable(
                name: "TransactionBalances",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Balance = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionBalances_Transactions_Id",
                        column: x => x.Id,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"create or replace procedure update_balances(account_id int, refresh_date timestamp without time zone)
  language plpgsql
as $$
  begin
	
	if account_id is null then
		RAISE EXCEPTION 'Account id must be set'
      	USING ERRCODE = 10001; 
	end if;
	
	delete from ""TransactionBalances"" b using ""Transactions"" t
	where b.""Id"" = t.""Id"" 
	and t.""AccountId"" = account_id
	and (refresh_date is null or t.""Date"" >= refresh_date);

	insert into ""TransactionBalances""
	select
	  t.""Id"",
	  (select sum(t2.""Amount"")
		from ""Transactions"" t2 
		where t2.""Date"" <= t.""Date"" and t2.""AccountId"" = t.""AccountId"" ) + a.""InitialBalance"" as ""Balance""
	from ""Transactions"" t 
	inner join ""Accounts"" a on a.""Id"" = t.""AccountId""
	where t.""AccountId"" = account_id
	and (refresh_date is null or t.""Date"" >= refresh_date)
	order by t.""AccountId"", t.""Date"" desc;

  end;
$$;
");

            migrationBuilder.Sql(@"create or replace procedure refresh_all_balances()
  language plpgsql
as $$
  begin

	delete from ""TransactionBalances"";

	insert into ""TransactionBalances""
	select
	  t.""Id"",
	  (select sum(t2.""Amount"")
		from ""Transactions"" t2 
		where t2.""Date"" <= t.""Date"" and t2.""AccountId"" = t.""AccountId"" ) + a.""InitialBalance"" as ""Balance""
	from ""Transactions"" t 
	inner join ""Accounts"" a on a.""Id"" = t.""AccountId""
	order by t.""AccountId"", t.""Date"" desc;

  end;
$$;
");

            migrationBuilder.Sql(@"
create function on_transaction_insert() returns trigger
  security definer
  language plpgsql
as $$
  begin
    call update_balances(new.""AccountId"", new.""Date"");
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
    call update_balances(old.""AccountId"", old.""Date"");
    return null;
  end;
$$;

create trigger transaction_delete after delete on ""Transactions""
    for each row execute procedure on_transaction_delete();");

            migrationBuilder.Sql(@"
create or replace function on_transaction_update() returns trigger
  security definer
  language plpgsql
as $$
  declare dt timestamp without time zone;
  begin

      if old.""AccountId"" != new.""AccountId"" then
        call update_balances(old.""AccountId"", old.""Date"");
        call update_balances(new.""AccountId"", new.""Date"");
      else 
	  	if old.""Amount"" != new.""Amount"" or old.""Date"" != new.""Date"" then
			if old.""Date"" > new.""Date"" then
				dt = new.""Date"";
			else
				dt = old.""Date"";
			end if;

			call update_balances(old.""AccountId"", dt);
		end if;
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
        call update_balances(old.""Id"", NULL);
      end if;
      return null;
  end;
$$;

create trigger account_update after update on ""Accounts""
    for each row execute procedure on_account_update();");

            migrationBuilder.Sql(@"create or replace procedure disable_balance_computation()
  language plpgsql
as $$
  begin
	ALTER TABLE ""Accounts"" DISABLE TRIGGER account_update;
	ALTER TABLE ""Transactions"" DISABLE TRIGGER transaction_insert;
	ALTER TABLE ""Transactions"" DISABLE TRIGGER transaction_update;
	ALTER TABLE ""Transactions"" DISABLE TRIGGER transaction_delete;
  end;
$$;

create or replace procedure reenable_balance_computation()
  language plpgsql
as $$
  begin
	ALTER TABLE ""Accounts"" ENABLE TRIGGER account_update;
	ALTER TABLE ""Transactions"" ENABLE TRIGGER transaction_insert;
	ALTER TABLE ""Transactions"" ENABLE TRIGGER transaction_update;
	ALTER TABLE ""Transactions"" ENABLE TRIGGER transaction_delete;
	
	call refresh_all_balances();
  end;
$$;");

            migrationBuilder.Sql("call refresh_all_balances();");
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
            migrationBuilder.Sql(@"drop procedure update_balances");
            migrationBuilder.Sql(@"drop procedure refresh_all_balances");
            migrationBuilder.Sql(@"drop procedure disable_balance_computation");
            migrationBuilder.Sql(@"drop procedure reenable_balance_computation");
            migrationBuilder.DropTable(
                name: "TransactionBalances");

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
    }
}
