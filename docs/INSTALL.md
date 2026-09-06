# SmartLock-Windows — نصب و راه‌اندازی

## وضعیت فعلی

این نسخه یک **Windows security application** در حالت Development Mode است. این برنامه جایگزین Winlogon/Windows Credential Provider نیست و رمز عبور Windows را دریافت، ذخیره یا بررسی نمی‌کند.

قابلیت‌های فعلی شامل:

- تشخیص تلاش‌های ناموفق و آستانه Lockout برنامه
- Lockout واقعی در سطح خود برنامه
- Incident Timeline در حافظه
- ذخیره محلی رویدادهای امنیتی در `%LOCALAPPDATA%\SmartLock\security-events.json`
- ثبت رویدادهای امنیتی در Windows Application Event Log، در صورت فراهم بودن مجوز لازم
- Camera Evidence به‌صورت opt-in و local-only
- تشخیص Active/Idle Session State

## پیش‌نیازها

برای build از سورس:

- Windows 10/11 x64
- .NET 10 SDK
- وب‌کم در صورت فعال‌سازی Camera Evidence

## Build و اجرای مستقیم

```powershell
git clone https://github.com/9M3a1h3d9i9/SmartLock-Windows.git
cd SmartLock-Windows
dotnet restore SmartLock.sln
dotnet build SmartLock.sln --configuration Release
dotnet test SmartLock.sln --configuration Release
dotnet run --project src\SmartLock.App\SmartLock.App.csproj --configuration Release
```

## انتشار به‌صورت EXE

برای ساخت نسخه self-contained تک‌فایلی روی Windows x64:

```powershell
dotnet publish src\SmartLock.App\SmartLock.App.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishReadyToRun=true `
  -o artifacts\SmartLock-win-x64
```

فایل اجرایی در مسیر زیر قرار می‌گیرد:

`artifacts\SmartLock-win-x64\SmartLock.App.exe`

> نکته: به‌دلیل استفاده از OpenCvSharp و native camera runtime، خروجی self-contained ممکن است در عمل چند فایل runtime/native نیز داشته باشد. کل پوشه publish را جابه‌جا کنید، نه فقط EXE، مگر اینکه خروجی نهایی شما با تست runtime تأیید شده باشد.

## نصب

نسخه فعلی installer MSI/Setup Wizard ندارد. نصب فعلی به شکل portable است:

1. پوشه publish را در مسیر دلخواه، مثلاً `C:\Program Files\SmartLock-Windows` قرار دهید.
2. `SmartLock.App.exe` را اجرا کنید.
3. برای Camera Evidence، گزینه مربوط به ثبت عکس پس از تلاش ناموفق را در برنامه فعال کنید.
4. دسترسی Camera را در Windows Privacy & security بررسی کنید.

## داده‌های محلی

رویدادهای امنیتی در `%LOCALAPPDATA%\SmartLock` ذخیره می‌شوند.

تصاویر camera evidence در `%LOCALAPPDATA%\SmartLock\SecurityEvidence` ذخیره می‌شوند و سرویس capture دارای retention limit است.

## Windows Event Log

SmartLock رویدادهای مهم را به Application log ارسال می‌کند. ایجاد/ثبت source با نام `SmartLock-Windows` ممکن است به Administrator نیاز داشته باشد. اگر ثبت در Event Log ممکن نباشد، persistence محلی رویدادها همچنان مسیر اصلی audit است.

برای مشاهده:

`Event Viewer → Windows Logs → Application`

و سپس فیلتر کردن Source بر اساس `SmartLock-Windows`.

## راه‌اندازی امن

- رمز واقعی Windows را در SmartLock وارد نکنید.
- Camera Evidence را فقط در صورت رضایت و نیاز فعال کنید.
- تصاویر را خارج از سیستم یا برای شخص ثالث ارسال نکنید.
- این نسخه هنوز Windows Lock Screen واقعی نیست.
- اتصال به Winlogon/Credential Provider باید در یک مرحله جداگانه و با APIهای رسمی Windows انجام شود؛ SmartLock نباید یک password system موازی بسازد.

## عیب‌یابی

### دوربین باز نمی‌شود

- Camera را در Windows Privacy & security فعال کنید.
- مطمئن شوید برنامه دیگری دوربین را قفل نکرده است.
- اگر چند دوربین دارید، index دوربین در implementation را بررسی کنید.

### Event Log ثبت نمی‌شود

اجرای اول ممکن است برای ثبت Source به دسترسی Administrator نیاز داشته باشد. عدم دسترسی به Event Log نباید مانع ثبت local audit شود.

### برنامه از طریق EXE اجرا نمی‌شود

کل پوشه publish را کپی کنید و فقط EXE را جداگانه جابه‌جا نکنید. همچنین نسخه `win-x64` را روی Windows x64 اجرا کنید.
