using System.Globalization;
using POS.Application;
using POS.Domain;

namespace POS.Cli;

internal sealed class ConsoleMenu(PosFacade facade)
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

    public void Run()
    {
        (string Title, Action Execute)[] menu =
        [
            ("Добавить студента", AddStudent),
            ("Список студентов", ListStudents),
            ("Добавить критерий", AddCriterion),
            ("Рубрикатор", ListRubric),
            ("Выставить оценку", () => EnterEvaluation(facade.SetEvaluation, "Оценка выставлена.")),
            ("Оценки студента", ListEvaluations),
            ("Исправить оценку", () => EnterEvaluation(facade.CorrectEvaluation, "Оценка исправлена.")),
            ("Отменить оценку", CancelEvaluation),
            ("Рассчитать итоговую оценку", () => ShowResult("Итоговая оценка", facade.GetFinalScore)),
            ("Рассчитать текущий результат", () => ShowResult("Текущий результат", facade.GetCurrentScore)),
            ("Экспортировать ведомость в CSV", Export)
        ];

        Console.WriteLine("ПОС — Преподавательская оценка студентов");
        while (true)
        {
            Console.WriteLine();
            for (int index = 0; index < menu.Length; index++)
                Console.WriteLine($"{index + 1}. {menu[index].Title}");
            Console.WriteLine("0. Завершить программу");

            try
            {
                string command = Read("Команда: ").Trim();
                if (command == "0")
                    return;

                if (!int.TryParse(command, out int number) || number < 1 || number > menu.Length)
                {
                    Console.WriteLine("Неизвестная команда. Выберите номер из меню.");
                    continue;
                }

                menu[number - 1].Execute();
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("\nВвод завершён.");
                return;
            }
            catch (Exception error) when (error is GradebookException or FormatException)
            {
                Console.WriteLine($"Ошибка: {error.Message}");
            }
        }
    }

    private void AddStudent()
    {
        var student = facade.AddStudent(Read("Имя студента: "));
        Console.WriteLine($"Студент добавлен. ID: {student.Id}.");
    }

    private void ListStudents()
    {
        var students = facade.GetStudents();
        if (students.Count == 0)
            Console.WriteLine("В ведомости пока нет студентов.");

        foreach (var student in students)
            Console.WriteLine($"{student.Id}. {student.Name}");
    }

    private void AddCriterion()
    {
        string name = Read("Название критерия: ");
        decimal maximum = ReadDecimal("Максимальный балл: ");
        decimal weight = ReadDecimal("Вес: ");
        var criterion = facade.AddCriterion(name, maximum, weight);
        Console.WriteLine($"Критерий добавлен. ID: {criterion.Id}.");
    }

    private void ListRubric()
    {
        var rubric = facade.GetRubric();
        if (rubric.Count == 0)
            Console.WriteLine("В рубрикаторе пока нет критериев.");

        foreach (var criterion in rubric)
            Console.WriteLine($"{criterion.Id}. {criterion.Name} (максимум {Display(criterion.MaxScore)}, вес {Display(criterion.Weight)})");
    }

    private void EnterEvaluation(Action<int, int, decimal> apply, string confirmation)
    {
        int studentId = ReadId("ID студента: ");
        int criterionId = ReadId("ID критерия: ");
        decimal score = ReadDecimal("Балл: ");
        apply(studentId, criterionId, score);
        Console.WriteLine(confirmation);
    }

    private void ListEvaluations()
    {
        var evaluations = facade.GetEvaluations(ReadId("ID студента: "));
        if (evaluations.Count == 0)
            Console.WriteLine("Оценок пока нет.");

        foreach (var evaluation in evaluations)
            Console.WriteLine($"{evaluation.CriterionId}. {evaluation.CriterionName}: {Display(evaluation.Score)} из {Display(evaluation.MaxScore)}");
    }

    private void CancelEvaluation()
    {
        int studentId = ReadId("ID студента: ");
        int criterionId = ReadId("ID критерия: ");
        facade.CancelEvaluation(studentId, criterionId);
        Console.WriteLine("Оценка отменена.");
    }

    private static void ShowResult(string label, Func<int, decimal> calculate)
    {
        decimal score = calculate(ReadId("ID студента: "));
        Console.WriteLine($"{label}: {score.ToString("F2", Russian)} из 10.");
    }

    private void Export()
    {
        string path = Read("Путь к CSV [vedomost.csv]: ").Trim();
        if (path.Length == 0)
            path = "vedomost.csv";

        try
        {
            string fullPath = Path.GetFullPath(path);
            facade.ExportFinalScores(fullPath);
            Console.WriteLine($"Ведомость сохранена: {fullPath}");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            Console.WriteLine($"Не удалось сохранить ведомость: {error.Message}");
        }
    }

    private static string Read(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    private static int ReadId(string prompt)
    {
        if (int.TryParse(Read(prompt), out int id) && id > 0)
            return id;

        throw new FormatException("Введите положительный целый ID.");
    }

    private static decimal ReadDecimal(string prompt)
    {
        string input = Read(prompt).Trim().Replace(',', '.');
        const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (decimal.TryParse(input, styles, CultureInfo.InvariantCulture, out decimal value))
            return value;

        throw new FormatException("Введите число, например 4,5 или 4.5.");
    }

    private static string Display(decimal value) => value.ToString("G29", Russian);
}
