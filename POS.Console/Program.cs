using System.Text;
using Microsoft.Extensions.DependencyInjection;
using POS.Application;
using POS.Cli;
using POS.Domain;
using POS.Infrastructure;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

using var services = new ServiceCollection()
    .AddSingleton<IGradebookRepository, InMemoryGradebookRepository>()
    .AddSingleton<IGradebookExporter, CsvGradebookExporter>()
    .AddSingleton<FinalScoreCalculator>()
    .AddSingleton<AllCriteriaStrategy>()
    .AddSingleton<EvaluatedCriteriaStrategy>()
    .AddSingleton<PosFacade>()
    .AddSingleton<ConsoleMenu>()
    .BuildServiceProvider();

services.GetRequiredService<ConsoleMenu>().Run();
