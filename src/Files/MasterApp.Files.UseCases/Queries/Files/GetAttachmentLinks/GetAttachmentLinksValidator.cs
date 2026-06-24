using FluentValidation;

namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;

internal sealed class GetAttachmentLinksValidator : AbstractValidator<GetAttachmentLinksQuery>
{
    public GetAttachmentLinksValidator()
    {
        RuleFor(query => query.FileIds)
            .NotNull();
    }
}

