using PaymentsStoreApi.Models;

namespace PaymentStoreApi.Interfaces;

public interface IStoreService
{
    public Dictionary<string, List<VoucherDetails>> UpdateByPaymentDetails(PaymentData PaymentData, Dictionary<string, List<VoucherDetails>> dic);

    public List<VoucherDetails> UpdateVoucherDetails(List<VoucherDetails> list, PaymentData PaymentData);
    public T GetFileJson<T>(string filePath);
}
