namespace GameAccountLab;

// Інтерфейс команди терміналу.
public interface ICommand
{
    void ShowInfo(); // Відображає опис та можливості команди в меню терміналу
    void Execute();  // Виконує основну дію команди
}

// Команда для відображення всіх гравців із бази даних.
public class ShowPlayersCommand : ICommand
{
    private readonly IGameAccountService accountService;

    public ShowPlayersCommand(IGameAccountService accountService)
    {
        this.accountService = accountService;
    }

    public void ShowInfo()
    {
        Console.WriteLine("Відобразити всіх гравців з бази даних");
    }

    public void Execute()
    {
        Console.WriteLine("\n--- Список всіх гравців у базі даних ---");
        List<GameAccount> players = accountService.ReadAccounts();

        if (players.Count == 0)
        {
            Console.WriteLine("У базі даних ще немає жодного гравця.");
            return;
        }

        foreach (GameAccount player in players)
        {
            Console.WriteLine($"  {player}");
        }
    }
}

// Команда для додавання нового гравця (з вибором типу акаунту).
public class AddPlayerCommand : ICommand
{
    private readonly IGameAccountService accountService;

    public AddPlayerCommand(IGameAccountService accountService)
    {
        this.accountService = accountService;
    }

    public void ShowInfo()
    {
        Console.WriteLine("Додати нового гравця (з вибором типу акаунту)");
    }

    public void Execute()
    {
        Console.WriteLine("\n--- Створення нового гравця ---");

        string name = "";
        while (true)
        {
            Console.Write("Введіть ім'я нового гравця: ");
            string? nameInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(nameInput))
            {
                Console.WriteLine("Помилка: Ім'я гравця не може бути порожнім.");
            }
            else if (!nameInput.Any(c => !char.IsDigit(c)))
            {
                Console.WriteLine("Помилка: Ім'я гравця має містити хоча б один символ, відмінний від цифри (щоб не плутатися з ID).");
            }
            else
            {
                name = nameInput;
                break;
            }

            Console.Write("Натисніть Enter, щоб ввести ім'я заново, або введіть 0 для відміни додавання гравця: ");
            if (Console.ReadLine()?.Trim() == "0")
            {
                Console.WriteLine("Додавання гравця відмінено.");
                return;
            }
        }

        int startRating = 100;
        while (true)
        {
            Console.Write("Введіть початковий рейтинг (або натисніть Enter для стандартних 100 балів): ");
            string? ratingInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(ratingInput))
            {
                startRating = 100;
                Console.WriteLine("Обрано стандартний початковий рейтинг: 100 балів.");
                break;
            }

            if (int.TryParse(ratingInput, out int parsedRating) && parsedRating >= 1)
            {
                startRating = parsedRating;
                break;
            }

            Console.WriteLine("Помилка: Рейтинг має бути цілим числом не менше 1.");
            Console.Write("Натисніть Enter, щоб ввести рейтинг заново, або введіть 0 для відміни додавання гравця: ");
            if (Console.ReadLine()?.Trim() == "0")
            {
                Console.WriteLine("Додавання гравця відмінено.");
                return;
            }
        }

        GameAccount? newPlayer = null;
        while (newPlayer == null)
        {
            Console.WriteLine("Оберіть тип акаунту:");
            Console.WriteLine("  1 - StandardGameAccount (стандартне нарахування та зняття балів)");
            Console.WriteLine("  2 - HalfLossGameAccount (при поразці знімається вдвоє менше балів)");
            Console.WriteLine("  3 - WinStreakGameAccount (додаткові +5 балів за серію перемог)");
            Console.Write("Ваш вибір (1-3): ");
            string? typeChoice = Console.ReadLine()?.Trim();

            switch (typeChoice)
            {
                case "1":
                    newPlayer = new StandardGameAccount(name, startRating);
                    break;
                case "2":
                    newPlayer = new HalfLossGameAccount(name, startRating);
                    break;
                case "3":
                    newPlayer = new WinStreakGameAccount(name, startRating);
                    break;
                default:
                    Console.WriteLine("Помилка: Невірний вибір типу акаунту (потрібно ввести 1, 2 або 3).");
                    Console.Write("Натисніть Enter, щоб обрати тип заново, або введіть 0 для відміни додавання гравця: ");
                    if (Console.ReadLine()?.Trim() == "0")
                    {
                        Console.WriteLine("Додавання гравця відмінено.");
                        return;
                    }
                    break;
            }
        }

        accountService.CreateAccount(newPlayer);
        Console.WriteLine($"Успішно створено та додано в БД: {newPlayer}");
    }
}

// Команда для виведення статистики та списку ігор конкретного гравця.
public class ShowPlayerStatsCommand : ICommand
{
    private readonly IGameAccountService accountService;

    public ShowPlayerStatsCommand(IGameAccountService accountService)
    {
        this.accountService = accountService;
    }

    public void ShowInfo()
    {
        Console.WriteLine("Вивести статистику та ігри конкретного гравця (пошук за Ім'ям або ID)");
    }

    public void Execute()
    {
        Console.WriteLine("\n--- Статистика конкретного гравця ---");
        Console.Write("Введіть ID або ім'я гравця: ");
        string? input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Помилка: Ви нічого не ввели.");
            return;
        }

        GameAccount? player = FindPlayerByIdOrName(input);

        if (player == null)
        {
            Console.WriteLine($"Гравця з ID або ім'ям '{input}' не знайдено в базі даних.");
            return;
        }

        Console.WriteLine();
        accountService.ShowPlayerStats(player.Id);
    }

    private GameAccount? FindPlayerByIdOrName(string input)
    {
        if (int.TryParse(input, out int id))
        {
            return accountService.ReadAccountById(id);
        }

        return accountService.ReadAccounts()
            .FirstOrDefault(a => a.UserName.Equals(input, StringComparison.OrdinalIgnoreCase));
    }
}

