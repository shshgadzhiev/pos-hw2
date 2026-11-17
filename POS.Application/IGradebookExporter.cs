namespace POS.Application;

public sealed record GradebookRow(int StudentId, string StudentName, decimal FinalScore);

public interface IGradebookExporter
{
    void Export(string path, IReadOnlyList<GradebookRow> rows);
}
