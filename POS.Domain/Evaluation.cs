namespace POS.Domain;

public readonly record struct Score
{
    internal Score(decimal value)
    {
        if (value < 0)
            throw new GradebookException("Балл не может быть отрицательным.");

        Value = value;
    }

    public decimal Value { get; }
}

public sealed record Evaluation
{
    private Evaluation(int studentId, int criterionId, Score score)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
    }

    public int StudentId { get; }
    public int CriterionId { get; }
    public Score Score { get; }

    internal static Evaluation Create(Student student, Criterion criterion, decimal value)
    {
        var score = new Score(value);
        if (score.Value > criterion.MaxScore)
            throw new GradebookException($"Балл превышает максимум критерия ({criterion.MaxScore}).");

        return new Evaluation(student.Id, criterion.Id, score);
    }
}
