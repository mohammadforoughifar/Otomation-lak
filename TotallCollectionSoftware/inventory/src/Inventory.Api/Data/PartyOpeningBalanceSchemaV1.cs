using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data;

/// <summary>
/// خودتعمیرِ اسکیمای «افتتاح حساب طرف حساب‌ها (بدهکار/بستانکار اول دوره)» — نسخه ۱.
/// <para>
/// ستون‌های <c>OpeningDebit</c>، <c>OpeningCredit</c>، <c>OpeningDate</c> و <c>OpeningDescription</c>
/// را به‌صورت idempotent به جدول <c>Parties</c> در هر دو موتور دیتابیس (SQLite و SQL Server) اضافه می‌کند.
/// </para>
/// </summary>
public static class PartyOpeningBalanceSchemaV1
{
    public static async Task EnsureAsync(AppDbContext db)
    {
        if (db.Database.IsSqlite())
        {
            if (!await HasColumnAsync(db, "Parties", "OpeningDebit"))
                await SafeAsync(db, "ALTER TABLE Parties ADD COLUMN OpeningDebit TEXT NOT NULL DEFAULT '0';");
            if (!await HasColumnAsync(db, "Parties", "OpeningCredit"))
                await SafeAsync(db, "ALTER TABLE Parties ADD COLUMN OpeningCredit TEXT NOT NULL DEFAULT '0';");
            if (!await HasColumnAsync(db, "Parties", "OpeningDate"))
                await SafeAsync(db, "ALTER TABLE Parties ADD COLUMN OpeningDate TEXT NULL;");
            if (!await HasColumnAsync(db, "Parties", "OpeningDescription"))
                await SafeAsync(db, "ALTER TABLE Parties ADD COLUMN OpeningDescription TEXT NULL;");
            return;
        }

        await SafeAsync(db, @"
IF OBJECT_ID(N'dbo.Parties', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Parties','OpeningDebit') IS NULL
        ALTER TABLE [dbo].[Parties] ADD [OpeningDebit] decimal(18,2) NOT NULL CONSTRAINT [DF_Parties_OpeningDebit] DEFAULT(0);
    IF COL_LENGTH('dbo.Parties','OpeningCredit') IS NULL
        ALTER TABLE [dbo].[Parties] ADD [OpeningCredit] decimal(18,2) NOT NULL CONSTRAINT [DF_Parties_OpeningCredit] DEFAULT(0);
    IF COL_LENGTH('dbo.Parties','OpeningDate') IS NULL
        ALTER TABLE [dbo].[Parties] ADD [OpeningDate] datetime2 NULL;
    IF COL_LENGTH('dbo.Parties','OpeningDescription') IS NULL
        ALTER TABLE [dbo].[Parties] ADD [OpeningDescription] nvarchar(400) NULL;
END");
    }

    private static async Task<bool> HasColumnAsync(AppDbContext db, string table, string column)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='{column}';";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }
        catch
        {
            return true;
        }
    }

    private static async Task SafeAsync(AppDbContext db, string sql)
    {
        try { await db.Database.ExecuteSqlRawAsync(sql); }
        catch (Exception ex) { Console.WriteLine($"[DB] PartyOpeningBalance: {ex.Message}"); }
    }
}
