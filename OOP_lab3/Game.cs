namespace GameAccountLab;
public enum GameResult // Результат зіграної партії
{
    Win,
    Lose
}

/// Базовий абстрактний клас, який описує одну зіграну партію (запис гри в базі даних).
public abstract class Game
{
    private static int gameSeed = 0; // Спільний лічильник для всіх ігор — генерує унікальний GameId.
    public int GameId { get; } // Унікальний ідентифікатор гри в БД (генерується автоматично)
    public int PlayerId { get; set; } // Id гравця в БД, якому належить цей запис гри.
    public string PlayerName { get; set; } = ""; // Ім'я гравця.
    public string OpponentName { get; set; } = "";    // Ім'я опонента (заповнюється під час гри).
    public int Rating { get; }    // Базовий рейтинг гри.
    public GameResult Result { get; set; }    // Результат: перемога чи поразка.

    protected Game(int rating)
    {
        // Перевірка: рейтинг гри не може бути від'ємним
        if (rating < 0)
            throw new ArgumentException(
                "Рейтинг на який грають не може бути від'ємним", nameof(rating));

        // Автоматична генерація унікального ідентифікатора
        gameSeed++;
        GameId = gameSeed;

        // Ініціалізація базового рейтингу
        Rating = rating;
    }

    // Абстрактний метод для розрахунку рейтингу, на який буде гра.
    // Кожен клас-наслідник зобов'язаний надати власну реалізацію цього методу.
    public abstract int CalculateRating();

    public override string ToString() // Повертає рядок з інформацією про гру
    {
        string resultText = Result == GameResult.Win ? "Перемога" : "Поразка";
        return $"Id={GameId} [{GetType().Name}]: гравець {PlayerName} (PlayerId={PlayerId}) проти {OpponentName}, рейтинг {CalculateRating()}, {resultText}";
    }
}

/// Стандартна гра — грають на звичайний вказаний рейтинг.
public class StandardGame : Game // : - наслідування
{
    public StandardGame(int rating) : base(rating) // : base(rating) передає число rating у конструктор protected Game(int rating), де перевіряється і видається GameId.
    {
    }

    public override int CalculateRating()
    {
        return Rating; // Повертаємо базовий рейтинг без змін
    }
}

/// Тренувальна гра — гра без рейтингу (рейтинг завжди 0).
public class TrainingGame : Game
{
    public TrainingGame() : base(0)
    {
    }

    public override int CalculateRating()
    {
        return 0; // У тренувальній грі рейтинг не змінюється
    }
}

/// Гра, де на рейтинг грає лише один гравець.
public class SingleRatingGame : Game
{
    public bool IsRated { get; } // Чи грає поточний гравець на рейтинг

    public SingleRatingGame(int rating, bool isRated) : base(rating)
    {
        IsRated = isRated;
    }

    public override int CalculateRating()
    {
        // Якщо цей гравець грає на рейтинг — повертаємо Rating, інакше 0
        return IsRated ? Rating : 0;
    }
}