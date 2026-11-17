using System.Globalization;
using System.Text;
using POS.Application;

namespace POS.Infrastructure;

public sealed class CsvGradebookExporter : IGradebookExporter
{
    public void Export(string path, IReadOnlyList<GradebookRow> rows)
    {
        string destination = Path.GetFullPath(path);
        string temporaryPath = Path.Combine(Path.GetDirectoryName(destination)!, $".pos-{Guid.NewGuid():N}.tmp");

        try
        {
            using (var writer = new StreamWriter(temporaryPath, false, new UTF8Encoding(true)))
            {
                writer.NewLine = "\r\n";
                writer.WriteLine("ID,Студент,Итог из 10");
                foreach (var row in rows)
                {
                    var id = row.StudentId.ToString(CultureInfo.InvariantCulture);
                    var score = row.FinalScore.ToString("F2", CultureInfo.InvariantCulture);
                    var name = row.StudentName.Replace("\"", "\"\"");
                    writer.WriteLine($"{id},\"{name}\",{score}");
                }
            }
            File.Move(temporaryPath, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
