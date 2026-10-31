using Inventory.Shared;
using System.Text.Json.Serialization;

namespace Inventory.Shared.Dtos;

/// <summary>سطر سند (خرید/فروش)</summary>
public class OrderLine
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool DiscountIsAmount { get; set; }
    public decimal Total => Math.Max(0, (Quantity * Price) - DiscountAmount);

    /// <summary>خدمات است؟</summary>
    public bool IsService { get; set; }

    /// <summary>بهای تمام‌شدهٔ واحد؛ از snapshot ثبت‌شده یا روش قیمت‌گذاری انبار، پرشده توسط سرور.</summary>
    public decimal? UnitCost { get; set; }

    /// <summary>سود سطر فروش = (قیمت فروش − بهای تمام‌شده) × مقدار — برای کالا و خدمات دارای هزینهٔ ثبت‌شده.</summary>
    public decimal? Profit { get; set; }

    /// <summary>پورسانت معرف این سطر (پر شده توسط سرور — فقط فروش با معرف)</summary>
    public decimal? Commission { get; set; }
}

/// <summary>سند خرید/فروش</summary>
public class Order
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int PartyId { get; set; }
    public string? PartyName { get; set; }
    public TransactionType Type { get; set; }
    public string Number { get; set; } = "";
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public List<OrderLine> Lines { get; set; } = new();
    public decimal TotalAmount { get; set; }

    /// <summary>معرف (بازاریاب) سند فروش</summary>
    public int? ReferrerId { get; set; }
    public string? ReferrerName { get; set; }

    /// <summary>مجموع سود سند (فقط فروش — پر شده توسط سرور)</summary>
    public decimal? TotalProfit { get; set; }

    /// <summary>مبلغ پورسانت معرف (پر شده توسط سرور)</summary>
    public decimal? CommissionAmount { get; set; }

    /// <summary>سود خالص = سود کل − پورسانت معرف (پر شده توسط سرور)</summary>
    public decimal? NetProfit { get; set; }

    // ---------- پرداخت (فقط فروش) ----------
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public CashType? CashType { get; set; }

    /// <summary>پیش‌دریافت نقدی در پرداخت ترکیبی</summary>
    public decimal CashAmount { get; set; }

    /// <summary>شناسه بانک / حساب خزانه انتخابی برای تسویه</summary>
    public int? TrsAccountId { get; set; }
    public string? TrsAccountName { get; set; }

    public DateTime? DueDate { get; set; }
    public decimal SettledAmount { get; set; }
    public List<ChequeDto> Cheques { get; set; } = new();
    public List<InstallmentDto> Installments { get; set; } = new();

    public bool IsPosted { get; set; } = true;
    public bool CanDelete { get; set; } = true;

    public bool IsPurchase => Type == TransactionType.Purchase;
    public bool IsSale => Type == TransactionType.Sale;
}

/// <summary>درخواست ثبت/ویرایش سند</summary>
public class OrderCommand
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public int PartyId { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }

    /// <summary>معرف (بازاریاب) — اختیاری، فقط فروش</summary>
    public int? ReferrerId { get; set; }

    // ---------- پرداخت (فقط فروش) ----------
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public CashType? CashType { get; set; }

    /// <summary>پیش‌دریافت نقدی در پرداخت ترکیبی (نسیه/چک/اقساط)</summary>
    public decimal CashAmount { get; set; }

    /// <summary>شناسه بانک / حساب خزانه انتخابی برای تسویه</summary>
    public int? TrsAccountId { get; set; }

    public DateTime? DueDate { get; set; }
    public List<ChequeDto> Cheques { get; set; } = new();
    public List<InstallmentDto> Installments { get; set; } = new();

    public List<OrderLineInput> Lines { get; set; } = new();
}

/// <summary>چک دریافتی.</summary>
public class ChequeDto
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public int? TrsAccountId { get; set; }
    public string? BankName { get; set; }
    public string? AccountInfo { get; set; }
    public string? OwnerName { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCleared { get; set; }
    public DateTime? ClearedAt { get; set; }
    public ChequeStatus Status { get; set; } = ChequeStatus.InHand;
    public DateTime? BouncedAt { get; set; }
    public string? BounceReason { get; set; }
    public string? Note { get; set; }
}

