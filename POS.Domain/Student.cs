namespace POS.Domain;

public sealed class Student
{
    internal Student(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new GradebookException("Имя студента не может быть пустым.");

        Id = id;
        Name = name.Trim();
    }

    public int Id { get; }
    public string Name { get; }
}
