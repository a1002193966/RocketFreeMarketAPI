using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.DatabaseConnection
{
    public static class QueryConst
    {
        public readonly static string AccountInsertCMD = "INSERT INTO [Account](PhoneNumber, Email, PasswordHash, AesIV, AccountType) VALUES(@PhoneNumber, @Email, @PasswordHash, @AesIV, @AccountType)";
        public readonly static string AccessInsertCMD = "INSERT INTO [Access](AccountID, AesKey) VALUES(@AccountID, @AesKey)";
        public readonly static string UserInsertCMD = "INSERT INTO [User](AccountID) VALUES(@AccountID)";


        public readonly static string VerifyTokenCMD = "SELECT * FROM [ConfirmationToken] WHERE Email = @Email AND Token = @Token AND TokenType = 'Email' AND ExpirationDate > GETDATE()";
        public readonly static string ActivateAccountCMD = "UPDATE [Account] SET EmailVerificationStatus = 1 WHERE Email = @Email";

        public readonly static string GetAccountInfoByEmailCMD = "SELECT * FROM [Account] WHERE Email = @Email";
        public readonly static string GetAccountIDByEmailCMD = "SELECT AccountID FROM [Account] WHERE Email = @Email";
        public readonly static string GetAccountHashCMD = "SELECT AccountID, PasswordHash, AesIV FROM [Account] WHERE Email = @Email";
        public readonly static string GetAccountKeyCMD = "SELECT AesKey FROM [Access] WHERE AccountID = @AccountID";
    }
}
