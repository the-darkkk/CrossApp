# CrossApp
Наскрізний проєкт з крос-платформного програмування.
Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan.
Призначення: облік видач примірників книг читачам.
## Схема solution
```bash
.
├── CrossApp.sln
├── Dockerfile
├── README.md
└── src
    ├── Cli
    │   ├── Cli.csproj
    │   └── Program.cs
    └── Core
        ├── Core.csproj
        └── EnvironmentInfo.cs
```
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
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true
```
- `win-x64` - Windows 64-bit
- `linux-x64` - Linux 64-bit
- `osx-x64` - macOS 64-bit

- `net10.0 / net8.0` - Версії .NET

## Середовище
.NET SDK 10.0/8.0

## Розміри publish

| RID | Режим | Розмір publish | Потрібен runtime |
| --- | ----- | -------------- | ---------------- |
| win-x64 | self-contained | ~70.5MB | Ні |
| win-x64 | framework-dependent | ~0.2MB | Так (.NET 10.0/8.0) |
| linux-x64 | self-contained | ~78.8MB | Ні |
| linux-x64 | framework-dependent | ~0.1MB | Так (.NET 10.0/8.0) |
