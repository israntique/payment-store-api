using System.Text.Json.Serialization;

namespace PaymentsStoreApi.Models;

public class VoucherDetails
{

    public int PayId { get; set; } // N10
    public int CustomerID { get; set; }
    public string CustomerName { get; set; }
    public string PlanName { get; set; }
    public string PayFor { get; set; }
    public decimal Sum { get; set; }
    [JsonIgnore]
    public bool IsPaid { get; set; }

}
