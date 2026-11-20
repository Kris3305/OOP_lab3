namespace GameAccountLab;

/// Клас, який симулює базу даних: зберігає списки гравців та ігор і наповнює їх початковими даними.
public class DbContext
{
    public List<GameAccount> Accounts { get; } = new(); // Таблиця гравців у базі даних
    public List<Game> Games { get; } = new();           // Таблиця ігор у базі даних

    public DbContext()
    {
        SeedData(); // Наповнюємо списки початковими даними при старті бази даних
    }

    private void SeedData()
    {
        // Створюємо початкових гравців різних класів
        GameAccount player1 = new StandardGameAccount("Алла", 100);
        GameAccount player2 = new HalfLossGameAccount("Богдан", 100);

        Accounts.Add(player1);
        Accounts.Add(player2);

        // Початкова гра 1 (для Алли — перемога)
        Game startGame1 = GameFactory.CreateStandardGame(10);
        player1.WinGame("Богдан", startGame1);
        Games.Add(startGame1);

        // Початкова гра 2 (для Богдана — поразка у тій самій зустрічі)
        Game startGame2 = GameFactory.CreateStandardGame(10);
        player2.LoseGame("Алла", startGame2);
        Games.Add(startGame2);
    }
}

