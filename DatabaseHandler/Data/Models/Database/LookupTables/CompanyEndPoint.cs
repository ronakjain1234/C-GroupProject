using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler.Data.Models.Database.LookupTables;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey("CompanyID", "EndPointID")]
public class CompanyEndPoint
{

    public CompanyEndPoint(int companyID, int endpointID)
    {
        CompanyID = companyID;
        EndPointID = endpointID;
    }
    
    [ForeignKey("Companies")]
    public int CompanyID { get; set; }
    [ForeignKey("EndPoints")]
    public int EndPointID { get; set; }
    
    [ConcurrencyCheck]
    public DateTime LastChange {get; set;}
}