/// <summary>قسط دفترچه اقساط.</summary>
public class InstallmentDto
{
    public int Id { get; set; }
    public int No { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class OrderLineInput
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool DiscountIsAmount { get; set; }

    /// <summary>هزینهٔ واحدِ ارسالی فقط از سرویس داخلی تعمیرات؛ از درخواست‌های عمومی JSON پذیرفته نمی‌شود.</summary>
    [JsonIgnore]
    public decimal? UnitCostSnapshot { get; set; }
}

/// <summary>درخواست اصلاح موجودی</summary>
public class AdjustmentCommand
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

/// <summary>موجودی فعلی یک کالا در یک انبار</summary>
public class StockItem
{
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal AvgCost { get; set; }
    public decimal LastSalePrice { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal MaxStock { get; set; }
    public string? Category { get; set; }
    public bool BelowReorder => ReorderPoint > 0 && Quantity <= ReorderPoint;
}

/// <summary>یک سطر کاردکس کالا</summary>
public class KardexRow
{
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string DocumentNo { get; set; } = "";
    public string PartyName { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal InQty { get; set; }
    public decimal OutQty { get; set; }
    public decimal Balance { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>آیتم گزارش نقطه سفارش</summary>
public class ReorderItem
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "";
    public string? Category { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal MaxStock { get; set; }
    public decimal TotalStock { get; set; }

    /// <summary>مقدار پیشنهادی برای خرید (تا رسیدن به حداکثر)</summary>
    public decimal Suggested => (MaxStock > TotalStock ? MaxStock : ReorderPoint * 2) - TotalStock;

    public decimal Shortage => ReorderPoint - TotalStock;
}

/// <summary>خلاصه داشبورد</summary>
public class DashboardSummary
{
    public int ProductCount { get; set; }
    public int WarehouseCount { get; set; }
    public int CustomerCount { get; set; }
    public int SupplierCount { get; set; }
    public int BelowReorderCount { get; set; }
    public decimal InventoryValue { get; set; }
    public decimal TodaySales { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal MonthSales { get; set; }
    public decimal MonthPurchases { get; set; }
    public int LowStockThreshold { get; set; }
}

/// <summary>داشبورد مدیریتی: فروش/سود دوره‌ای + بدهکاران سررسیدشده + چک‌های امروز (فقط ادمین).</summary>
public class AdminDashboard
{
    // فروش دوره‌ای (شمسی)
    public decimal SalesToday { get; set; }
    public decimal SalesWeek { get; set; }
    public decimal SalesMonth { get; set; }
    public decimal SalesQuarter { get; set; }

    // سود دوره‌ای (شمسی)
    public decimal ProfitToday { get; set; }
    public decimal ProfitWeek { get; set; }
    public decimal ProfitMonth { get; set; }
    public decimal ProfitQuarter { get; set; }

    /// <summary>بدهکاران دارای سررسید گذشته/امروز</summary>
    public List<DebtorItem> OverdueDebtors { get; set; } = new();

    /// <summary>چک‌هایی که امروز باید پاس شوند</summary>
    public List<ChequeAlertItem> TodayCheques { get; set; } = new();

    /// <summary>چک‌های پرداختی ما (صادره به تأمین‌کننده) که سررسید شده یا امروز سررسید دارند</summary>
    public List<ChequeAlertItem> IssuedCheques { get; set; } = new();

    /// <summary>روند فروش/سود ۳۰ روز اخیر (برای نمودار)</summary>
    public List<TrendPoint> DailyTrend { get; set; } = new();

    /// <summary>فروش/سود ۶ ماه شمسی اخیر (برای نمودار میله‌ای)</summary>
    public List<TrendPoint> MonthlyTrend { get; set; } = new();

    /// <summary>تفکیک فروش بر اساس روش پرداخت — فصل جاری (برای نمودار دونات)</summary>
    public decimal PayCash { get; set; }
    public decimal PayCredit { get; set; }
    public decimal PayCheque { get; set; }
    public decimal PayInstallment { get; set; }
}

/// <summary>نقطه نمودار روند.</summary>
public class TrendPoint
{
    public string Label { get; set; } = "";
    public decimal Sales { get; set; }
    public decimal Profit { get; set; }
}

/// <summary>بدهکار سررسیدشده.</summary>
public class DebtorItem
{
    public int PartyId { get; set; }
    public string PartyName { get; set; } = "";
    public string? Mobile { get; set; }
    public string Number { get; set; } = "";
    public int TransactionId { get; set; }

    /// <summary>نسیه یا قسط</summary>
    public string Kind { get; set; } = "";

    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }

    /// <summary>چند روز از سررسید گذشته (۰ = امروز)</summary>
    public int DaysOverdue { get; set; }
}

/// <summary>چک قابل وصول امروز.</summary>
public class ChequeAlertItem
{
    public int ChequeId { get; set; }
    public string Number { get; set; } = "";
    public string? BankName { get; set; }
    public string? OwnerName { get; set; }
    public string PartyName { get; set; } = "";
    public string OrderNumber { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public ChequeStatus Status { get; set; } = ChequeStatus.InHand;
    public string? BounceReason { get; set; }
}

/// <summary>آیتم فعالیت اخیر برای داشبورد</summary>
public class RecentActivity
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = "";
    public string Number { get; set; } = "";
    public string PartyName { get; set; } = "";
    public decimal Amount { get; set; }
}


/// <summary>کالای موجود — نمای محدود برای پنل معرف.</summary>
public class ReferrerProductItem
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Unit { get; set; } = "";
    public string? Category { get; set; }
    public bool InStock { get; set; }
}

/// <summary>دسته هزینه.</summary>
public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    /// <summary>تعداد اسناد این دسته (پر شده توسط سرور)</summary>
    public int ExpenseCount { get; set; }

    /// <summary>جمع مبلغ اسناد این دسته (پر شده توسط سرور)</summary>
    public decimal TotalAmount { get; set; }
}

/// <summary>سند هزینه.</summary>
public class ExpenseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    /// <summary>نحوه پرداخت (نقد/کارت‌خوان/کارت به کارت)</summary>
    public CashType PayType { get; set; } = CashType.Cash;

