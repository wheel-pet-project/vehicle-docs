using Api.Adapters.Grpc.EnumMappers;
using Application.UseCases.Commands.AddOsago;
using Application.UseCases.Commands.AddPtsToVehicleDocuments;
using Application.UseCases.Commands.AddStsToVehicleDocuments;
using Application.UseCases.Queries.GetOsagoByVehicleDocumentsId;
using Application.UseCases.Queries.GetPtsByVehicleDocumentsId;
using Application.UseCases.Queries.GetStsByVehicleDocumentsId;
using Application.UseCases.Queries.GetVehicleDocumentsByVehicleId;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.Exceptions.PublicException;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Api.Adapters.Grpc;

public class VehicleDocumentsV1(
    IMediator mediator,
    ColorMapper colorMapper,
    ExpiryStatusMapper expiryStatusMapper) : VehicleDocuments.VehicleDocumentsBase
{
    public override async Task<AddStsResponse> AddSts(AddStsRequest request, ServerCallContext context)
    {
        var response = await mediator.Send(new AddStsToVehicleDocumentsCommand(
            ParseGuidOrThrow(request.VehicleDocsId),
            request.FrontPhotoBytes.ToList(),
            request.BackPhotoBytes.ToList()));

        return response.IsSuccess
            ? new AddStsResponse()
            : ParseErrorToRpcException<AddStsResponse>(response.Errors);
    }

    public override async Task<AddPtsResponse> AddPts(AddPtsRequest request, ServerCallContext context)
    {
        var response = await mediator.Send(new AddPtsToVehicleDocumentsCommand(
            ParseGuidOrThrow(request.VehicleDocsId),
            request.FrontPhotoBytes.ToList(),
            request.BackPhotoBytes.ToList(),
            DateOnly.FromDateTime(request.YearOfManufacture.ToDateTime()),
            colorMapper.ProtoToDomain(request.Color),
            request.Vin));

        return response.IsSuccess
            ? new AddPtsResponse()
            : ParseErrorToRpcException<AddPtsResponse>(response.Errors);
    }

    public override async Task<AddOsagoResponse> AddOsago(AddOsagoRequest request, ServerCallContext context)
    {
        var response = await mediator.Send(new AddOsagoCommand(
            ParseGuidOrThrow(request.VehicleDocsId),
            request.PhotoBytes.ToList(),
            DateOnly.FromDateTime(request.DateOfIssue.ToDateTime()),
            DateOnly.FromDateTime(request.DateOfExpiry.ToDateTime())));

        return response.IsSuccess
            ? new AddOsagoResponse()
            : ParseErrorToRpcException<AddOsagoResponse>(response.Errors);
    }

    public override async Task<GetVehicleDocumentsByVehicleIdResponse> GetVehicleDocumentsByVehicleId(
        GetVehicleDocumentsByVehicleIdRequest request,
        ServerCallContext context)
    {
        var response = await mediator.Send(new GetVehicleDocumentsByVehicleIdQuery(ParseGuidOrThrow(request.VehicleId)),
            context.CancellationToken);

        if (response.IsFailed) return ParseErrorToRpcException<GetVehicleDocumentsByVehicleIdResponse>(response.Errors);

        return new GetVehicleDocumentsByVehicleIdResponse
        {
            VehicleDocsId = response.Value.VehicleDocumentsId.ToString(),
            VehicleId = response.Value.VehicleId.ToString(),
            Status = new GetVehicleDocumentsByVehicleIdResponse.Types.Status
            {
                IsStsAdded = response.Value.Status.IsStsAdded,
                IsOsagoAdded = response.Value.Status.IsOsagoAdded,
                IsPtsAdded = response.Value.Status.IsPtsAdded
            }
        };
    }

    public override async Task<GetStsByVehicleDocumentsIdResponse> GetStsByVehicleDocumentsId(
        GetStsByVehicleDocumentsIdRequest request,
        ServerCallContext context)
    {
        var response = await mediator.Send(new GetStsByVehicleDocumentsIdQuery(
                ParseGuidOrThrow(request.VehicleDocsId)),
            context.CancellationToken);

        if (response.IsFailed) return ParseErrorToRpcException<GetStsByVehicleDocumentsIdResponse>(response.Errors);

        return new GetStsByVehicleDocumentsIdResponse
        {
            FrontPhotoS3Url = response.Value.FrontPhotoS3Url,
            BackPhotoS3Url = response.Value.BackPhotoS3Url
        };
    }

    public override async Task<GetPtsByVehicleDocumentsIdResponse> GetPtsByVehicleDocumentsId(
        GetPtsByVehicleDocumentsIdRequest request,
        ServerCallContext context)
    {
        var response = await mediator.Send(new GetPtsByVehicleDocumentsIdQuery(ParseGuidOrThrow(request.VehicleDocsId)),
            context.CancellationToken);

        if (response.IsFailed) return ParseErrorToRpcException<GetPtsByVehicleDocumentsIdResponse>(response.Errors);

        return new GetPtsByVehicleDocumentsIdResponse
        {
            FrontPhotoS3Url = response.Value.FrontPhotoS3Url,
            BackPhotoS3Url = response.Value.BackPhotoS3Url,
            Vin = response.Value.Vin,
            Color = colorMapper.DomainToProto(response.Value.Color),
            YearOfManufacture =
                Timestamp.FromDateTime(response.Value.YearOfManufacture.ToDateTime(new TimeOnly(), DateTimeKind.Utc))
        };
    }

    public override async Task<GetOsagoByVehicleDocumentsIdResponse> GetOsagoByVehicleDocumentsId(
        GetOsagoByVehicleDocumentsIdRequest request,
        ServerCallContext context)
    {
        var response = await mediator.Send(new GetOsagoByVehicleDocumentsIdQuery(
            ParseGuidOrThrow(request.VehicleDocsId)), context.CancellationToken);

        if (response.IsFailed) return ParseErrorToRpcException<GetOsagoByVehicleDocumentsIdResponse>(response.Errors);

        return new GetOsagoByVehicleDocumentsIdResponse
        {
            PhotoS3Url = response.Value.PhotoS3Url,
            ExpiryStatus = expiryStatusMapper.DomainToProto(response.Value.ExpiryStatus),
            DateOfIssue =
                Timestamp.FromDateTime(response.Value.DateOfIssue.ToDateTime(new TimeOnly(), DateTimeKind.Utc)),
            DateOfExpiry =
                Timestamp.FromDateTime(response.Value.DateOfExpiry.ToDateTime(new TimeOnly(), DateTimeKind.Utc))
        };
    }

    private T ParseErrorToRpcException<T>(List<IError> errors)
    {
        if (errors.Exists(x => x is NotFound))
            throw new RpcException(new Status(StatusCode.NotFound, string.Join(' ', errors.Select(x => x.Message))));

        if (errors.Exists(x => x is CommitFail))
            throw new RpcException(new Status(StatusCode.Unavailable, string.Join(' ', errors.Select(x => x.Message))));

        if (errors.Exists(x => x is ObjectStorageUnavailable))
            throw new RpcException(new Status(StatusCode.Unavailable, string.Join(' ', errors.Select(x => x.Message))));

        throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(' ', errors.Select(x => x.Message))));
    }

    private Guid ParseGuidOrThrow(string potentialId)
    {
        return Guid.TryParse(potentialId, out var id)
            ? id
            : throw new ValueIsUnsupportedException($"{nameof(potentialId)} is invalid uuid");
    }
}