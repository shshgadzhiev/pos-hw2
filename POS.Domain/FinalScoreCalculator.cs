namespace POS.Domain;

public interface IFinalScoreStrategy
{
    bool Includes(Score? score);
}

public sealed class AllCriteriaStrategy : IFinalScoreStrategy
{
    public bool Includes(Score? score) => true;
}

public sealed class EvaluatedCriteriaStrategy : IFinalScoreStrategy
{
    public bool Includes(Score? score) => score.HasValue;
}

public sealed class FinalScoreCalculator
{
    public decimal Calculate(Gradebook gradebook, int studentId, IFinalScoreStrategy strategy)
    {
        var evaluations = gradebook.GetEvaluations(studentId)
            .ToDictionary(evaluation => evaluation.CriterionId, evaluation => evaluation.Score);
        var criteria = gradebook.Rubric.Select(criterion =>
        {
            Score? score = evaluations.TryGetValue(criterion.Id, out var value) ? value : null;
            return (Criterion: criterion, Score: score);
        }).Where(entry => strategy.Includes(entry.Score)).ToArray();

        if (criteria.Length == 0)
            return 0m;

        decimal largestWeight = criteria.Max(entry => entry.Criterion.Weight);
        decimal scale = largestWeight < 1m || largestWeight >= decimal.MaxValue / criteria.Length ? largestWeight : 1m;
        decimal totalWeight = 0m;
        decimal weightedScore = 0m;
        foreach (var (criterion, score) in criteria)
        {
            decimal weight = criterion.Weight / scale;
            decimal fraction = (score?.Value ?? 0m) / criterion.MaxScore;
            totalWeight += weight;
            weightedScore += fraction * weight;
        }

        return Math.Round(weightedScore / totalWeight * 10m, 2, MidpointRounding.AwayFromZero);
    }
}
