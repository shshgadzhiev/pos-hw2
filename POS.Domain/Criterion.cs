namespace POS.Domain;

public sealed class Criterion
{
    internal Criterion(int id, string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new GradebookException("Название критерия не может быть пустым.");
        if (maxScore <= 0)
            throw new GradebookException("Максимальный балл должен быть больше нуля.");
        if (weight <= 0)
            throw new GradebookException("Вес должен быть больше нуля.");

        Id = id;
        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }

    public int Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }
}
