namespace TelegramBot.Helper;

public static class TelegramIdHelper
{
    public static IEnumerable<long> GetIds()
    {
        var fileName = "ids.txt";
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