    /// <summary>بانک / حساب خزانهٔ انتخابی برای نحوهٔ پرداخت</summary>
    public int? TrsAccountId { get; set; }
    public string? TrsAccountName { get; set; }

    /// <summary>دریافت‌کننده / طرف هزینه</summary>
    public string? Payee { get; set; }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>مدل مدیریت چک در سامانه فروشگاهی (دریافتی و پرداختی)</summary>
public class ShopChequeItem
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string OrderNumber { get; set; } = "";
    public TransactionType TransactionType { get; set; }
    public int PartyId { get; set; }
    public string PartyName { get; set; } = "";
    public string? PartyMobile { get; set; }
    public string Number { get; set; } = "";
    public string? BankName { get; set; }
    public string? AccountInfo { get; set; }
    public string? OwnerName { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCleared { get; set; }
    public DateTime? ClearedAt { get; set; }
    public ChequeStatus Status { get; set; } = ChequeStatus.InHand;
    public DateTime? BouncedAt { get; set; }
    public string? BounceReason { get; set; }
    public string? Note { get; set; }
    public int DaysOverdue { get; set; }
}

/// <summary>مدل مدیریت اقساط در سامانه فروشگاهی</summary>
public class ShopInstallmentItem
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string OrderNumber { get; set; } = "";
    public int PartyId { get; set; }
    public string PartyName { get; set; } = "";
    public string? PartyMobile { get; set; }
    public int No { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public int DaysOverdue { get; set; }
}

/// <summary>آمار و خلاصه وضعیت چک‌ها و اقساط فروشگاهی</summary>
public class ShopCreditSummary
{
    public int InHandChequesCount { get; set; }
    public decimal InHandChequesTotal { get; set; }
    public int ClearedChequesCount { get; set; }
    public decimal ClearedChequesTotal { get; set; }
    public int BouncedChequesCount { get; set; }
    public decimal BouncedChequesTotal { get; set; }
    public int PendingInstallmentsCount { get; set; }
    public decimal PendingInstallmentsTotal { get; set; }
    public int PaidInstallmentsCount { get; set; }
    public decimal PaidInstallmentsTotal { get; set; }
    public int OverdueInstallmentsCount { get; set; }
    public decimal OverdueInstallmentsTotal { get; set; }
}

/// <summary>فرمان ثبت یا ویرایش افتتاح حساب (مانده اول دوره بدهکار/بستانکار) برای طرف حساب</summary>
public class PartyOpeningBalanceCommand
{
    public int PartyId { get; set; }

