using FluentValidation;
using MasterApp.Files.UseCases.Commands.Files.UploadAttachment;
using Microsoft.Extensions.DependencyInjection;

namespace MasterApp.Files.UseCases;

public static class FilesUseCasesModule
{
    public static void AddFilesUseCases(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UploadAttachmentCommandValidator>(includeInternalTypes: true);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FilesUseCasesModule).Assembly));
    }
}