// Команда для проведення гри між гравцями.
public class PlayGameCommand : ICommand
{
    private readonly IGameAccountService accountService;
    private readonly IGameService gameService;

    public PlayGameCommand(IGameAccountService accountService, IGameService gameService)
    {
        this.accountService = accountService;
        this.gameService = gameService;
    }

    public void ShowInfo()
    {
        Console.WriteLine("Зіграти в гру (вибір гравців, типу гри та рейтингу)");
    }

    public void Execute()
    {
        Console.WriteLine("\n--- Проведення нової гри ---");

        List<GameAccount> allPlayers = accountService.ReadAccounts();
        if (allPlayers.Count < 2)
        {
            Console.WriteLine("Для проведення гри в базі має бути щонайменше 2 гравці.");
            return;
        }

        Console.WriteLine("Доступні гравці в базі:");
        foreach (var p in allPlayers)
        {
            Console.WriteLine($"  {p}");
        }

        Console.Write("\nВведіть ID або ім'я першого гравця, який виграв партію: ");
        string? input1 = Console.ReadLine()?.Trim();
        GameAccount? winner = FindPlayer(input1);
        if (winner == null)
        {
            Console.WriteLine("Першого гравця не знайдено в базі.");
            return;
        }

        Console.Write("Введіть ID або ім'я другого гравця, який програв партію: ");
        string? input2 = Console.ReadLine()?.Trim();
        GameAccount? loser = FindPlayer(input2);
        if (loser == null)
        {
            Console.WriteLine("Другого гравця не знайдено в базі.");
            return;
        }

        if (winner.Id == loser.Id)
        {
            Console.WriteLine("Помилка: Гравець не може грати сам проти себе!");
            return;
        }

        Console.WriteLine("\nОберіть тип гри:");
        Console.WriteLine("  1 - StandardGame (стандартна гра на рейтинг)");
        Console.WriteLine("  2 - TrainingGame (тренувальна гра без рейтингу, рейтинг = 0)");
        Console.WriteLine("  3 - SingleRatingGame (на рейтинг грає лише один із гравців)");
        Console.Write("Ваш вибір (1-3): ");
        string? gameType = Console.ReadLine()?.Trim();

        int rating = 0;
        if (gameType != "2")
        {
            Console.Write("Введіть рейтинг, на який грають (наприклад, 10): ");
            string? ratingStr = Console.ReadLine()?.Trim();
            if (!int.TryParse(ratingStr, out rating) || rating < 0)
            {
                Console.WriteLine("Помилка: Рейтинг має бути невід'ємним цілим числом.");
                return;
            }
        }

        Game gameForWinner;
        Game gameForLoser;

        switch (gameType)
        {
            case "1":
                gameForWinner = GameFactory.CreateStandardGame(rating);
                gameForLoser = GameFactory.CreateStandardGame(rating);
                break;
            case "2":
                gameForWinner = GameFactory.CreateTrainingGame();
                gameForLoser = GameFactory.CreateTrainingGame();
                break;
            case "3":
                Console.Write($"Чи грає на рейтинг переможець {winner.UserName}? (1 - Так, 0 - Ні): ");
                bool winnerRated = Console.ReadLine()?.Trim() == "1";
                gameForWinner = GameFactory.CreateSingleRatingGame(rating, winnerRated);
                // Якщо перший грає на рейтинг, то другий не на рейтинг (і навпаки)
                gameForLoser = GameFactory.CreateSingleRatingGame(rating, !winnerRated);
                break;
            default:
                Console.WriteLine("Невідомий тип гри, обрано StandardGame.");
                gameForWinner = GameFactory.CreateStandardGame(rating);
                gameForLoser = GameFactory.CreateStandardGame(rating);
                break;
        }

        try
        {
            // Спочатку проводимо поразку через сервіс (перевірить, чи не впаде рейтинг нижче 1)
            gameService.LoseGame(loser.Id, winner.UserName, gameForLoser);
            // Проводимо перемогу через сервіс (сервіс сам зберігає ігри та оновлює гравців у БД)
            gameService.WinGame(winner.Id, loser.UserName, gameForWinner);

            Console.WriteLine("\nГру успішно проведено та збережено в базі даних!");
            Console.WriteLine($"  Переможець: {accountService.ReadAccountById(winner.Id)}");
            Console.WriteLine($"  Програвший: {accountService.ReadAccountById(loser.Id)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка під час проведення гри: {ex.Message}");
        }
    }

    private GameAccount? FindPlayer(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (int.TryParse(input, out int id))
            return accountService.ReadAccountById(id);

        return accountService.ReadAccounts()
            .FirstOrDefault(a => a.UserName.Equals(input, StringComparison.OrdinalIgnoreCase));
    }
}

// Команда для виведення всіх зіграних ігор із бази даних.
public class ShowAllGamesCommand : ICommand
{
    private readonly IGameService gameService;

    public ShowAllGamesCommand(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public void ShowInfo()
    {
        Console.WriteLine("Вивести список всіх зіграних ігор у базі даних");
    }

    public void Execute()
    {
        Console.WriteLine("\n--- Список всіх ігор у базі даних ---");
        List<Game> allGames = gameService.ReadGames();

        if (allGames.Count == 0)
        {
            Console.WriteLine("У базі даних ще немає жодної зіграної гри.");
            return;
        }

        foreach (Game game in allGames)
        {
            Console.WriteLine($"  {game}");
        }
        Console.WriteLine($"Всього записів ігор у БД: {allGames.Count}");
    }
}

