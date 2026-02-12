namespace PlanMorph.Infrastructure.Utilities;

public static class EnvLoader
{
    private const string DefaultEnvFile = ".env";

    public static void Load(string? envFileName = null)
    {
        var fileName = string.IsNullOrWhiteSpace(envFileName) ? DefaultEnvFile : envFileName;
        var envPath = FindEnvFile(fileName);

        if (envPath == null)
            return;

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                continue;

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                line = line.Substring(7).Trim();

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            if (value.Length >= 2)
            {
                var first = value[0];
                var last = value[value.Length - 1];
                if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                    value = value.Substring(1, value.Length - 2);
            }

            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? FindEnvFile(string fileName)
    {
        foreach (var root in EnumerateRoots())
        {
            var directory = new DirectoryInfo(root);
            for (var i = 0; i < 4 && directory != null; i++)
            {
                var candidate = Path.Combine(directory.FullName, fileName);
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateRoots()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var current = Directory.GetCurrentDirectory();
        if (seen.Add(current))
            yield return current;

        var baseDir = AppContext.BaseDirectory;
        if (seen.Add(baseDir))
            yield return baseDir;
    }
}
