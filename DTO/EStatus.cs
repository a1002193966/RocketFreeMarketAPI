
namespace DTO
{
    public enum EStatus
    {
        Succeeded = 1,
        Failed = 0,
        DatabaseError = 500,
        EmailExists = -10,
        TokenExpired = -20,
        InvalidLink = -30
    }

    public enum ELoginStatus
    {
        LoginSucceeded = 1,
        EmailNotVerified = 0,
        AccountLocked = -1,
        AccountDisabled = -7,
        IncorrectCredential = -9
    }

}
