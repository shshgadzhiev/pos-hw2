using POS.Application;
using POS.Domain;

namespace POS.Infrastructure;

public sealed class InMemoryGradebookRepository : IGradebookRepository
{
    private Gradebook _gradebook = new();

    public Gradebook Load() => _gradebook;

    public void Save(Gradebook gradebook) => _gradebook = gradebook;
}
