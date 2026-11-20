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
IGameAccountService accountService = new GameAccountService(accountRepository);
IGameService gameService = new GameService(gameRepository);

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

// 6. [READ ACCOUNT BY ID] Отримуємо гравців із бази даних за їхніми Id (ReadAccountById)
GameAccount player1 = accountService.ReadAccountById(1)!; // Алла (StandardGameAccount)
GameAccount player2 = accountService.ReadAccountById(2)!; // Богдан (HalfLossGameAccount)
GameAccount player3 = accountService.ReadAccountById(3)!; // Вікторія (WinStreakGameAccount)
Console.WriteLine($"[ReadAccountById] Знайдено гравця -> {player3}\n");

// 7. [CREATE GAME] Імітація ігор різних типів (результати автоматично записуються в БД через gameService)
Console.WriteLine("=== 3. Проведення ігор різних типів із записом у БД (CreateGame) ===");

// --- Ігри для Алли ---
player1.WinGame("Вікторія", GameFactory.CreateTrainingGame(), gameService);             // Тренувальна гра (+0)
player1.LoseGame("Богдан", GameFactory.CreateStandardGame(8), gameService);             // Стандартна гра (-8)
player1.WinGame("Вікторія", GameFactory.CreateSingleRatingGame(12, true), gameService); // Гра з одним рейтингом (+12)

// --- Ігри для Богдана ---
player2.WinGame("Вікторія", GameFactory.CreateStandardGame(20), gameService);            // Стандартна гра (+20)
player2.WinGame("Алла", GameFactory.CreateTrainingGame(), gameService);                  // Тренувальна гра (+0)
player2.LoseGame("Вікторія", GameFactory.CreateSingleRatingGame(16, true), gameService); // Гра з одним рейтингом (зніме -8)

// --- Ігри для Вікторії ---
player3.WinGame("Алла", GameFactory.CreateStandardGame(10), gameService);               // 1-ша перемога (+10)
player3.WinGame("Богдан", GameFactory.CreateStandardGame(10), gameService);             // 2-га перемога поспіль (+10 та +5 бонус = +15)
player3.WinGame("Алла", GameFactory.CreateSingleRatingGame(12, false), gameService);    // Грає не на рейтинг (+0)
player3.LoseGame("Богдан", GameFactory.CreateStandardGame(8), gameService);             // Поразка (-8, серія скидається)

// Додаткова наочна гра на 13 балів для демонстрації ReadGameById, UpdateGame та DeleteGame
player3.WinGame("Богдан", GameFactory.CreateStandardGame(13), gameService);
Console.WriteLine($"[CreateGame] {player3.UserName} зіграла у гру на 13 балів проти Богдана і перемогла. Тепер ігор у БД: {gameService.ReadGames().Count}");

// 8. [READ GAME BY ID] Пошук конкретної гри в базі даних за її Id
Game? foundGame = gameService.ReadGameById(13);
Console.WriteLine($"[ReadGameById] Знайдено гру -> {foundGame}");

// 9. [UPDATE] Оновлення даних гравця та гри в базі даних
player3.UserName = "Вікторія_Pro";
accountService.UpdateAccount(player3);
Console.WriteLine($"[UpdateAccount] Оновлено дані гравця в БД -> {accountService.ReadAccountById(player3.Id)}");

if (foundGame != null)
{
    foundGame.PlayerName = player3.UserName;
    gameService.UpdateGame(foundGame);
    Console.WriteLine($"[UpdateGame] Оновлено запис гри в БД -> {gameService.ReadGameById(13)}");
}

// 10. [READ PLAYER GAMES BY PLAYER ID] Вивід списку ігор та статистики кожного конкретного гравця
Console.WriteLine("\n=== 4. Статистика та список ігор кожного гравця з БД (ReadPlayerGamesByPlayerId) ===\n");
player1.GetStats(gameService);
Console.WriteLine();
player2.GetStats(gameService);
Console.WriteLine();
player3.GetStats(gameService);

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

accountService.DeleteAccount(player3.Id);
Console.WriteLine($"[DeleteAccount] Гравця {player3.UserName} (Id={player3.Id}) видалено з БД.");
Console.WriteLine("Список гравців у БД після видалення:");
foreach (var p in accountService.ReadAccounts())
{
    Console.WriteLine($"  {p}");
}
Console.WriteLine(new string('-', 40));

// 14. Демонстрація перевірок на помилки
Console.WriteLine("\nДемонстрація помилки: від'ємний рейтинг");
try
{
    player1.WinGame("Хтось", GameFactory.CreateStandardGame(-5), gameService);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine("\nДемонстрація помилки: рейтинг < 1");
try
{
    GameAccount weakPlayer = new StandardGameAccount("Слабкий", 5);
    weakPlayer.LoseGame("Сильний", GameFactory.CreateStandardGame(100), gameService);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}
