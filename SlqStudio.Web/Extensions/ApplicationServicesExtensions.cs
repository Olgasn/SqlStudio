using SlqStudio.Application.Services.AppSettingsServices;
using SlqStudio.Application.Services.DiagramServices;
using SlqStudio.Application.Services.DiagramServices.Implementation;
using SlqStudio.Application.Services.EmailServices;
using SlqStudio.Application.Services.EmailServices.Implementation;
using SlqStudio.Application.Services.EmailServices.Models;
using SlqStudio.Application.Services.LogFileServices;
using SlqStudio.Application.Services.VariantServices;

namespace SlqStudio.Extensions;

public static class ApplicationServicesExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<VariantServices>();

        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddTransient<IEmailService, EmailService>();

        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<AppSettingsBuilder>();
        
        services.AddScoped<ILogFileService, LogFileService>();
        
        services.Configure<DiagramSettings>(configuration.GetSection("DiagramSettings"));
        services.AddScoped<IDiagramService, DiagramService>();
       


    }
}