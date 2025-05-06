using Domain.SharedKernel.Exceptions.PublicException;
using DomainStatus = Domain.OsagoAggregate.ExpiryStatus;

namespace Api.Adapters.Grpc.EnumMappers;

public class ExpiryStatusMapper
{
    public ExpiryStatus DomainToProto(DomainStatus domainStatus)
    {
        return domainStatus switch
        {
            _ when domainStatus == DomainStatus.NotExpired => ExpiryStatus.NotExpiredUnspecified,
            _ when domainStatus == DomainStatus.Expired => ExpiryStatus.Expired,
            _ => throw new ValueIsUnsupportedException($"{domainStatus} is unknown status")
        };
    }
}