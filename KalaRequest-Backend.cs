// ============================================================================
//  متدِ اصلاح‌شده‌ی GetListOfTaminDarkhastViewModel
//  جایگزینِ متدِ فعلی در Repository/Service خودتان کنید.
//
//  چه چیزهایی اصلاح شده:
//   ۱) KalaTeadCount در نسخه‌ی قبلی کاملاً کامنت بود  => همیشه صفر چاپ می‌شد
//   ۲) join روی Tamin_Tbls انجام شده بود اما خروجی‌اش (j) هیچ‌جا استفاده نمی‌شد؛
//      فقط ردیف‌ها را ضرب می‌کرد و هر Sum/First را خراب می‌کرد  => حذف شد
//   ۳) join روی Darkhast / Anbar / ElamieErsal هم ردیف‌ها را ضرب می‌کرد
//      => به Lookup (Dictionary) تبدیل شد؛ حالا به‌ازای هر کالا یک ردیف داریم
//   ۴) g.SematId و d.CodeKala در صورت نبودِ ردیفِ مرتبط NullReferenceException
//      می‌دادند و کل متد در catch می‌افتاد و null برمی‌گشت  => کاملاً ایمن شد
//   ۵) فیلترها از ۳ بلوکِ if به یک زنجیره تبدیل شد و همیشه مرتب‌سازی می‌شود
//   ۶) سه فیلدِ جدیدِ گزارش اضافه شد: Tozihat / DarkhastKonandeName / TaiidKonandeName
//
//  ⚠️ در نسخه‌ی قبلی UserName و AnbarDar هر دو از زنجیره‌ی Anbar می‌آمدند،
//     یعنی UserName در واقع «انباردار» بود نه «درخواست‌کننده».
//     نامِ درخواست‌کننده حالا در فیلدِ جداگانه‌ی DarkhastKonandeName است.
// ============================================================================

