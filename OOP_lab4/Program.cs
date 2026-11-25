using System.Text;
using GameAccountLab;

// Встановлюємо кодування UTF-8 для коректного відображення та введення українських літер у терміналі
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// 1. Створюємо імітацію бази даних (DbContext вже містить початкових гравців Аллу та Богдана)
DbContext dbContext = new DbContext();

// 2. Створюємо об'єкти рівня Repository та приводимо їх до відповідних інтерфейсів
IGameAccountRepository accountRepository = new GameAccountRepository(dbContext);
IGameRepository gameRepository = new GameRepository(dbContext);

// 3. Створюємо об'єкти рівня Service (передаємо їм інтерфейси репозиторіїв)
IGameAccountService accountService = new GameAccountService(accountRepository, gameRepository);
IGameService gameService = new GameService(gameRepository, accountRepository);

// 4. Створюємо словник команд терміналу, де кожен клас реалізує спільний інтерфейс ICommand (Поліморфізм)
Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>
{
    { "1", new ShowPlayersCommand(accountService) },
    { "2", new AddPlayerCommand(accountService) },
    { "3", new ShowPlayerStatsCommand(accountService) },
    { "4", new PlayGameCommand(accountService, gameService) },
    { "5", new ShowAllGamesCommand(gameService) }
};

// 5. Головний цикл роботи програми в терміналі
bool isRunning = true;
while (isRunning)
{
    Console.WriteLine("\nОберіть команду зі списку:");
    foreach (var item in commands)
    {
        Console.Write($"  [{item.Key}] - ");
        item.Value.ShowInfo(); // Викликаємо метод інтерфейсу ICommand для показу опису команди
    }
    Console.WriteLine("  [0] - Вийти з програми");

    Console.Write("\nВведіть номер команди: ");
    string? choice = Console.ReadLine()?.Trim();

    if (choice == "0")
    {
        isRunning = false;
        Console.WriteLine("Роботу програми завершено.");
    }
    else if (choice != null && commands.TryGetValue(choice, out ICommand? selectedCommand))
    {
        // Викликаємо виконання обраної команди через інтерфейс ICommand
        selectedCommand.Execute();
    }
    else
    {
        Console.WriteLine("Невідома команда! Будь ласка, оберіть номер зі списку (0-5).");
    }
}
