namespace PaymentsStoreApi.Models;

public class PaymentData
{
    public UserIdentification UserIdentification { get; set; }
    public List<VoucherDetails> VoucherDetails { get; set; }
    public OrderDetails OrderDetails { get; set; }

}
