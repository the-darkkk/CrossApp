using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Core;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    var sysInfo = new
    {
        App = "CrossApp – практикум з крос-платформного програмування",
        Student = "Гаргас Олег",
        Group = "ФЕІ-32с",
        BuildNote = report.BuildNote,
        OSDescription = report.OsDescription,
        Runtime = report.FrameworkDescription,
        ProcessArchitecture = report.ProcessArchitecture.ToString(),
        DetectedRid = report.DetectedRid,
        ReportedRid = report.ReportedRid,
        BaseDirectory = report.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan"
    };

    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
        WriteIndented = true
    };

    Console.WriteLine(JsonSerializer.Serialize(sysInfo, options));
}
else
{
    Console.WriteLine("CrossApp – інформація про середовище");
    Console.WriteLine("Студент: Гаргас Олег, група ФЕІ-32с");
    Console.WriteLine($"Збірка: {report.BuildNote}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС             : {report.OsDescription}");
    Console.WriteLine($"Runtime        : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура    : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено): {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог        : {report.BaseDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan");
}