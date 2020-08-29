
namespace DTO
{
    public enum ERegisterStatus
    {
        RegisterSucceeded = 1,
        EmailExists = -1,
        InternalServerError = 0
    }

    public enum ELoginStatus
    {
        LoginSucceeded = 1,
        EmailNotVerified = 0,
        AccountLocked = -1,
        AccountDisabled = -7,
        IncorrectCredential = -9
    }

    public enum EEmailVerifyStatus
    {
        VerifiedSucceeded = 1,
        VerifiedFailed = 0,
        InternalServerError = -1
    }

}
