using POS.Domain;

namespace POS.Tests;

public sealed class GradebookTests
{
    [Fact]
    public void AddsStudentsAndCriteriaWithUniqueIds()
    {
        var gradebook = new Gradebook();
        var student = gradebook.AddStudent(" Анна ");
        var criterion = gradebook.AddCriterion(" Проект ", 10m, 3m);

        Assert.Equal("Анна", student.Name);
        Assert.Equal("Проект", criterion.Name);
        Assert.Equal(10m, criterion.MaxScore);
        Assert.Equal(3m, criterion.Weight);
        Assert.NotEqual(student.Id, gradebook.AddStudent("Анна").Id);
        Assert.NotEqual(criterion.Id, gradebook.AddCriterion("Проект", 5m, 1m).Id);
        Assert.Same(student, gradebook.GetStudent(student.Id));
        Assert.Same(criterion, gradebook.GetCriterion(criterion.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t")]
    [InlineData(null)]
    public void RejectsMissingNames(string? name)
    {
        var gradebook = new Gradebook();

        Assert.Throws<GradebookException>(() => gradebook.AddStudent(name!));
        Assert.Throws<GradebookException>(() => gradebook.AddCriterion(name!, 10m, 1m));
        Assert.Empty(gradebook.Students);
        Assert.Empty(gradebook.Rubric);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void RejectsNonPositiveMaximumOrWeight(int maximum, int weight)
    {
        var gradebook = new Gradebook();

        Assert.Throws<GradebookException>(() => gradebook.AddCriterion("Проект", maximum, weight));
        Assert.Empty(gradebook.Rubric);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void AcceptsScoresAtBothBounds(int score)
    {
        var (gradebook, student, criterion) = CreateGradebook();

        gradebook.SetEvaluation(student.Id, criterion.Id, score);
        var evaluation = Assert.Single(gradebook.GetEvaluations(student.Id));

        Assert.Equal(student.Id, evaluation.StudentId);
        Assert.Equal(criterion.Id, evaluation.CriterionId);
        Assert.Equal((decimal)score, evaluation.Score.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void RejectsScoresOutsideBounds(int invalidScore)
    {
        var (gradebook, student, criterion) = CreateGradebook();

        Assert.Throws<GradebookException>(() => gradebook.SetEvaluation(student.Id, criterion.Id, invalidScore));
        Assert.Empty(gradebook.GetEvaluations(student.Id));
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 2)]
    public void RejectsEvaluationForUnknownStudentOrCriterion(int studentId, int criterionId)
    {
        var (gradebook, student, _) = CreateGradebook();

        Assert.Throws<GradebookException>(() => gradebook.SetEvaluation(studentId, criterionId, 4m));
        Assert.Empty(gradebook.GetEvaluations(student.Id));
    }

    [Fact]
    public void SupportsEvaluationLifecycleAndPreservesScoreOnFailedChange()
    {
        var (gradebook, student, criterion) = CreateGradebook();
        int studentId = student.Id;
        int criterionId = criterion.Id;

        Assert.Throws<GradebookException>(() => gradebook.CorrectEvaluation(studentId, criterionId, 4m));
        Assert.Throws<GradebookException>(() => gradebook.CancelEvaluation(studentId, criterionId));

        gradebook.SetEvaluation(studentId, criterionId, 4m);
        var initial = Assert.Single(gradebook.GetEvaluations(studentId));
        Assert.Throws<GradebookException>(() => gradebook.SetEvaluation(studentId, criterionId, 8m));
        Assert.Throws<GradebookException>(() => gradebook.CorrectEvaluation(studentId, criterionId, 11m));
        Assert.Same(initial, Assert.Single(gradebook.GetEvaluations(studentId)));

        gradebook.CorrectEvaluation(studentId, criterionId, 8m);
        Assert.Equal(8m, Assert.Single(gradebook.GetEvaluations(studentId)).Score.Value);

        gradebook.CancelEvaluation(studentId, criterionId);
        Assert.Empty(gradebook.GetEvaluations(studentId));
        gradebook.SetEvaluation(studentId, criterionId, 0m);
        Assert.Equal(0m, Assert.Single(gradebook.GetEvaluations(studentId)).Score.Value);
    }

    private static (Gradebook Gradebook, Student Student, Criterion Criterion) CreateGradebook()
    {
        var gradebook = new Gradebook();
        return (gradebook, gradebook.AddStudent("Анна"), gradebook.AddCriterion("Проект", 10m, 3m));
    }
}
