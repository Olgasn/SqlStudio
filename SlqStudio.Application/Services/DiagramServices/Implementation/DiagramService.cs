namespace SlqStudio.Application.Services.DiagramServices.Implementation;

public class DiagramService : IDiagramService
{
    public async Task<string> LoadDiagramAsync(string diagramPath)
    {
        try
        {
            if (File.Exists(diagramPath))
            {
                return await File.ReadAllTextAsync(diagramPath);
            }
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
