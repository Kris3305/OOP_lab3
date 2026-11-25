namespace GameAccountLab;

/// Фабрика для створення об'єктів різних типів ігор.
public class GameFactory
{
    // Створює стандартну гру та повертає її приведеною до базового типу Game
    public static Game CreateStandardGame(int rating)
    {
        StandardGame game = new StandardGame(rating);
        return (Game)game;
    }

    // Створює тренувальну гру без рейтингу та повертає як базовий тип Game
    public static Game CreateTrainingGame()
    {
        TrainingGame game = new TrainingGame();
        return (Game)game;
    }

    // Створює гру, де на рейтинг грає лише один гравець, та повертає як базовий тип Game
    public static Game CreateSingleRatingGame(int rating, bool isRated)
    {
        SingleRatingGame game = new SingleRatingGame(rating, isRated);
        return (Game)game;
    }
}

