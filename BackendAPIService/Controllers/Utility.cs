using DatabaseHandler;

namespace BackendAPIService.Controllers;

public class Utility
{
    public static bool IsUserInCompany(int userID, int companyID, ApplicationDbContext dbContext)
    {
        return dbContext.UserCompanies.Any(uc => uc.UserID == userID && uc.CompanyID == companyID);
    }
}