using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

if (args.Contains("--json"))
{
    var sysInfo = new
    {
        App = "CrossApp – практикум з крос-платформного програмування",
        Student = "Гаргас Олег",
        Group = "ФЕІ-32с",
        OSDescription = RuntimeInformation.OSDescription,
        OSVersion = Environment.OSVersion.ToString(),
        ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
        DotNetVersion = Environment.Version.ToString(),
        Runtime = RuntimeInformation.FrameworkDescription,
        BaseDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan"
    };

    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };

    Console.WriteLine(JsonSerializer.Serialize(sysInfo, options));
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine("Студент: Гаргас Олег, група ФЕІ-32с");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription)  : {RuntimeInformation.OSDescription}");
    Console.WriteLine($"ОС (Environment)    : {Environment.OSVersion}");
    Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)   : {Environment.Version}");
    Console.WriteLine($"Runtime             : {RuntimeInformation.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку  : {AppContext.BaseDirectory}");
    Console.WriteLine($"Поточний каталог    : {Environment.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan");
}