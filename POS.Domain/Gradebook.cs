namespace POS.Domain;

public sealed class Gradebook
{
    private readonly Dictionary<int, Student> _students = new();
    private readonly Dictionary<int, Criterion> _rubric = new();
    private readonly Dictionary<(int StudentId, int CriterionId), Evaluation> _evaluations = new();

    public IReadOnlyList<Student> Students => _students.Values.OrderBy(student => student.Id).ToArray();
    public IReadOnlyList<Criterion> Rubric => _rubric.Values.OrderBy(criterion => criterion.Id).ToArray();

    public Student AddStudent(string name)
    {
        var student = new Student(_students.Count + 1, name);
        _students.Add(student.Id, student);
        return student;
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var criterion = new Criterion(_rubric.Count + 1, name, maxScore, weight);
        _rubric.Add(criterion.Id, criterion);
        return criterion;
    }

    public Student GetStudent(int id) => _students.TryGetValue(id, out var student)
        ? student
        : throw new GradebookException($"Студент с ID {id} не найден.");

    public Criterion GetCriterion(int id) => _rubric.TryGetValue(id, out var criterion)
        ? criterion
        : throw new GradebookException($"Критерий с ID {id} не найден.");

    public IReadOnlyList<Evaluation> GetEvaluations(int studentId)
    {
        GetStudent(studentId);
        return _evaluations.Values
            .Where(evaluation => evaluation.StudentId == studentId)
            .OrderBy(evaluation => evaluation.CriterionId)
            .ToArray();
    }

    public void SetEvaluation(int studentId, int criterionId, decimal score)
    {
        var evaluation = Evaluation.Create(GetStudent(studentId), GetCriterion(criterionId), score);
        if (!_evaluations.TryAdd((studentId, criterionId), evaluation))
            throw new GradebookException("Оценка уже выставлена. Выберите исправление оценки.");
    }

    public void CorrectEvaluation(int studentId, int criterionId, decimal score)
    {
        var key = (studentId, criterionId);
        var student = GetStudent(studentId);
        var criterion = GetCriterion(criterionId);
        if (!_evaluations.ContainsKey(key))
            throw new GradebookException("Оценка ещё не выставлена.");

        _evaluations[key] = Evaluation.Create(student, criterion, score);
    }

    public void CancelEvaluation(int studentId, int criterionId)
    {
        GetStudent(studentId);
        GetCriterion(criterionId);
        if (!_evaluations.Remove((studentId, criterionId)))
            throw new GradebookException("Оценка ещё не выставлена.");
    }
}
