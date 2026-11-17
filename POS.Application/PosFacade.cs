using POS.Domain;

namespace POS.Application;

public sealed record EvaluationDetails(int CriterionId, string CriterionName, decimal Score, decimal MaxScore);

public sealed class PosFacade(
    IGradebookRepository repository,
    IGradebookExporter exporter,
    FinalScoreCalculator calculator,
    AllCriteriaStrategy finalStrategy,
    EvaluatedCriteriaStrategy currentStrategy)
{
    public Student AddStudent(string name) => Update(gradebook => gradebook.AddStudent(name));

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight) =>
        Update(gradebook => gradebook.AddCriterion(name, maxScore, weight));

    public IReadOnlyList<Student> GetStudents() => repository.Load().Students;

    public IReadOnlyList<Criterion> GetRubric() => repository.Load().Rubric;

    public IReadOnlyList<EvaluationDetails> GetEvaluations(int studentId)
    {
        var gradebook = repository.Load();
        return gradebook.GetEvaluations(studentId).Select(evaluation =>
        {
            var criterion = gradebook.GetCriterion(evaluation.CriterionId);
            return new EvaluationDetails(criterion.Id, criterion.Name, evaluation.Score.Value, criterion.MaxScore);
        }).ToArray();
    }

    public void SetEvaluation(int studentId, int criterionId, decimal score) =>
        Update(gradebook => gradebook.SetEvaluation(studentId, criterionId, score));

    public void CorrectEvaluation(int studentId, int criterionId, decimal score) =>
        Update(gradebook => gradebook.CorrectEvaluation(studentId, criterionId, score));

    public void CancelEvaluation(int studentId, int criterionId) =>
        Update(gradebook => gradebook.CancelEvaluation(studentId, criterionId));

    public decimal GetFinalScore(int studentId) =>
        calculator.Calculate(repository.Load(), studentId, finalStrategy);

    public decimal GetCurrentScore(int studentId) =>
        calculator.Calculate(repository.Load(), studentId, currentStrategy);

    public void ExportFinalScores(string path)
    {
        var gradebook = repository.Load();
        var rows = gradebook.Students.Select(student => new GradebookRow(
            student.Id, student.Name, calculator.Calculate(gradebook, student.Id, finalStrategy))).ToArray();
        exporter.Export(path, rows);
    }

    private TResult Update<TResult>(Func<Gradebook, TResult> change)
    {
        var gradebook = repository.Load();
        var result = change(gradebook);
        repository.Save(gradebook);
        return result;
    }

    private void Update(Action<Gradebook> change)
    {
        var gradebook = repository.Load();
        change(gradebook);
        repository.Save(gradebook);
    }
}
