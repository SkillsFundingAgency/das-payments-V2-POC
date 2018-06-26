using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public interface IAccountProvider
    {
        Account GetAccount(long id);


    }
}