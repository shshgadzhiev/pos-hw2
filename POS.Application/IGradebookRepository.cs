using POS.Domain;

namespace POS.Application;

public interface IGradebookRepository
{
    Gradebook Load();
    void Save(Gradebook gradebook);
}
