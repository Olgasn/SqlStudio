namespace SlqStudio.ViewModel.Log;

public class LogFileViewModel
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
}
