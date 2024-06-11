using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SnakeGame
{
    class Program
    {
        // Directions
        enum Direction { Up, Down, Left, Right }

        // Snake and food positions
        static List<(int x, int y)> snake = new List<(int x, int y)>();
        static List<(int x, int y)> foods = new List<(int x, int y)>();

        // Game settings
        static int width = 40;
        static int height = 20;
        static Direction direction = Direction.Right;
        static bool gameOver = false;
        static bool isPaused = false;
        static Random random = new Random();

        // To keep track of previous snake tail for efficient redraw
        static (int x, int y) previousTail;

        // Score
        static int score = 0;
        static int highScore = 0;
        static string highScoreName = "No Name";

        // Game speed
        static int verticalSpeed = 125;
        static int horizontalSpeed = 75; // Increase this value to slow down horizontal speed

        //File Paths
        static string scoresFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "snake_scores.txt");

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            LoadHighScore();
            ShowMainMenu();
        }

        static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Snake Game!");
            Console.WriteLine("1. Start Game");
            Console.WriteLine("2. Instructions");
            Console.WriteLine("3. View Scores");
            Console.WriteLine("4. Exit");
            Console.WriteLine("\n \nSnakeTHB v1.2");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    StartGame();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    ShowInstructions();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    ShowScores();
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    Environment.Exit(0);
                    break;
            }
        }

        static void ShowInstructions()
        {
            Console.Clear();
            Console.WriteLine("Instructions:");
            Console.WriteLine("Use W, A, S, D or arrow keys to move the snake.");
            Console.WriteLine("Eat the food to grow and gain points.");
            Console.WriteLine("Avoid running into yourself.");
            Console.WriteLine("Press P or ESC to pause/resume the game.");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey(true);
            ShowMainMenu();
        }

        static void StartGame()
        {
            Console.Clear();
            InitializeGame();

            // Main game loop
            while (!gameOver)
            {
                // Input handling
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    ChangeDirection(key);
                }

                if (isPaused)
                {
                    Thread.Sleep(100);
                    continue;
                }

                // Game logic
                UpdateSnake();
                CheckCollisions();
                Draw();

                // Control the game speed based on direction
                if (direction == Direction.Up || direction == Direction.Down)
                {
                    Thread.Sleep(verticalSpeed);
                }
                else
                {
                    Thread.Sleep(horizontalSpeed);
                }
            }

            EndGame();
        }

        static void InitializeGame()
        {
            gameOver = false;
            isPaused = false;
            snake.Clear();
            direction = Direction.Right;
            score = 0;

            // Initialize the snake
            snake.Add((width / 2, height / 2));
            for (int i = 1; i < 5; i++)
            {
                snake.Add((width / 2 - i, height / 2));
            }

            // Place the initial foods
            PlaceFoods(3); // Change this number to how many food items you want to start with

            // Draw the initial game state
            DrawBorder();
            Draw();
        }

        static void ChangeDirection(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    if (direction != Direction.Down) direction = Direction.Up;
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    if (direction != Direction.Up) direction = Direction.Down;
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (direction != Direction.Right) direction = Direction.Left;
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (direction != Direction.Left) direction = Direction.Right;
                    break;
                case ConsoleKey.Escape:
                case ConsoleKey.P:
                    isPaused = !isPaused;
                    if (isPaused)
                    {
                        Console.SetCursorPosition(width / 2 - 3, height / 2);
                        Console.Write("PAUSED");
                    }
                    else
                    {
                        Console.SetCursorPosition(width / 2 - 3, height / 2);
                        Console.Write("      "); // Clear the PAUSED message
                    }
                    break;
            }
        }

        static void UpdateSnake()
        {
            // Calculate new head position
            var head = snake[0];
            (int x, int y) newHead = head;

            switch (direction)
            {
                case Direction.Up: newHead.y--; break;
                case Direction.Down: newHead.y++; break;
                case Direction.Left: newHead.x--; break;
                case Direction.Right: newHead.x++; break;
            }

            // Wrap around if the snake goes out of bounds
            newHead.x = (newHead.x + width) % width;
            newHead.y = (newHead.y + height) % height;

            // Insert new head and remove the tail
            snake.Insert(0, newHead);

            // Remember the current tail to erase it later
            previousTail = snake.Last();

            if (foods.Contains(newHead))
            {
                foods.Remove(newHead); // If the snake eats food, remove it from the list
                score += 5;  // Increment score
                PlaceFoods(1); // Place new food
            }
            else
            {
                snake.RemoveAt(snake.Count - 1); // If not, remove the last segment
            }
        }

        static void CheckCollisions()
        {
            var head = snake[0];

            // Check for collision with itself
            if (snake.Skip(1).Any(segment => segment == head))
            {
                gameOver = true;
            }
        }

        static void Draw()
        {
            // Draw the border and score
            DrawBorder();
            DrawScore();

            // Erase the previous tail
            Console.SetCursorPosition(previousTail.x + 1, previousTail.y + 1);
            Console.Write(" ");

            // Draw the new head
            var head = snake[0];
            Console.SetCursorPosition(head.x + 1, head.y + 1);
            Console.Write("O");

            // Draw all food positions
            foreach (var food in foods)
            {
                Console.SetCursorPosition(food.x + 1, food.y + 1);
                Console.Write("X");
            }
        }

        static void DrawBorder()
        {
            // Draw the horizontal border
            Console.SetCursorPosition(0, 0);
            Console.Write(new string('-', width + 2));
            for (int y = 1; y <= height; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write("|");
                Console.SetCursorPosition(width + 1, y);
                Console.Write("|");
            }
            Console.SetCursorPosition(0, height + 1);
            Console.Write(new string('-', width + 2));
        }

        static void DrawScore()
        {
            Console.SetCursorPosition(0, height + 2);
            Console.Write($"Score: {score}  High Score: {highScore} by {highScoreName}");
        }

        static void ShowScores()
        {
            Console.Clear();
            Console.WriteLine("High Scores:");
            if (File.Exists(scoresFilePath))
            {
                var scores = File.ReadAllLines(scoresFilePath)
                                 .Select(line => {
                                     var parts = line.Split(',');
                                     return new { Name = parts[0], Score = int.Parse(parts[1]) };
                                 })
                                 .OrderByDescending(score => score.Score)
                                 .ToList();

                foreach (var score in scores)
                {
                    Console.WriteLine($"{score.Name}: {score.Score}");
                }
            }
            else
            {
                Console.WriteLine("No scores available.");
            }
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey(true);
            ShowMainMenu();
        }


        static void SaveScore(string name, int score)
        {
            try
            {
                File.AppendAllText(scoresFilePath, $"{name},{score}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save score: {ex.Message}");
            }
        }

        static void PlaceFoods(int numberOfFoods)
        {
            for (int i = 0; i < numberOfFoods; i++)
            {
                int x, y;
                do
                {
                    x = random.Next(0, width);
                    y = random.Next(0, height);
                } while (snake.Contains((x, y)) || foods.Contains((x, y)));

                foods.Add((x, y));
            }
        }

        static void EndGame()
        {
            Console.Clear();
            Console.WriteLine("Game Over!");
            Console.WriteLine($"Your score: {score}");
            if (score > highScore)
            {
                Console.WriteLine("New High Score!");
                Console.Write("Enter your name: ");
                highScoreName = Console.ReadLine();
                highScore = score;
                SaveHighScore();
            }
            else
            {
                Console.Write("Enter your name: ");
                var name = Console.ReadLine();
                SaveScore(name, score);
            }
            Console.WriteLine("Press Enter to return to the main menu...");

            // Wait for the Enter key to be pressed
            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                // Do nothing, just wait
            }

            ShowMainMenu();
        }

        static void SaveHighScore()
        {
            try
            {
                File.WriteAllText(scoresFilePath, $"{highScoreName},{highScore}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save high score: {ex.Message}");
            }
        }

        static void LoadHighScore()
        {
            try
            {
                if (File.Exists("highscore.txt"))
                {
                    var data = File.ReadAllText("highscore.txt").Split(',');
                    highScoreName = data[0];
                    highScore = int.Parse(data[1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load high score: {ex.Message}");
                highScore = 0;
                highScoreName = "No Name";
            }
        }
    }
}