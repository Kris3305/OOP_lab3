namespace GameAccountLab;

/// Інтерфейс репозиторію гравців (рівень Repository). Містить базові функції CRUD.
public interface IGameAccountRepository
{
    void Create(GameAccount account);       // Create — додати гравця в БД
    List<GameAccount> ReadAll();            // Read — отримати всіх гравців
    GameAccount? ReadById(int id);          // Read — отримати гравця за його Id
    void Update(GameAccount account);       // Update — оновити дані гравця
    void Delete(int id);                    // Delete — видалити гравця за Id
}

/// Інтерфейс репозиторію ігор (рівень Repository). Містить базові функції CRUD.
public interface IGameRepository
{
    void Create(Game game);                 // Create — додати гру в БД
    List<Game> ReadAll();                   // Read — отримати всі ігри
    Game? ReadById(int id);                 // Read — отримати гру за її GameId
    void Update(Game game);                 // Update — оновити дані гри
    void Delete(int id);                    // Delete — видалити гру за GameId
}

/// Реалізація репозиторію гравців. Приймає DbContext для доступу до списку гравців у БД.
public class GameAccountRepository : IGameAccountRepository
{
    private readonly DbContext context; // Посилання на базу даних

    public GameAccountRepository(DbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Create(GameAccount account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        context.Accounts.Add(account);
    }

    public List<GameAccount> ReadAll()
    {
        return context.Accounts.ToList();
    }

    public GameAccount? ReadById(int id)
    {
        // FirstOrDefault шукає першого гравця з таким Id (або повертає null, якщо не знайдено)
        return context.Accounts.FirstOrDefault(a => a.Id == id);
    }

    public void Update(GameAccount account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        int index = context.Accounts.FindIndex(a => a.Id == account.Id);
        if (index != -1)
        {
            context.Accounts[index] = account;
        }
    }

    public void Delete(int id)
    {
        var account = ReadById(id);
        if (account != null)
        {
            context.Accounts.Remove(account);
        }
    }
}

/// Реалізація репозиторію ігор. Приймає DbContext для доступу до списку ігор у БД.
public class GameRepository : IGameRepository
{
    private readonly DbContext context; // Посилання на базу даних

    public GameRepository(DbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Create(Game game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        context.Games.Add(game);
    }

    public List<Game> ReadAll()
    {
        return context.Games.ToList();
    }

    public Game? ReadById(int id)
    {
        return context.Games.FirstOrDefault(g => g.GameId == id);
    }

    public void Update(Game game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        int index = context.Games.FindIndex(g => g.GameId == game.GameId);
        if (index != -1)
        {
            context.Games[index] = game;
        }
    }

    public void Delete(int id)
    {
        var game = ReadById(id);
        if (game != null)
        {
            context.Games.Remove(game);
        }
    }
}

