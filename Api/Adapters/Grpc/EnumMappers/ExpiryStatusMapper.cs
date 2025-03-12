using Domain.SharedKernel.Exceptions.ArgumentException;
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
            _ => throw new ValueOutOfRangeException($"{domainStatus} is unknown status")
        };
    }
}