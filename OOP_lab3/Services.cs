namespace GameAccountLab;

/// Інтерфейс сервісу для роботи з гравцями (рівень Service).
public interface IGameAccountService
{
    void CreateAccount(GameAccount account);
    List<GameAccount> ReadAccounts();
    GameAccount? ReadAccountById(int id);
    void UpdateAccount(GameAccount account);
    void DeleteAccount(int id);
    void ShowPlayerStats(int playerId);
}

/// Інтерфейс сервісу для роботи з іграми та взаємодії гравців з іграми (рівень Service).
public interface IGameService
{
    void CreateGame(Game game);
    List<Game> ReadGames();
    Game? ReadGameById(int id);
    List<Game> ReadPlayerGamesByPlayerId(int playerId);
    void UpdateGame(Game game);
    void DeleteGame(int id);
    void WinGame(int playerId, string opponentName, Game game);
    void LoseGame(int playerId, string opponentName, Game game);
}

/// Сервіс гравців. Приймає об'єкти репозиторіїв, приведені до інтерфейсів рівня Repository.
public class GameAccountService : IGameAccountService
{
    private readonly IGameAccountRepository accountRepository;
    private readonly IGameRepository gameRepository;

    public GameAccountService(IGameAccountRepository accountRepository, IGameRepository gameRepository)
    {
        this.accountRepository = accountRepository
            ?? throw new ArgumentNullException(nameof(accountRepository));
        this.gameRepository = gameRepository
            ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public void CreateAccount(GameAccount account)
    {
        accountRepository.Create(account);
    }

    public List<GameAccount> ReadAccounts()
    {
        return accountRepository.ReadAll();
    }

    public GameAccount? ReadAccountById(int id)
    {
        return accountRepository.ReadById(id);
    }

    public void UpdateAccount(GameAccount account)
    {
        accountRepository.Update(account);

        // Оновлюємо ім'я гравця також у всіх записах його ігор у БД
        var playerGames = gameRepository.ReadAll().Where(g => g.PlayerId == account.Id).ToList();
        foreach (var game in playerGames)
        {
            game.PlayerName = account.UserName;
            gameRepository.Update(game);
        }
    }

    public void DeleteAccount(int id)
    {
        accountRepository.Delete(id);
    }

    // Отримує гравця та список його ігор із бази даних і виводить статистику
    public void ShowPlayerStats(int playerId)
    {
        GameAccount? player = accountRepository.ReadById(playerId);
        if (player == null)
            throw new ArgumentException($"Гравця з Id={playerId} не знайдено в базі даних.");

        List<Game> playerGames = gameRepository.ReadAll()
            .Where(g => g.PlayerId == playerId)
            .ToList();

        player.GetStats(playerGames);
    }
}

/// Сервіс ігор. Відповідає за взаємодію між гравцями та іграми і збереження результатів у БД.
public class GameService : IGameService
{
    private readonly IGameRepository gameRepository;
    private readonly IGameAccountRepository accountRepository;

    public GameService(IGameRepository gameRepository, IGameAccountRepository accountRepository)
    {
        this.gameRepository = gameRepository
            ?? throw new ArgumentNullException(nameof(gameRepository));
        this.accountRepository = accountRepository
            ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public void CreateGame(Game game)
    {
        gameRepository.Create(game);
    }

    public List<Game> ReadGames()
    {
        return gameRepository.ReadAll();
    }

    public Game? ReadGameById(int id)
    {
        return gameRepository.ReadById(id);
    }

    // Повертає список ігор конкретного гравця за його Id у базі даних
    public List<Game> ReadPlayerGamesByPlayerId(int playerId)
    {
        return gameRepository.ReadAll()
            .Where(g => g.PlayerId == playerId)
            .ToList();
    }

    public void UpdateGame(Game game)
    {
        gameRepository.Update(game);
    }

    public void DeleteGame(int id)
    {
        gameRepository.Delete(id);
    }

    // Проводить переможну гру для гравця за його Id, зберігає гру в БД та оновлює гравця в БД
    public void WinGame(int playerId, string opponentName, Game game)
    {
        GameAccount? player = accountRepository.ReadById(playerId);
        if (player == null)
            throw new ArgumentException($"Гравця з Id={playerId} не знайдено в базі даних.");

        player.WinGame(opponentName, game);
        gameRepository.Create(game);
        accountRepository.Update(player);
    }

    // Проводить програшну гру для гравця за його Id, зберігає гру в БД та оновлює гравця в БД
    public void LoseGame(int playerId, string opponentName, Game game)
    {
        GameAccount? player = accountRepository.ReadById(playerId);
        if (player == null)
            throw new ArgumentException($"Гравця з Id={playerId} не знайдено в базі даних.");

        player.LoseGame(opponentName, game);
        gameRepository.Create(game);
        accountRepository.Update(player);
    }
}
