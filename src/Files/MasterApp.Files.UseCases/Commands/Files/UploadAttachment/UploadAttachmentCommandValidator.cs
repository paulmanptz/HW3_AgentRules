using FluentValidation;

namespace MasterApp.Files.UseCases.Commands.Files.UploadAttachment;

internal sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(command => command.Stream)
            .NotNull()
            .Must(command => command.CanRead);

        RuleFor(command => command.ContentType)
            .NotEmpty();

        RuleFor(command => command.FileName)
            .NotEmpty();
    }
}

