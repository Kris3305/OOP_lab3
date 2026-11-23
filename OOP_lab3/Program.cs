using System.Text;
using GameAccountLab;

// UTF-8 для коректного виводу українських літер
Console.OutputEncoding = Encoding.UTF8;

// 1. Створюємо імітацію бази даних (DbContext вже містить початкових гравців Аллу та Богдана і їхню першу гру)
DbContext dbContext = new DbContext();

// 2. Створюємо об'єкти рівня Repository (передаємо їм DbContext) і приводимо до інтерфейсів
IGameAccountRepository accountRepository = new GameAccountRepository(dbContext);
IGameRepository gameRepository = new GameRepository(dbContext);

// 3. Створюємо об'єкти рівня Service (передаємо їм інтерфейси репозиторіїв)
IGameAccountService accountService = new GameAccountService(accountRepository, gameRepository);
IGameService gameService = new GameService(gameRepository, accountRepository);

// 4. [CREATE ACCOUNT] Створення нового гравця через сервіс (CreateAccount)
Console.WriteLine("=== 1. Створення гравця та робота з базою даних (CRUD) ===");
GameAccount newPlayer = new WinStreakGameAccount("Вікторія", 100);
accountService.CreateAccount(newPlayer);
Console.WriteLine($"[CreateAccount] Додано гравця в БД -> {newPlayer}\n");

// 5. [READ ACCOUNTS] Вивід списку всіх створених гравців із бази даних (ReadAccounts)
Console.WriteLine("=== 2. Список всіх гравців у базі даних (ReadAccounts) ===");
List<GameAccount> allPlayers = accountService.ReadAccounts();
foreach (var p in allPlayers)
{
    Console.WriteLine($"  {p}");
}
Console.WriteLine();

// 6. [READ ACCOUNT BY ID] Пошук гравця в базі даних за його Id (ReadAccountById)
GameAccount? player3 = accountService.ReadAccountById(3);
Console.WriteLine($"[ReadAccountById] Знайдено гравця -> {player3}\n");

// 7. [CREATE GAME / PLAY GAME] Проведення ігор різних типів через рівень Service (GameService)
Console.WriteLine("=== 3. Проведення ігор різних типів через Service із записом у БД ===");

// --- Ігри для Алли (Id = 1) ---
gameService.WinGame(1, "Вікторія", GameFactory.CreateTrainingGame());             // Тренувальна гра (+0)
gameService.LoseGame(1, "Богдан", GameFactory.CreateStandardGame(8));             // Стандартна гра (-8)
gameService.WinGame(1, "Вікторія", GameFactory.CreateSingleRatingGame(12, true)); // Гра з одним рейтингом (+12)

// --- Ігри для Богдана (Id = 2) ---
gameService.WinGame(2, "Вікторія", GameFactory.CreateStandardGame(20));            // Стандартна гра (+20)
gameService.WinGame(2, "Алла", GameFactory.CreateTrainingGame());                  // Тренувальна гра (+0)
gameService.LoseGame(2, "Вікторія", GameFactory.CreateSingleRatingGame(16, true)); // Гра з одним рейтингом (зніме -8)

// --- Ігри для Вікторії (Id = 3) ---
gameService.WinGame(3, "Алла", GameFactory.CreateStandardGame(10));               // 1-ша перемога (+10)
gameService.WinGame(3, "Богдан", GameFactory.CreateStandardGame(10));             // 2-га перемога поспіль (+10 та +5 бонус = +15)
gameService.WinGame(3, "Алла", GameFactory.CreateSingleRatingGame(12, false));    // Грає не на рейтинг (+0)
gameService.LoseGame(3, "Богдан", GameFactory.CreateStandardGame(8));             // Поразка (-8, серія скидається)

// Додаткова наочна гра на 13 балів через сервіс
gameService.WinGame(3, "Богдан", GameFactory.CreateStandardGame(13));
Console.WriteLine($"[CreateGame] Вікторія зіграла у гру на 13 балів проти Богдана і перемогла. Тепер ігор у БД: {gameService.ReadGames().Count}");

// 8. [READ GAME BY ID] Пошук конкретної гри в базі даних за її Id
Game? foundGame = gameService.ReadGameById(13);
Console.WriteLine($"[ReadGameById] Знайдено гру -> {foundGame}");

// 9. [UPDATE] Оновлення даних гравця та його ігор через рівень Service
if (player3 != null)
{
    player3.UserName = "Вікторія_Pro";
    accountService.UpdateAccount(player3);
    Console.WriteLine($"[UpdateAccount] Оновлено дані гравця в БД -> {accountService.ReadAccountById(3)}");
    Console.WriteLine($"[UpdateGame] Автоматично оновлено запис гри в БД -> {gameService.ReadGameById(13)}");
}

// 10. [READ PLAYER GAMES BY PLAYER ID] Вивід списку ігор та статистики кожного конкретного гравця через Service
Console.WriteLine("\n=== 4. Статистика та список ігор кожного гравця з БД (ShowPlayerStats) ===\n");
accountService.ShowPlayerStats(1);
Console.WriteLine();
accountService.ShowPlayerStats(2);
Console.WriteLine();
accountService.ShowPlayerStats(3);

// 11. [READ GAMES] Вивід всіх ігор, які зберігаються в базі даних (ReadGames)
Console.WriteLine("\n=== 5. Всі ігри, записані в базі даних (ReadGames) ===");
List<Game> allGames = gameService.ReadGames();
foreach (var g in allGames)
{
    Console.WriteLine($"  {g}");
}
Console.WriteLine($"Всього ігор у БД: {allGames.Count}");
Console.WriteLine(new string('-', 40));

// 12. [DELETE GAME & DELETE ACCOUNT] Демонстрація видалення гри та гравця з бази даних
Console.WriteLine("\n=== 6. Видалення гри та гравця з бази даних (DeleteGame, DeleteAccount) ===");
gameService.DeleteGame(13);
Console.WriteLine($"[DeleteGame] Гру Id=13 видалено з БД. Залишилось ігор у БД: {gameService.ReadGames().Count}");

accountService.DeleteAccount(3);
Console.WriteLine($"[DeleteAccount] Гравця Id=3 видалено з БД.");
Console.WriteLine("Список гравців у БД після видалення:");
foreach (var p in accountService.ReadAccounts())
{
    Console.WriteLine($"  {p}");
}
Console.WriteLine(new string('-', 40));

// 13. Демонстрація перевірок на помилки через рівень Service
Console.WriteLine("\nДемонстрація помилки: від'ємний рейтинг");
try
{
    gameService.WinGame(1, "Хтось", GameFactory.CreateStandardGame(-5));
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine("\nДемонстрація помилки: рейтинг < 1");
try
{
    GameAccount weakPlayer = new StandardGameAccount("Слабкий", 5);
    accountService.CreateAccount(weakPlayer);
    gameService.LoseGame(weakPlayer.Id, "Сильний", GameFactory.CreateStandardGame(100));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}
