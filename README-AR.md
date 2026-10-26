# SUHOOLTECH — المرحلة 6: بناء API وإخراج Publish عبر GitHub Actions

## إصلاح مهم
تم توحيد مفاتيح المستخدمين إلى `string` في ASP.NET Core Identity الافتراضي. هذا النوع هو الأقرب إلى جداول Identity القياسية التي تستخدم `nvarchar(450)`، بخلاف نسخة سابقة استخدمت `Guid` وكانت تحتاج مخططًا مختلفًا.

## كيفية تشغيل البناء من الجوال
1. ارفع محتويات هذه الحزمة إلى مستودع GitHub الخاص بالمشروع، مع الحفاظ على مجلد `.github/workflows`.
2. افتح تبويب Actions في GitHub.
3. شغّل `Build SUHOOLTECH API` عبر Run workflow.
4. إذا نجح التشغيل، نزّل Artifact باسم `SUHOOLTECH-API-PUBLISH-AND-SQL`.
5. فك ضغط الملف على الجوال وتأكد من وجود `SuhoolTech.Api.dll` و`web.config` و`Identity-Initial.sql`.

## تنبيه قاعدة البيانات
`Identity-Initial.sql` ينشئ مخطط ASP.NET Core Identity فقط، ولا يحوّل الجداول الخمسين من PostgreSQL. افحص السكربت قبل تشغيله، وشغله فقط على قاعدة SQL Server جديدة مخصصة للاختبار. لا تشغله على قاعدة إنتاج أو قاعدة تحتوي بيانات.

## إعدادات الخادم
اضبط متغيرات البيئة على الخادم بدل حفظ الأسرار في GitHub:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret` (سر عشوائي قوي لا يقل عن 32 بايت UTF-8)
- اختياري: `Jwt__Issuer`
- اختياري: `Jwt__Audience`

## حدود هذه الحزمة
لم يتم تشغيل بناء .NET في بيئة إعداد الملفات هذه، لأن .NET SDK غير مثبت هنا. GitHub Actions هو الذي سيحاول البناء والتحقق. لا تعتبر الحزمة ناجحة إلا بعد نجاح الـ workflow. هذا API تأسيسي فقط: لا يهاجر حسابات Supabase، ولا يطبق MFA كاملًا، ولا ينفذ صلاحيات أنظمة SUHOOLTECH أو وحدات الأعمال بعد.
