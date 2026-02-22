namespace SlqStudio.Logger;

public class FileLogger : ILogger, IDisposable
{
    private readonly string _basePath;
    private readonly long _maxFileSize;
    private readonly int _maxFiles;
    private string _currentFilePath;
    private static readonly Lock Lock = new Lock();

    public FileLogger(string basePath, long maxFileSizeBytes, int maxFiles)
    {
        _basePath = basePath;
        _maxFileSize = maxFileSizeBytes;
        _maxFiles = maxFiles;
        _currentFilePath = GetNewFilePath();
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_basePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
    }

    private string GetNewFilePath()
    {
        var counter = 1;
        var fileName = Path.GetFileNameWithoutExtension(_basePath);
        var extension = Path.GetExtension(_basePath);
        var directory = Path.GetDirectoryName(_basePath);

        while (true)
        {
            var newPath = Path.Combine(
                directory!,
                $"{fileName}-{DateTime.Now:yyyy_MM_dd}-{counter}{extension}");

            if (!File.Exists(newPath))
            {
                return newPath;
            }

            counter++;
        }
    }

    private void RotateFileIfNeeded()
    {
        var fileInfo = new FileInfo(_currentFilePath);
        if (fileInfo.Exists && fileInfo.Length > _maxFileSize)
        {
            _currentFilePath = GetNewFilePath();
            CleanupOldFiles();
        }
    }

    private void CleanupOldFiles()
    {
        try
        {
            var directory = Path.GetDirectoryName(_basePath);
            var pattern = Path.GetFileNameWithoutExtension(_basePath) + "-*" + Path.GetExtension(_basePath);
            
            var files = Directory.GetFiles(directory!, pattern)
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .ToList();

            while (files.Count > _maxFiles)
            {
                var fileToDelete = files.Last();
                File.Delete(fileToDelete);
                files.Remove(fileToDelete);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up log files: {ex.Message}");
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;

    public void Dispose() { }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        lock (Lock)
        {
            RotateFileIfNeeded();
            
            var message = formatter(state, exception);
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel,-12}] {message}";

            if (exception != null)
            {
                logEntry += $"\nException: {exception}";
            }

            File.AppendAllText(_currentFilePath, logEntry + Environment.NewLine);
        }
    }
}
