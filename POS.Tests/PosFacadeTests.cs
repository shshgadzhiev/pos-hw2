using POS.Application;
using POS.Domain;

namespace POS.Tests;

public sealed class PosFacadeTests
{
    [Fact]
    public void UpdatesRepositoryAndExportsOnlyFinalScores()
    {
        var repository = new RecordingRepository();
        var exporter = new RecordingExporter();
        var facade = new PosFacade(repository, exporter, new FinalScoreCalculator(),
            new AllCriteriaStrategy(), new EvaluatedCriteriaStrategy());

        var student = facade.AddStudent("Анна");
        var scored = facade.AddCriterion("Проект", 10m, 3m);
        facade.AddCriterion("Тест", 5m, 1m);
        facade.SetEvaluation(student.Id, scored.Id, 8m);

        Assert.Throws<GradebookException>(() => facade.CorrectEvaluation(student.Id, scored.Id, 12m));
        Assert.Equal(4, repository.SaveCount);
        Assert.Equal(scored.Name, Assert.Single(facade.GetEvaluations(student.Id)).CriterionName);
        Assert.Equal(8m, facade.GetCurrentScore(student.Id));

        facade.ExportFinalScores("ведомость.csv");

        Assert.Equal("ведомость.csv", exporter.Path);
        Assert.Equal(new GradebookRow(student.Id, student.Name, 6m), Assert.Single(exporter.Rows));

        facade.CorrectEvaluation(student.Id, scored.Id, 10m);
        Assert.Equal(7.50m, facade.GetFinalScore(student.Id));
        facade.CancelEvaluation(student.Id, scored.Id);
        Assert.Equal(0m, facade.GetFinalScore(student.Id));
        Assert.Equal(6, repository.SaveCount);
    }

    private sealed class RecordingRepository : IGradebookRepository
    {
        private Gradebook _gradebook = new();
        public int SaveCount { get; private set; }

        public Gradebook Load() => _gradebook;

        public void Save(Gradebook gradebook)
        {
            _gradebook = gradebook;
            SaveCount++;
        }
    }

    private sealed class RecordingExporter : IGradebookExporter
    {
        public string? Path { get; private set; }
        public IReadOnlyList<GradebookRow> Rows { get; private set; } = [];

        public void Export(string path, IReadOnlyList<GradebookRow> rows)
        {
            Path = path;
            Rows = rows;
        }
    }
}
