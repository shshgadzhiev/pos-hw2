using POS.Domain;

namespace POS.Tests;

public sealed class FinalScoreCalculatorTests
{
    private readonly FinalScoreCalculator _calculator = new();
    private readonly AllCriteriaStrategy _final = new();
    private readonly EvaluatedCriteriaStrategy _current = new();

    [Fact]
    public void UsesWeightsAndNormalizesDifferentMaximums()
    {
        var gradebook = new Gradebook();
        var student = gradebook.AddStudent("Анна");
        var project = gradebook.AddCriterion("Проект", 10m, 3m);
        var test = gradebook.AddCriterion("Тест", 5m, 1m);

        gradebook.SetEvaluation(student.Id, project.Id, 8m);
        gradebook.SetEvaluation(student.Id, test.Id, 5m);

        Assert.Equal(8.50m, _calculator.Calculate(gradebook, student.Id, _final));
        Assert.Equal(8.50m, _calculator.Calculate(gradebook, student.Id, _current));

        gradebook.CorrectEvaluation(student.Id, test.Id, 4m);
        Assert.Equal(8m, _calculator.Calculate(gradebook, student.Id, _final));
    }

    [Fact]
    public void DistinguishesMissingEvaluationFromExplicitZero()
    {
        var gradebook = new Gradebook();
        var student = gradebook.AddStudent("Анна");
        var other = gradebook.AddStudent("Борис");
        var project = gradebook.AddCriterion("Проект", 10m, 3m);
        var test = gradebook.AddCriterion("Тест", 5m, 1m);
        gradebook.SetEvaluation(other.Id, test.Id, 5m);
        gradebook.SetEvaluation(student.Id, project.Id, 8m);

        Assert.Equal(6m, _calculator.Calculate(gradebook, student.Id, _final));
        Assert.Equal(8m, _calculator.Calculate(gradebook, student.Id, _current));

        gradebook.SetEvaluation(student.Id, test.Id, 0m);
        Assert.Equal(6m, _calculator.Calculate(gradebook, student.Id, _current));

        gradebook.CorrectEvaluation(student.Id, project.Id, 10m);
        Assert.Equal(7.50m, _calculator.Calculate(gradebook, student.Id, _final));
        gradebook.CancelEvaluation(student.Id, project.Id);
        Assert.Equal(0m, _calculator.Calculate(gradebook, student.Id, _final));
    }

    [Fact]
    public void ReturnsZeroWithoutEvaluationsOrCriteria()
    {
        var gradebook = new Gradebook();
        var student = gradebook.AddStudent("Анна");

        Assert.Equal(0m, _calculator.Calculate(gradebook, student.Id, _final));
        Assert.Equal(0m, _calculator.Calculate(gradebook, student.Id, _current));

        gradebook.AddCriterion("Тест", 10m, 3m);
        Assert.Equal(0m, _calculator.Calculate(gradebook, student.Id, _final));
        Assert.Equal(0m, _calculator.Calculate(gradebook, student.Id, _current));
    }

    [Fact]
    public void RoundsMidpointAwayFromZero()
    {
        var gradebook = new Gradebook();
        var student = gradebook.AddStudent("Анна");
        var test = gradebook.AddCriterion("Тест", 10m, 1m);
        gradebook.SetEvaluation(student.Id, test.Id, 1.005m);

        Assert.Equal(1.01m, _calculator.Calculate(gradebook, student.Id, _final));
    }

    [Fact]
    public void CalculatesWithExtremePositiveWeights()
    {
        var gradebook = new Gradebook();
        var student = gradebook.AddStudent("Анна");
        var small = gradebook.AddCriterion("Тест", 10m, 0.0000000000000000000000000001m);
        gradebook.SetEvaluation(student.Id, small.Id, 1m);
        Assert.Equal(1m, _calculator.Calculate(gradebook, student.Id, _final));

        var large = gradebook.AddCriterion("Проект", 10m, decimal.MaxValue);
        gradebook.AddCriterion("Защита", 10m, decimal.MaxValue);
        gradebook.SetEvaluation(student.Id, large.Id, 10m);

        Assert.Equal(5m, _calculator.Calculate(gradebook, student.Id, _final));
    }
}
