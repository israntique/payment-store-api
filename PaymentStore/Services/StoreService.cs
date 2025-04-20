using System.Text.Json;
using PaymentsStoreApi.Models;
using PaymentStoreApi.Interfaces;

namespace PaymentStoreApi.Services;

public class StoreService : IStoreService
{
    private readonly ILogger<StoreService> _logger;

    public StoreService(ILogger<StoreService> logger)
    {
        _logger = logger;
    }

    public T GetFileJson<T>(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("File path is null or empty.");
                return default(T);
            }

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("File does not exist at path: {FilePath}", filePath);
                return default(T);
            }

            var json = System.IO.File.ReadAllText(filePath);
            _logger.LogInformation("File read successfully from path: {FilePath}", filePath);
            var dic = JsonSerializer.Deserialize<T>(json);
            return dic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading or deserializing file at path: {FilePath}", filePath);
            throw;
        }
    }

    public List<VoucherDetails> UpdateVoucherDetails(List<VoucherDetails> list, PaymentData PaymentData)
    {
        try
        {
            _logger.LogInformation("Updating voucher details for PaymentData with IdFile: {IdFile}, FileNum:{FileNum}", PaymentData.UserIdentification.IdFile, PaymentData.UserIdentification.FileNum);

            List<int> paidIds = PaymentData.VoucherDetails.Select(v => v.PayId).ToList();

            list.ForEach(v =>
            {
                if (paidIds.Contains(v.PayId))
                {
                    v.IsPaid = true;
                    _logger.LogInformation("Voucher with PayId {PayId} marked as paid.", v.PayId);
                }
            });

            _logger.LogInformation("Voucher details updated successfully.");
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher details.");
            throw new Exception("Error updating voucher details", ex);
        }
    }

    public Dictionary<string, List<VoucherDetails>> UpdateByPaymentDetails(PaymentData PaymentData, Dictionary<string, List<VoucherDetails>> dic)
    {
        try
        {
            string key = $"{PaymentData.UserIdentification.IdFile}_{PaymentData.UserIdentification.FileNum}";
            _logger.LogInformation("Updating payment details for key: {Key}", key);

            if (dic.ContainsKey(key))
            {
                dic[key] = UpdateVoucherDetails(dic[key], PaymentData);
                _logger.LogInformation("Payment details updated successfully for key: {Key}", key);
                return dic;
            }

            _logger.LogWarning("Key {Key} not found in dictionary.", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment details.");
            throw new Exception("Error updating voucher details", ex);
        }
    }
}
