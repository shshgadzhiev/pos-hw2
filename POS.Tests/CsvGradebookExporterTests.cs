using System.Text;
using POS.Application;
using POS.Infrastructure;

namespace POS.Tests;

public sealed class CsvGradebookExporterTests
{
    [Fact]
    public void WritesUtf8AndEscapesCommasQuotesAndNewlines()
    {
        var directory = Directory.CreateTempSubdirectory("pos-tests-");
        string path = Path.Combine(directory.FullName, "ведомость.csv");
        try
        {
            File.WriteAllText(path, "старый отчёт");
            new CsvGradebookExporter().Export(path, [new GradebookRow(1, "Анна, \"Аня\"\nИванова", 7.5m)]);

            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
            Assert.Equal("ID,Студент,Итог из 10\r\n1,\"Анна, \"\"Аня\"\"\nИванова\",7.50\r\n",
                File.ReadAllText(path));
            Assert.Single(directory.GetFiles());
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