/// <summary>
/// لیست خرید: کالاهایی که مدیریت تایید کرده و هنوز اعلامیه‌ی ارسال ندارند.
/// </summary>
public List<DarkhastViewModel> GetListOfTaminDarkhastViewModel(string Filter = "", DateTime? Date = null)
{
    try
    {
        // ---------- ۱) خواندنِ جداول (یک بار) ----------
        var listuser        = _db.Oto_TBL_Users.ToList();
        var ListSemat       = _db.Oto_TBL_Semats.ToList();
        var ListModiriat    = db.Modiriat_Tbls.ToList();
        var ListSharhKala   = db.SharhKala_Tbls.ToList();
        var ListDarkhast    = db.Darkhast_Tbls.ToList();
        var ListAnbar       = db.AnbarDarkhast_Tbls.ToList();
        var ListElamieErsal = db.ElamiehErsal_Tbls.ToList();
        // جدول Tamin_Tbls دیگر خوانده نمی‌شود (توضیح در بالا، بند ۲).

        // ---------- ۲) ساختِ ایندکس‌ها (جلوگیری از ضرب شدنِ ردیف‌ها) ----------

        // کالاهایی که اعلامیه‌ی ارسال دارند => نباید در لیست خرید بیایند
        var kalaErsalShode = ListElamieErsal
            .GroupBy(x => x.KalaId)
            .ToDictionary(g => g.Key, g => true);

        // مشخصات کالا
        var kalaById = ListSharhKala
            .GroupBy(k => k.KalaId)
            .ToDictionary(g => g.Key, g => g.First());

        // نامِ درخواست‌کننده  (مسیر: درخواست -> سمت -> کاربر)
        var darkhastKonandeByKala =
            (from d in ListDarkhast
             join s in ListSemat on d.SematId equals s.Semat_Id
             join u in listuser  on s.UserId  equals u.UserId
             select new { d.KalaId, Name = s.Title + "/" + u.F_Name + " " + u.L_Name })
            .GroupBy(x => x.KalaId)
            .ToDictionary(g => g.Key, g => g.First().Name);

        // نامِ انباردار  (مسیر: انبار -> سمت -> کاربر)
        var anbarDarByKala =
            (from t in ListAnbar
             join s in ListSemat on t.SematId equals s.Semat_Id
             join u in listuser  on s.UserId  equals u.UserId
             select new { t.KalaId, Name = s.Title + "/" + u.F_Name + " " + u.L_Name })
            .GroupBy(x => x.KalaId)
            .ToDictionary(g => g.Key, g => g.First().Name);

        // نامِ تاییدکننده / مدیر عامل  (مسیر: مدیریت -> سمت -> کاربر)
        // اگر تاییدکننده در سیستم شما جای دیگری است (مثلاً یک کاربرِ ثابت)،
        // این بلوک را با مقدارِ دلخواه جایگزین کنید.
        var taiidKonandeByKala =
            (from a in ListModiriat
             join s in ListSemat on a.SematId equals s.Semat_Id
             join u in listuser  on s.UserId  equals u.UserId
             select new { a.KalaId, Name = s.Title + "/" + u.F_Name + " " + u.L_Name })
            .GroupBy(x => x.KalaId)
            .ToDictionary(g => g.Key, g => g.First().Name);

        // ---------- ۳) گروه‌بندی بر اساس کالا ----------
        var item =
            (from a in ListModiriat
             where !kalaErsalShode.ContainsKey(a.KalaId)      // ارسال نشده باشد
             group a by a.KalaId into grp
             let kala = kalaById.ContainsKey(grp.Key) ? kalaById[grp.Key] : null
             select new DarkhastViewModel
             {
                 KalaId   = grp.Key,
                 CodeKala = (kala == null || kala.CodeKala == null) ? "" : kala.CodeKala,
                 KalaName = (kala == null || kala.KalaName == null) ? "" : kala.KalaName,

                 // ★ رفعِ مشکلِ صفر: مجموعِ تعدادِ درخواستیِ همان کالا
                 //   اگر KalaTead در مدل شما int غیر-nullable است، عبارتِ "?? 0"
                 //   را حذف کنید تا warning نگیرید.
                 KalaTeadCount = grp.Sum(x => x.KalaTead ?? 0),

                 HeaderDarkhastId = grp.First().HeaderDarkhastId,
                 ShomareDarkhast  = grp.First().ShomareDarkhast,
                 DateSabt         = grp.Max(x => x.DateSabt),   // جدیدترین تاریخِ گروه

                 ShomareDarkhastslist = string.Join(", ",
                     grp.Select(x => x.ShomareDarkhast)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()),

                 // ★ جدید — توضیحاتِ هر درخواست (ستون جدیدِ گزارش)
                 Tozihat = string.Join(" | ",
                     grp.Select(x => x.Sharh)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()),

                 // ★ جدید — نامِ درخواست‌کننده (ستون جدیدِ گزارش)
                 DarkhastKonandeName = darkhastKonandeByKala.ContainsKey(grp.Key)
                     ? darkhastKonandeByKala[grp.Key] : "",

                 // ★ جدید — نامِ تاییدکننده / مدیر عامل (پانویسِ گزارش)
                 TaiidKonandeName = taiidKonandeByKala.ContainsKey(grp.Key)
                     ? taiidKonandeByKala[grp.Key] : "",

                 // ---- حفظِ سازگاری با کد قدیمی ----
                 UserName  = anbarDarByKala.ContainsKey(grp.Key) ? anbarDarByKala[grp.Key] : "",   // انباردار
                 SematName = taiidKonandeByKala.ContainsKey(grp.Key) ? taiidKonandeByKala[grp.Key] : "",
             })
            .ToList();

        // ---------- ۴) فیلتر و مرتب‌سازی ----------
        IEnumerable<DarkhastViewModel> result = item;

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var f = Filter.Trim();
            result = result.Where(r =>
                   (r.KalaName             != null && r.KalaName.Contains(f))
                || (r.CodeKala             != null && r.CodeKala.Contains(f))
                || (r.ShomareDarkhastslist != null && r.ShomareDarkhastslist.Contains(f)));
        }

        if (Date.HasValue)
        {
            var d = Date.Value.Date;
            result = result.Where(r => r.DateSabt.Date == d);
        }

        return result.OrderByDescending(r => r.DateSabt).ToList();
    }
    catch (Exception ex)
    {
        // TODO: در صورت داشتن سیستمِ لاگ، اینجا ثبت کنید
        System.Diagnostics.Debug.WriteLine("GetListOfTaminDarkhastViewModel: " + ex.Message);
        return null;
    }
}


// ============================================================================
//  سه پراپرتی که باید به کلاس DarkhastViewModel اضافه کنید:
// ============================================================================
//
//  public string Tozihat             { get; set; }   // توضیحاتِ هر درخواست
//  public string DarkhastKonandeName { get; set; }   // نامِ درخواست‌کننده
//  public string TaiidKonandeName    { get; set; }   // نامِ تاییدکننده (مدیر عامل)
//
// ============================================================================


// ============================================================================
//  اختیاری — اگر قانونِ کسب‌وکار شماست که کالاهایِ «در حالِ تامین» دوباره در
//  لیست خرید نیایند، این بلوک را قبل از گروه‌بندی اضافه کنید:
// ============================================================================
//
//  var kalaDarHaleTamin = db.Tamin_Tbls
//      .GroupBy(x => x.KalaId)
//      .ToDictionary(g => g.Key, g => true);
//
//  و در شرطِ where:
//      where !kalaErsalShode.ContainsKey(a.KalaId)
//         && !kalaDarHaleTamin.ContainsKey(a.KalaId)
//
// ============================================================================
//  اختیاری — اگر می‌خواهید «تعدادِ باقی‌مانده برای خرید» نمایش داده شود
//  (یعنی تعدادِ تاییدشده منهایِ تعدادی که قبلاً تامین شده):
// ============================================================================
//
//  var tedadTaminShodeByKala = db.Tamin_Tbls
//      .GroupBy(x => x.KalaId)
//      .ToDictionary(g => g.Key, g => g.Sum(x => x.KalaTead ?? 0));
//
//  و در select:
//      KalaTeadCount = grp.Sum(x => x.KalaTead ?? 0)
//                    - (tedadTaminShodeByKala.ContainsKey(grp.Key)
//                          ? tedadTaminShodeByKala[grp.Key] : 0),
//
// ============================================================================
