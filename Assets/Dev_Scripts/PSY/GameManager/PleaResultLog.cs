using System.Collections.Generic;

public static class PleaResultLog
{
    private static readonly List<string> pendingNews = new List<string>();

    public static IReadOnlyList<string> PendingNews => pendingNews;

    public static void Add(string news)
    {
        if (string.IsNullOrWhiteSpace(news))
            return;

        pendingNews.Add(news);
    }

    public static void Clear()
    {
        pendingNews.Clear();
    }
}
