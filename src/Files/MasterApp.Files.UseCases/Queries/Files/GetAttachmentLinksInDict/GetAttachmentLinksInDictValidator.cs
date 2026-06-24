using FluentValidation;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;

namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinksInDict;
internal sealed class GetAttachmentLinksInDictValidator : AbstractValidator<GetAttachmentLinksQuery>
{
    public GetAttachmentLinksInDictValidator()
    {
        RuleFor(query => query.FileIds)
            .NotNull();
    }
}

