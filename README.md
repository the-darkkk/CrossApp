# CrossApp
Наскрізний проєкт з крос-платформного програмування.
Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan.
Призначення: облік видач примірників книг читачам.
## Запуск

```bash
dotnet build
dotnet run --project src/Cli
```

Вивід у json-форматі:

```bash
dotnet run --project src/Cli --json
```

## Публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
```
- `win-x64` - Windows 64-bit
- `linux-x64` - Linux 64-bit
- `osx-x64` - macOS 64-bit

## Середовище
.NET SDK 10.0

win-x64 publish size: 80 386 052 bytes

linux-x64 publish size: 82 621 561 bytes