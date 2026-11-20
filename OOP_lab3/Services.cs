namespace GameAccountLab;

/// Інтерфейс сервісу для роботи з гравцями (рівень Service).
public interface IGameAccountService
{
    void CreateAccount(GameAccount account);
    List<GameAccount> ReadAccounts();
    GameAccount? ReadAccountById(int id);
    void UpdateAccount(GameAccount account);
    void DeleteAccount(int id);
}

/// Інтерфейс сервісу для роботи з іграми (рівень Service).
public interface IGameService
{
    void CreateGame(Game game);
    List<Game> ReadGames();
    Game? ReadGameById(int id);
    List<Game> ReadPlayerGamesByPlayerId(int playerId);
    void UpdateGame(Game game);
    void DeleteGame(int id);
}

/// Сервіс гравців. Приймає об'єкт репозиторію, приведений до інтерфейсу IGameAccountRepository.
public class GameAccountService : IGameAccountService
{
    private readonly IGameAccountRepository accountRepository;

    public GameAccountService(IGameAccountRepository accountRepository)
    {
        this.accountRepository = accountRepository
            ?? throw new ArgumentNullException(nameof(accountRepository));
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
    }

    public void DeleteAccount(int id)
    {
        accountRepository.Delete(id);
    }
}

/// Сервіс ігор. Приймає об'єкт репозиторію, приведений до інтерфейсу IGameRepository.
public class GameService : IGameService
{
    private readonly IGameRepository gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        this.gameRepository = gameRepository
            ?? throw new ArgumentNullException(nameof(gameRepository));
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
}

