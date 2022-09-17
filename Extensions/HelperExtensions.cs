namespace TelegramBot.Helpers;

public static class HelperExtensions
{
    public static List<long> GetIds(string fileName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);
        var rightIds = new List<long>();
        
        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrEmpty(line)) throw new ArgumentNullException($"New line is null in {filePath}");
            rightIds.Add(long.Parse(line));
        }

        return rightIds;
    }
}