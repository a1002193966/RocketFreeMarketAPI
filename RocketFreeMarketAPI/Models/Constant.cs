using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Models
{
    public class Constant
    {
        public readonly static string accountInsertCMD = "INSERT INTO [Account](PhoneNumber, Email, PasswordHash, AesIV, AccountType) VALUES(@PhoneNumber, @Email, @PasswordHash, @AesIV, @AccountType)";
        public readonly static string accessInsertCMD = "INSERT INTO [Access](AccountID, AesKey) VALUES(@AccountID, @AesKey)";

        public readonly static string userInsertCMD = "INSERT INTO [User](AccountID) VALUES(@AccountID)";
        public readonly static string checkUniqueEmailCmd = "SELECT * FROM [Account] WHERE Email = @Email";

        public readonly static string GetAccountCmd = "SELECT AesKey FROM [Access] WHERE AccountID = @AccountID";
        public readonly static string GetAccountIDCmd = "SELECT AccountID FROM [Account] WHERE Email = @Email";

        public readonly static string VerifyAccountCmd = "SELECT AccountID, PasswordHash, AesIV FROM [Account] WHERE Email = @Email";
        public readonly static string GetKeyAccountCmd = "SELECT AesKey FROM [Access] WHERE AccountID = @AccountID";
    }
}
