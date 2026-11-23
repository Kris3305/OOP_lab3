using System.Linq;   // для Count(g => ...)
namespace GameAccountLab; // Простір імен для класів GameAccount та Game

public class GameAccount // Описує гравця в базі даних: Id, ім'я, рейтинг та історію ігор.
{
    private static int accountSeed = 0; // Спільний лічильник для генерації унікального Id гравця в БД
    private int currentRating;    // Приховане поле рейтингу
    private readonly List<Game> history = new();    // Історія ігор 
    public int Id { get; }    // Унікальний ідентифікатор гравця в базі даних
    public string UserName { get; set; }    // Ім'я користувача.
    public int CurrentRating    // Рейтинг користувача (не може стати менше 1).
    {
        get => currentRating; //Вираз після => обчислюється і повертається як значення властивості CurrentRating.
        private set
        {
            if (value < 1)
                throw new InvalidOperationException($"Рейтинг не може стати менше 1 (спроба встановити {value})");
            currentRating = value;
        }
    }
    public int GamesCount => history.Count;
    // Історія ігор (тільки для читання).
    public IReadOnlyList<Game> History => history.AsReadOnly();

    public GameAccount(string userName, int startRating = 100)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException(
                "Ім'я користувача не може бути порожнім", nameof(userName));

        accountSeed++;
        Id = accountSeed; // Присвоюємо унікальний Id

        UserName = userName;
        CurrentRating = startRating;   // set перевірить >= 1
    }

    // Віртуальний метод для розрахунку балів при перемозі.
    // Класи-наслідники можуть перевизначити (override) його за допомогою ключового слова override.
    protected virtual int CalculateWinRating(int rating)
    {
        return rating;
    }

    // Віртуальний метод для розрахунку балів при поразці.
    protected virtual int CalculateLoseRating(int rating)
    {
        return rating;
    }

    // Викликається у випадку перемоги.
    public void WinGame(string opponentName, Game game)
    {
        if (string.IsNullOrWhiteSpace(opponentName))
            throw new ArgumentException(
                "Ім'я опонента не може бути порожнім", nameof(opponentName));

        if (game == null)
            throw new ArgumentNullException(nameof(game), "Гра не може бути null");

        int rating = game.CalculateRating(); // Визначаємо, на який рейтинг була гра
        if (rating < 0)
            throw new ArgumentException(
                "Рейтинг на який грають не може бути від'ємним", nameof(game));

        int points = CalculateWinRating(rating); // Розраховуємо бали з урахуванням типу акаунту

        game.PlayerId = Id;
        game.PlayerName = UserName;
        game.OpponentName = opponentName;
        game.Result = GameResult.Win;

        history.Add(game);
        CurrentRating += points; // Перемога додає розраховані бали
    }

    // Викликається у випадку поразки.
    public void LoseGame(string opponentName, Game game)
    {
        if (string.IsNullOrWhiteSpace(opponentName))
            throw new ArgumentException(
                "Ім'я опонента не може бути порожнім", nameof(opponentName));

        if (game == null)
            throw new ArgumentNullException(nameof(game), "Гра не може бути null");

        int rating = game.CalculateRating(); // Визначаємо, на який рейтинг була гра
        if (rating < 0)
            throw new ArgumentException(
                "Рейтинг на який грають не може бути від'ємним", nameof(game));

        int points = CalculateLoseRating(rating); // Розраховуємо, скільки балів зніметься

        // Перевірка: після поразки рейтинг не має впасти нижче 1
        if (CurrentRating - points < 1)
            throw new InvalidOperationException(
                $"Неможливо зіграти на рейтинг {rating} (зніметься {points}): " +
                $"поточний рейтинг {CurrentRating} став би менше 1");

        game.PlayerId = Id;
        game.PlayerName = UserName;
        game.OpponentName = opponentName;
        game.Result = GameResult.Lose;

        history.Add(game);
        CurrentRating -= points; // Поразка віднімає розраховані бали
    }

    // Показує статистику та історію ігор гравця (список ігор передається з рівня Service).
    public void GetStats(List<Game>? gamesFromDb = null)
    {
        List<Game> playerGames = gamesFromDb ?? history;

        Console.WriteLine($"Статистика гравця Id={Id}, {UserName} ({GetType().Name}):");
        Console.WriteLine($"Поточний рейтинг: {CurrentRating}");
        Console.WriteLine($"Кількість ігор у БД: {playerGames.Count}");
        Console.WriteLine();

        if (playerGames.Count == 0)
        {
            Console.WriteLine("Ігор ще не було.");
            return;
        }

        Console.WriteLine("Історія ігор гравця з БД:");
        for (int i = 0; i < playerGames.Count; i++)
        {
            Console.WriteLine($"  Гра #{i + 1}: {playerGames[i]}");
        }

        // Підрахунок перемог та поразок за допомогою LINQ
        int wins = playerGames.Count(g => g.Result == GameResult.Win);
        int losses = playerGames.Count(g => g.Result == GameResult.Lose);

        Console.WriteLine();
        Console.WriteLine($"Перемог: {wins}, Поразок: {losses}");
        Console.WriteLine(new string('-', 40));
    }

    public override string ToString()
    {
        return $"Id={Id}: {UserName} [{GetType().Name}], Поточний рейтинг: {CurrentRating}";
    }
}

/// Стандартний акаунт: отримує та втрачає бали у звичайному розмірі.
public class StandardGameAccount : GameAccount
{
    public StandardGameAccount(string userName, int startRating = 100)
        : base(userName, startRating)
    {
    }

    protected override int CalculateWinRating(int rating)
    {
        return rating; // Стандартне нарахування балів
    }

    protected override int CalculateLoseRating(int rating)
    {
        return rating; // Стандартне зняття балів
    }
}

/// Акаунт, у якого при програші знімається вдвоє менше балів.
public class HalfLossGameAccount : GameAccount
{
    public HalfLossGameAccount(string userName, int startRating = 100)
        : base(userName, startRating)
    {
    }

    // Перевизначаємо лише метод для поразки (при перемозі працює метод базового класу)
    protected override int CalculateLoseRating(int rating)
    {
        return rating / 2; // Знімаємо вдвоє менше балів
    }
}

/// Акаунт, у якому нараховуються додаткові бали за серію перемог.
public class WinStreakGameAccount : GameAccount
{
    private int winStreak = 0; // Лічильник поточної серії перемог

    public WinStreakGameAccount(string userName, int startRating = 100)
        : base(userName, startRating)
    {
    }

    protected override int CalculateWinRating(int rating)
    {
        winStreak++; // Збільшуємо серію перемог

        // Якщо це друга або більше перемога поспіль (серія) і гра рейтингова — додаємо бонус +5 балів
        if (winStreak >= 2 && rating > 0)
        {
            return rating + 5;
        }

        return rating;
    }

    protected override int CalculateLoseRating(int rating)
    {
        winStreak = 0; // Поразка перериває серію перемог
        return rating;
    }
}