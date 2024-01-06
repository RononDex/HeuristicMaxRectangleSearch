using System.Globalization;
using System.Text;

public class Program
{
    static float MaxSeconds = 0f;
    static DateTime StartTime = DateTime.Now;
    static Rectangle[] BiggestFoundRectangles;
    static ParticleSearchParams[] GlobalBestParams;
    static float[] GlobalBestFitness;

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
        var searchParticles = InitParticleSearchParams(searchFields);
        while (true)
        {
            var foundNewSolution = false;
            for (var i = 0; i < searchFields.Length; i++)
            {
                var searchField = searchFields[i];
                foreach (var particle in searchParticles[i])
                {
                    var rectangle = particle.AsRectangle();
                    var fitness = 0f;
                    if (!searchField.AnyPointsInRect(rectangle))
                    {
                        fitness = rectangle.Area;

                        if (fitness > particle.BestFitness)
                        {
                            particle.BestFitness = fitness;
                            particle.BestParamsSoFar.RectangleCenterYPos = particle.CurSearchParams.RectangleCenterYPos;
                            particle.BestParamsSoFar.RectangleCenterXPos = particle.CurSearchParams.RectangleCenterXPos;
                            particle.BestParamsSoFar.RectangleHeight = particle.CurSearchParams.RectangleHeight;
                            particle.BestParamsSoFar.RectangleWidth = particle.CurSearchParams.RectangleWidth;

                            if (fitness > GlobalBestFitness[i])
                            {
#if DEBUG
                                Console.WriteLine($"Found new best solution for Field {i} with fitness / area of {fitness}");
#endif
                                GlobalBestFitness[i] = fitness;
                                GlobalBestParams[i].RectangleCenterYPos = particle.CurSearchParams.RectangleCenterYPos;
                                GlobalBestParams[i].RectangleCenterXPos = particle.CurSearchParams.RectangleCenterXPos;
                                GlobalBestParams[i].RectangleHeight = particle.CurSearchParams.RectangleHeight;
                                GlobalBestParams[i].RectangleWidth = particle.CurSearchParams.RectangleWidth;

                                foundNewSolution = true;
                            }
                        }
                    }
                }

                Step(searchField, searchParticles[i]);
            }

            if (foundNewSolution)
            {
                OutputResults();
            }
        }
    }

    private static void Step(ISearchField searchField, SearchParticle[] searchParticles)
    {
        foreach (var particle in searchParticles)
        {
            particle.CurSearchParams.RectangleCenterXPos += particle.Velocities.RectangleCenterXPos;
            particle.CurSearchParams.RectangleCenterYPos += particle.Velocities.RectangleCenterYPos;
            particle.CurSearchParams.RectangleWidth += particle.Velocities.RectangleWidth;
            particle.CurSearchParams.RectangleHeight += particle.Velocities.RectangleHeight;

            particle.CurSearchParams.RectangleCenterXPos = particle.CurSearchParams.RectangleCenterXPos % 1;
            particle.CurSearchParams.RectangleCenterYPos = particle.CurSearchParams.RectangleCenterYPos % 1;
            particle.CurSearchParams.RectangleWidth = particle.CurSearchParams.RectangleWidth % 1;
            particle.CurSearchParams.RectangleHeight = particle.CurSearchParams.RectangleHeight % 1;
        }
    }

    private static SearchParticle[][] InitParticleSearchParams(ISearchField[] searchFields)
    {
        var searchParams = new SearchParticle[searchFields.Length][];
        GlobalBestParams = new ParticleSearchParams[searchFields.Length];
        GlobalBestFitness = new float[searchFields.Length];

        for (var i = 0; i < searchFields.Length; i++)
        {
            var numberOfParticles = searchFields[i].NumberOfPoints / 3 + 1;

            searchParams[i] = new SearchParticle[numberOfParticles];
            GlobalBestFitness[i] = 0.0f;
            GlobalBestParams[i] = new ParticleSearchParams();

            // Initialize all the particles with random values
            for (var j = 0; j < numberOfParticles; j++)
            {
                searchParams[i][j] = new SearchParticle();

                var xPos = Random.Shared.NextSingle();
                var yPos = Random.Shared.NextSingle();
                var width = Random.Shared.NextSingle();
                var height = Random.Shared.NextSingle();
                searchParams[i][j].CurSearchParams.RectangleCenterXPos = xPos;
                searchParams[i][j].CurSearchParams.RectangleCenterYPos = yPos;
                searchParams[i][j].CurSearchParams.RectangleWidth = MathF.Min(width, 1 - xPos);
                searchParams[i][j].CurSearchParams.RectangleHeight = MathF.Min(height, 1 - yPos);

                searchParams[i][j].Velocities.RectangleHeight = (Random.Shared.NextSingle() - 0.5f) * 0.01f;
                searchParams[i][j].Velocities.RectangleWidth = (Random.Shared.NextSingle() - 0.5f) * 0.01f;
                searchParams[i][j].Velocities.RectangleCenterYPos = (Random.Shared.NextSingle() - 0.5f) * 0.01f;
                searchParams[i][j].Velocities.RectangleCenterXPos = (Random.Shared.NextSingle() - 0.5f) * 0.01f;

                searchParams[i][j].Parameters.Z1 = Random.Shared.NextSingle();
                searchParams[i][j].Parameters.Z2 = Random.Shared.NextSingle();

                searchParams[i][j].Parameters.MemoryCoefficient = 0.3f;
                searchParams[i][j].Parameters.SocialCoefficient = 0.5f;
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

public class SearchParticle
{
    public ParticleSearchParams CurSearchParams { get; } = new ParticleSearchParams();
    public ParticleSearchParams BestParamsSoFar { get; } = new ParticleSearchParams();
    public float BestFitness { get; set; } = 0.0f;

    public Velocities Velocities { get; } = new Velocities();
    public ParticleParamters Parameters { get; } = new ParticleParamters();

    public Rectangle AsRectangle()
    {
        return new Rectangle
        {
            A = new Point(CurSearchParams.RectangleCenterXPos - CurSearchParams.RectangleWidth / 2, CurSearchParams.RectangleCenterYPos - CurSearchParams.RectangleHeight / 2),
            B = new Point(CurSearchParams.RectangleCenterXPos + CurSearchParams.RectangleWidth / 2, CurSearchParams.RectangleCenterYPos + CurSearchParams.RectangleHeight / 2),
        };
    }
}

public class ParticleSearchParams : ICloneable
{
    public float RectangleCenterXPos { get; set; }
    public float RectangleCenterYPos { get; set; }
    //public float Angle;
    public float RectangleWidth { get; set; }
    public float RectangleHeight { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public class ParticleParamters
{
    public float Momentum { get; set; } = 0.3f;
    public float Z1 { get; set; }
    public float Z2 { get; set; }
    public float MemoryCoefficient { get; set; }
    public float SocialCoefficient { get; set; }
}

public class Velocities
{
    public float RectangleCenterXPos { get; set; }
    public float RectangleCenterYPos { get; set; }
    //public float Angle;
    public float RectangleWidth { get; set; }
    public float RectangleHeight { get; set; }
};
