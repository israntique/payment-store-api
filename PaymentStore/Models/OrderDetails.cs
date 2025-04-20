namespace PaymentsStoreApi.Models;

public class OrderDetails
{
    public long OrderId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PayDevc { get; set; } // A(15)
    public long ShovarNum { get; set; } // N10
    public int Last4Digits { get; set; } // N4
    public string PayMethod { get; set; } // A(5)
    public string Mutag { get; set; } // A20
    public DateTime RikuzDate { get; set; }
    public int RikuzNum { get; set; } // N8
    public int PaymentNum { get; set; }
    public int PaymentType { get; set; }
    public decimal TotalCheck { get; set; }
}