    /// <summary>بدهکار اول دوره (طلب ما از طرف حساب — ریال)</summary>
    public decimal OpeningDebit { get; set; }

    /// <summary>بستانکار اول دوره (بدهی ما به طرف حساب — ریال)</summary>
    public decimal OpeningCredit { get; set; }

    /// <summary>تاریخ افتتاح حساب</summary>
    public DateTime? OpeningDate { get; set; }

    /// <summary>شرح افتتاح حساب</summary>
    public string? OpeningDescription { get; set; }
}

/// <summary>یک ردیف از گزارش حساب و کتاب طرف حساب‌ها (بدهکار و بستانکار)</summary>
public class ShopPartyAccountRow
{
    public int PartyId { get; set; }
    public PartyType PartyType { get; set; }
    public string PartyName { get; set; } = "";
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public DateTime? OpeningDate { get; set; }
    public string? OpeningDescription { get; set; }

    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }

    public decimal TotalDebit => OpeningDebit + PeriodDebit;
    public decimal TotalCredit => OpeningCredit + PeriodCredit;

    /// <summary>مانده نهایی (مثبت = بدهکار به ما، منفی = بستانکار از ما، صفر = تسویه)</summary>
    public decimal Balance => TotalDebit - TotalCredit;
    public decimal ClosingDebit => Balance > 0 ? Balance : 0;
    public decimal ClosingCredit => Balance < 0 ? -Balance : 0;
    public string StatusText => Balance > 0 ? "بدهکار" : Balance < 0 ? "بستانکار" : "تسویه";

    public int BouncedChequesCount { get; set; }
    public decimal BouncedChequesTotal { get; set; }
    public int OverdueInstallmentsCount { get; set; }
    public decimal OverdueInstallmentsTotal { get; set; }
}

/// <summary>نتیجه جامع گزارش حساب و کتاب طرف حساب‌ها (بدهکار و بستانکار)</summary>
public class ShopPartyAccountsReportDto
{
    public List<ShopPartyAccountRow> Items { get; set; } = new();

    public int TotalPartiesCount => Items.Count;
    public int DebtorsCount => Items.Count(x => x.Balance > 0);
    public int CreditorsCount => Items.Count(x => x.Balance < 0);
    public int SettledCount => Items.Count(x => x.Balance == 0);
    public int WithOpeningCount => Items.Count(x => x.OpeningDebit > 0 || x.OpeningCredit > 0);

    public decimal SumOpeningDebit => Items.Sum(x => x.OpeningDebit);
    public decimal SumOpeningCredit => Items.Sum(x => x.OpeningCredit);
    public decimal SumPeriodDebit => Items.Sum(x => x.PeriodDebit);
    public decimal SumPeriodCredit => Items.Sum(x => x.PeriodCredit);
    public decimal SumTotalDebit => Items.Sum(x => x.TotalDebit);
    public decimal SumTotalCredit => Items.Sum(x => x.TotalCredit);
    public decimal SumClosingDebit => Items.Sum(x => x.ClosingDebit);
    public decimal SumClosingCredit => Items.Sum(x => x.ClosingCredit);
    public decimal NetBalance => SumClosingDebit - SumClosingCredit;
}

/// <summary>یک ردیف از ریز گردش حساب و کتاب (دفتر حساب) یک طرف حساب</summary>
public class ShopPartyLedgerRow
{
    public DateTime Date { get; set; }
    public string DocNumber { get; set; } = "";
    public string KindTitle { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string BalanceStatus => RunningBalance > 0 ? "بدهکار" : RunningBalance < 0 ? "بستانکار" : "تسویه";
    public bool IsOpening { get; set; }
    public int? TransactionId { get; set; }
}

/// <summary>صورت‌حساب و ریز گردش کامل یک طرف حساب در سامانه فروشگاهی</summary>
public class ShopPartyLedgerDto
{
    public int PartyId { get; set; }
    public PartyType PartyType { get; set; }
    public string PartyName { get; set; } = "";
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public DateTime? OpeningDate { get; set; }
    public string? OpeningDescription { get; set; }

    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance => TotalDebit - TotalCredit;
    public string ClosingStatus => ClosingBalance > 0 ? "بدهکار" : ClosingBalance < 0 ? "بستانکار" : "تسویه";

    public List<ShopPartyLedgerRow> Rows { get; set; } = new();
}

