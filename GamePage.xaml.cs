using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;

namespace ChristmasCodingChallange;

public partial class GamePage : ContentPage
{

    bool inProgress = false;
    readonly IDevice Controller;
    readonly List<Image> Obstacles = new List<Image>();
    Stopwatch obstacleTimer = new Stopwatch();


    private int score;

    public int Score
    {
        get { return score; }
        set
        {
            if (score != value)
            {
                score = value;
                OnPropertyChanged(nameof(Score));
            }
        }
    }

    private int highScore;

    public int HighScore
    {
        get { return highScore; }
        set
        {
            if (highScore != value)
            {
                highScore = value;
                OnPropertyChanged(nameof(HighScore));
            }
        }
    }

    public GamePage(IDevice device)
    {
        InitializeComponent();
        Score = 0;
        HighScore = 0;
        BindingContext = this;
        Controller = device;
        ButtonListener();
        AddObstacles();
    }

    private void GameLoop()
    {
        inProgress = true;
        RestartButton.IsVisible = false;
        Score = 0;
        ObstacleGrid.Clear();
        Obstacles.Clear();

        Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            // Add a new obstacle every three seconds
            if (!obstacleTimer.IsRunning || obstacleTimer.ElapsedMilliseconds >= 3000)
            {
                obstacleTimer.Restart();
                AddObstacles();
            }

            // Move character and obstacles at 60fps
            MoveSprites();

            // End the game if character is out of bounds
            bool characterInBounds = (Character.TranslationY < 155 && Character.TranslationY > -350);

            if (!characterInBounds)
            {
                EndGame();
                return false;
            }
            else
            {
                return true;
            }
        });
    }

    private void EndGame()
    {
        RestartButton.IsVisible = true;
        inProgress = false;
        Character.TranslationY = -150;
    }

    private void MoveSprites()
    {
        MoveObstacles();
        Character.TranslationY += 2;
    }

    private void MoveObstacles()
    {
        bool ScoreUpdated = false;
        List<Image> toDelete = new List<Image>();

        foreach (Image Obstacle in Obstacles)
        {
            // Move obstacles to the left
            Obstacle.TranslationX -= 5;

            // Remove obstacles frame scene if they have moved off screen
            if (Obstacle.TranslationX == -800)
            {
                toDelete.Add(Obstacle);
                ObstacleGrid.Children.Remove(Obstacle);
            }

            // Increase the score if the user has passed an obstacle
            bool CharacterPassedGoal = Character.TranslationX < Obstacle.TranslationX + 5 && Character.TranslationX > Obstacle.TranslationX - 5;

            if (CharacterPassedGoal && !ScoreUpdated)
            {
                Score += 1;
                HighScore = Math.Max(Score, HighScore);
                ScoreUpdated = true;
            }
        }

        Obstacles.RemoveAll(toDelete.Contains);
    }

    private void AddObstacles()
    {
        Image topObstacle = new Image
        {
            Source = "obstacle.png",
            MaximumHeightRequest = 200,
            VerticalOptions = LayoutOptions.Start,
            RotationX = 180,
            TranslationX = 750
        };

        Image bottomObstacle = new Image
        {
            Source = "obstacle.png",
            MaximumHeightRequest = 200,
            TranslationY = 85,
            TranslationX = 750
        };

        ObstacleGrid.Children.Add(topObstacle);
        ObstacleGrid.Children.Add(bottomObstacle);

        Obstacles.Add(topObstacle);
        Obstacles.Add(bottomObstacle);
    }

    private void ActionPress()
    {
        if (!inProgress)
        {
            GameLoop();
        }
        else
        {
            Character.TranslationY -= 50;
        }
    }

    private async void ButtonListener()
    {
        IService service = await Controller.GetServiceAsync(Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E"));
        ICharacteristic characteristic = await service.GetCharacteristicAsync(Guid.Parse("6e400003-b5a3-f393-e0a9-e50e24dcca9e"));
        await characteristic.StartUpdatesAsync();

        characteristic.ValueUpdated += (sender, args) =>
        {
            if (BitConverter.ToString(characteristic.Value) == "F3-15-F3-01-01")
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ActionPress();
                });
            }
        };
    }
}