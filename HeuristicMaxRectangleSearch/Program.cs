using System.Globalization;
using System.Text;

public class Program
{
    static float MaxSeconds = 0f;
    static DateTime StartTime = DateTime.Now;
    static Rectangle[] BiggestFoundRectangles;

    public static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var searchFields = ParseInput();

#if DEBUG
        Console.WriteLine($"Parsing took {(DateTime.Now - StartTime).TotalMilliseconds}ms");
#endif

        // Before we start using any heuristics, make a random guess with a small rectangle
        // This ensures that we will always have an 'answer' ready that is valid
        FindSingleRandomRectangle(searchFields);
        OutputResults();

        FindSolutionsUsingParticleAlgo(searchFields);
    }

    private static void FindSolutionsUsingParticleAlgo(ISearchField[] searchFields)
    {
        var searchParams = InitParticleSearchParams(searchFields);
        while (true)
        {
            foreach (var searchField in searchFields)
            {

            }

            OutputResults();
        }
    }

    private static SearchParams[][] InitParticleSearchParams(ISearchField[] searchFields)
    {
        var searchParams = new SearchParams[searchFields.Length][];

        for (var i = 0; i < searchFields.Length; i++)
        {
            var numberOfParticles = searchFields[i].NumberOfPoints / 3 + 1;

            searchParams[i] = new SearchParams[numberOfParticles];

            // Initialize all the particles with random values
            for (var j = 0; j < numberOfParticles; j++)
            {
                var xPos = Random.Shared.NextSingle();
                var yPos = Random.Shared.NextSingle();
                searchParams[i][j].RectangleCenterXPos = xPos;
                searchParams[i][j].RectangleCenterYPos = yPos;
                searchParams[i][j].RectangleWidth = MathF.Min(xPos, 1 - xPos);
                searchParams[i][j].RectangleHeight = MathF.Min(yPos, 1 - yPos);
            }
        }

        return searchParams;
    }

    public static void OutputResults()
    {
        var outputStr = new StringBuilder();
        for (var i = 0; i < BiggestFoundRectangles.Length; i++)
        {
            var width = BiggestFoundRectangles[i].B.X - BiggestFoundRectangles[i].A.X;
            var height = BiggestFoundRectangles[i].B.Y - BiggestFoundRectangles[i].A.Y;

            outputStr.Append(BiggestFoundRectangles[i].A.X);
            outputStr.Append(",");
            outputStr.Append(BiggestFoundRectangles[i].A.Y);

            outputStr.Append(" ");

            outputStr.Append(BiggestFoundRectangles[i].A.X + width);
            outputStr.Append(",");
            outputStr.Append(BiggestFoundRectangles[i].A.Y);

            outputStr.Append(" ");

            outputStr.Append(BiggestFoundRectangles[i].B.X);
            outputStr.Append(",");
            outputStr.Append(BiggestFoundRectangles[i].B.Y);

            outputStr.Append(BiggestFoundRectangles[i].B.X - width);
            outputStr.Append(",");
            outputStr.Append(BiggestFoundRectangles[i].B.Y);

            if (i != BiggestFoundRectangles.Length - 1)
            {
                outputStr.Append("\r\n");
            }
        }

        File.WriteAllText("points.out", outputStr.ToString());
    }

    private static void FindSingleRandomRectangle(ISearchField[] searchFields)
    {
        for (var i = 0; i < searchFields.Length; i++)
        {
            // Search with decreasing rectangle size if we could not place any rectangle 
            // with the dimensions
            var averageSpaceBetweenPoints = 1f / MathF.Sqrt(searchFields[i].NumberOfPoints);
            var rectLength = averageSpaceBetweenPoints * 1.5f;

            while (!FindSingleRandomRectanlge(searchFields[i], rectLength, rectLength, i))
            {
                rectLength *= 0.9f;
            }

#if DEBUG
            Console.WriteLine($"Found random rectangle for field {i}: {BiggestFoundRectangles[i].ToString()}");
            Console.WriteLine($"  --> With Area {BiggestFoundRectangles[i].Area}");
#endif
        }
    }

    private static bool FindSingleRandomRectanlge(ISearchField searchField, float width, float height, int searchFieldIndex)
    {
        var foundValidPlacement = false;
        var tries = 0;
        while (!foundValidPlacement && tries < 15)
        {
            var randX = (Random.Shared.NextSingle() * (1 - width)) - 0.5f;
            var randY = (Random.Shared.NextSingle() * (1 - height)) - 0.5f;
            var rect = new Rectangle
            {
                A = new Point(randX, randY),
                B = new Point(randX + width, randY + height)
            };

            if (!searchField.AnyPointsInRect(rect))
            {
                foundValidPlacement = true;
                BiggestFoundRectangles[searchFieldIndex] = rect;
            }

            tries++;
        }

        return foundValidPlacement;
    }

    private static ISearchField[] ParseInput()
    {
        var lines = File.ReadAllLines("points.in");

        MaxSeconds = float.Parse(lines[0]);
        SetupExitTimer();
        var numberOfFields = int.Parse(lines[1]);
        BiggestFoundRectangles = new Rectangle[numberOfFields];
        var searchFields = new SearchFieldBasic[numberOfFields];

        for (var i = 0; i < numberOfFields; i++)
        {
            var splitted = lines[i + 2].Split(' ');
            var numberOfPoints = int.Parse(splitted[0]);
            var points = new Point[numberOfPoints];
            var searchField = new SearchFieldBasic(points);
            searchFields[i] = searchField;

            for (var j = 0; j < numberOfPoints; j++)
            {
                var coordSplitted = splitted[j + 1].Split(',');
                var x = float.Parse(coordSplitted[0]);
                var y = float.Parse(coordSplitted[1]);

                searchField.Points[j] = new Point(x - 0.5f, y - 0.5f);
            }
        }

        return searchFields;
    }

    private static void SetupExitTimer()
    {
        var secondsEpsilonForExit = 0.05f;
        var timer = new Timer((obj) => Exit(), null, TimeSpan.FromSeconds(MaxSeconds - secondsEpsilonForExit), TimeSpan.FromSeconds(MaxSeconds - secondsEpsilonForExit));
    }

    private static void Exit()
    {
        Environment.Exit(0);
    }
}

public interface ISearchField
{
    int NumberOfPoints { get; }
    bool AnyPointsInRect(Rectangle rect);
}

public class SearchFieldBasic : ISearchField
{
    public Point[] Points { get; private set; }

    public int NumberOfPoints => Points.Length;

    public SearchFieldBasic(Point[] points)
    {
        this.Points = points;
    }

    public bool AnyPointsInRect(Rectangle rect)
    {
        return Points.Any(p => p.X > rect.A.X && p.X < rect.B.X && p.Y > rect.A.Y && p.Y < rect.B.Y);
    }
}

public record Point(float X, float Y);

/// <summary>
/// Will always be defined with A at the lower left corner and B at the top right corner
///                                   B
/// 		+------------------------+
///         |                        |
///         |                        |
///         |                        |
/// 		+------------------------+
/// 	   A
/// </summary>
public class Rectangle
{
    public Point A;
    public Point B;

    public float Area
    {
        get { return (B.X - A.X) * (B.Y - A.Y); }
    }

    public override string ToString()
    {
        return $"A: {A.X},{A.Y}  B: {B.X},{B.Y}";
    }
}

public class SearchParams
{
    public float RectangleCenterXPos { get; set; }
    public float RectangleCenterYPos { get; set; }
    //public float Angle;
    public float RectangleWidth { get; set; }
    public float RectangleHeight { get; set; }
}

public class Velocities : SearchParams { };
