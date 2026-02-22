namespace SlqStudio.Application.Services.LogFileServices.DTO;

public class LogFileListDto
{
    public List<LogFileDto> Files { get; set; } = new List<LogFileDto>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
