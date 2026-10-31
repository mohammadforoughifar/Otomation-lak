# گزارش دانلود و کم‌حجم‌سازی پروژه `TotallCollectionSoftware`

مخزن [`mohammadforoughifar/TotallCollectionSoftware`](https://github.com/mohammadforoughifar/TotallCollectionSoftware.git) دانلود شد، عوامل افزایش کاذب حجم آن بررسی و حذف شد، و نسخهٔ کم‌حجم و استاندارد در محیط کاری آماده گردید.

---

## ۱. جدول مقایسهٔ حجم (قبل و بعد)

| وضعیت | حجم کل | تعداد فایل | توضیح |
|---|---|---|---|
| **مخزن اصلی هنگام Clone** | **۲۴۷ مگابایت** | ۳٬۱۷۸ فایل | شامل `.git`، دو فایل ZIP قدیمی در ریشه و پوشهٔ سورس تکراری قدیمی |
| **پوشهٔ پروژه پس از پاک‌سازی (`TotallCollectionSoftware/`)** | **۴۲٫۲ مگابایت** | ۱٬۷۶۳ فایل | حذف فایل‌های تکراری و آرشیوهای قدیمی؛ حفظ ۱۰۰٪ سورس مرجع (`inventory/src`) |
| **بستهٔ فشردهٔ نهایی (`TotallCollectionSoftware-Slim.zip`)** | **۱۳٫۹ مگابایت** | ۱ فایل ZIP | **کاهش ۹۴٫۴٪ حجم** نسبت به کلون اولیه (و کاهش ۶۹٪ نسبت به زیپ ۴۴ مگابایتی قبلی) |

---

## ۲. چه مواردی حجم مخزن اصلی را به ۲۴۷ مگابایت رسانده بود؟

1. **`TotallCollectionSoftware.zip` در ریشهٔ مخزن (۴۳٫۴ مگابایت):** آرشیو قدیمی ذخیره‌شده در ریشهٔ گیت.
2. **`TotallCollectionSoftware_TabsRedesigned.zip` در ریشهٔ مخزن (۴۳٫۴ مگابایت):** آرشیو قدیمی دوم در ریشهٔ گیت.
3. **پوشهٔ سورس بازنشستهٔ `TotallCollectionSoftware/src` (۳۶٫۷ مگابایت):**
   طبق مستند رسمی `CANONICAL-SOURCE-FA.md` و فایل `src/Directory.Build.props`، این پوشه بازنشسته شده و بیلد مستقیم آن متوقف شده است و هر دو فایل `Inventory.sln` (چه در ریشه و چه در `inventory/`) فقط به **`inventory/src`** اشاره می‌کنند.
4. **پوشهٔ آرشیوی `TotallCollectionSoftware/Report` (۴٫۴ مگابایت):**
   نسخهٔ تکراری قالب‌های `.mrt` که نسخهٔ اصلی و مورد استفادهٔ برنامه در مسیر `inventory/src/Inventory.Api/Reports/` قرار دارد.
5. **پوشهٔ `.git` (۶۹٫۲ مگابایت):** تاریخچهٔ کامیت‌های قبلی و فایل‌های باینری/زیپ ذخیره‌شده در تاریخچهٔ گیت.

---

## ۳. چه چیزهایی در نسخهٔ کم‌حجم حفظ و اصلاح شده است؟

- **سورس مرجع کامل (`TotallCollectionSoftware/inventory/`):** شامل `Inventory.sln`، کل پروژه‌های `Inventory.Api`، `Inventory.Client`، `Inventory.Shared`، `Agent.Console`، قالب‌های گزارش Stimulsoft (`Reports/`)، مایگریشن‌ها، تست‌ها، اسکریپت‌های SQL و پکیج محلی مودیان (`packages/`).
- **سرویس‌های جانبی و مستندات:** پوشه‌های `ocr-service`، `stt-service`، `android`، `docs`، `sql`، `tools` و تمام راهنماهای فارسی.
- **اصلاح اسکریپت‌های ریشه (`TotallCollectionSoftware/*.bat` و `*.ps1`):**
  اسکریپت‌های `publish-server.bat`، `publish-win-server.bat`، `start.bat`، `update-database.bat`، `RUN.ps1`، `deploy-single.ps1` و `rebuild.ps1` در پوشهٔ بالایی به‌روزرسانی شدند تا مستقیماً به مسیر مرجع `inventory/src` اشاره کنند.

---

## ۴. اگر بخواهید خودِ مخزن اصلی GitHub را هم سبک کنید

اگر بخواهید در مخزن اصلی `TotallCollectionSoftware` هم این فایل‌های اضافه از شاخهٔ `main` حذف شوند، کافی است دستورات زیر را در کلون آن مخزن اجرا کنید:

```bash
git rm TotallCollectionSoftware.zip TotallCollectionSoftware_TabsRedesigned.zip
git rm -r TotallCollectionSoftware/src TotallCollectionSoftware/Report
git commit -m "chore: remove legacy duplicate src tree and old root zip archives"
git push origin main
```
