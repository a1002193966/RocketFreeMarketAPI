using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.DatabaseConnection
{
    public static class QueryConst
    {
        public const string AccountInsertCMD = "INSERT INTO [Account](PhoneNumber, Email, PasswordHash, AesIV, AccountType) VALUES(@PhoneNumber, @Email, @PasswordHash, @AesIV, @AccountType)";
        public const string AccessInsertCMD = "INSERT INTO [Access](AccountID, AesKey) VALUES(@AccountID, @AesKey)";
        public const string UserInsertCMD = "INSERT INTO [User](AccountID) VALUES(@AccountID)";

        public const string VerifyTokenCMD = "SELECT Email, Token FROM [ConfirmationToken] WHERE Email = @Email AND Token = @Token AND TokenType = 'Email' AND ExpirationDate > GETDATE()";
        public const string ActivateAccountCMD = "UPDATE [Account] SET EmailVerificationStatus = 1, AccountStatus = 1 WHERE Email = @Email";

        public const string GetAccountInfoByEmailCMD = "SELECT * FROM [Account] WHERE Email = @Email";
        public const string GetAccountStatusByEmailCMD = "SELECT AccountStatus FROM [Account] WHERE Email = @Email";
        public const string GetAccountIDByEmailCMD = "SELECT AccountID FROM [Account] WHERE Email = @Email";
        public const string GetAccountHashCMD = "SELECT AccountID, PasswordHash, AesIV FROM [Account] WHERE Email = @Email";
        public const string GetAccountKeyCMD = "SELECT AesKey FROM [Access] WHERE AccountID = @AccountID";
    }
}
