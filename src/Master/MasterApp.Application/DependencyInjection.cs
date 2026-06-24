using Microsoft.Extensions.DependencyInjection;
using MasterApp.Application.Interfaces;
using MasterApp.Application.Services;

namespace MasterApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcherServicesService, DispatcherServicesService>();
        services.AddScoped<IDispatcherQuestsService, DispatcherQuestsService>();
        services.AddScoped<IDispatcherMastersService, DispatcherMastersService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IQuestService, QuestService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<MasterApp.Auth.Application.Interfaces.IUserClaimsProvider, UserClaimsProvider>();
        
        return services;
    }
}
