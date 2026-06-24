using FluentResults;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterApp.Files.UseCases.Queries.FileDetails.GetMaxFileCreatedAt;

public sealed record GetMaxFileCreatedAtQuery(IReadOnlyCollection<Guid> FileIds) : IRequest<Result<GetMaxFileCreatedAtResponse>>;

