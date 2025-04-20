using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using PaymentsStoreApi.Models;
using PaymentStoreApi.Interfaces;

namespace PaymentStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<StoreController> _logger;
        public readonly IStoreService _storeService;
        string filePath = string.Empty;
        Dictionary<int, string> msg = new Dictionary<int, string>();

        public StoreController(IStoreService storeService, IConfiguration config, ILogger<StoreController> logger)
        {
            _config = config;
            _logger = logger;
            filePath = _config["DBFilePath"] ?? string.Empty;
            msg = new Dictionary<int, string>()
            {
                {0,"פעולה בוצע בהצלחה"},
                {1,"אופס! כבר לא ניתן לשלם את השובר מכיוון שפג תוקפו, נא לפנות למוקד רשות העתיקות בבקשה לקבל שובר חדש "},
                {2,"בדקו את קוד האימות ומספר השובר ונסו שנית"}
            };
            _storeService = storeService;
        }

        [HttpPost]
        [Route("voucher-details")]
        public IActionResult GetPaymentsDetails([FromBody] UserIdentification userIdentification)
        {
            _logger.LogInformation("GetPaymentsDetails called with UserIdentification: {@UserIdentification}", userIdentification);

            object? notPaid = null;
            List<VoucherDetails> VoucherDetails = new List<VoucherDetails>();

            if (userIdentification != null)
            {
                var dic = _storeService.GetFileJson<Dictionary<string, List<VoucherDetails>>>(filePath);
                if (dic == null)
                {
                    _logger.LogWarning("File not found or empty at path: {FilePath}", filePath);
                    return Ok(new { notPaid, Response });
                }

                string key = $"{userIdentification.IdFile}_{userIdentification.FileNum}";
                if (dic.ContainsKey(key))
                {
                    VoucherDetails = dic[key];
                    notPaid = VoucherDetails.Where(v => !v.IsPaid)?.ToList();
                    _logger.LogInformation("Voucher details retrieved successfully for key: {Key}", key);
                    return Ok(new { VoucherDetails = notPaid, Response = new Response { ResponseCode = 0, ResponseMessage = msg[0] } });
                }
            }

            _logger.LogWarning("Invalid UserIdentification or no matching data found.");
            return Ok(new
            {
                notPaid,
                Response = new Response
                {
                    ResponseCode = 2,
                    ResponseMessage = msg[2]
                }
            });
        }

        [HttpPost]
        [Route("payment-details")]
        public IActionResult UpdatePaymentDetails([FromBody] PaymentData PaymentData)
        {
            _logger.LogInformation("UpdatePaymentDetails called with PaymentData: {@PaymentData}", PaymentData);

            try
            {
                if (PaymentData != null)
                {
                    var dic = _storeService.GetFileJson<Dictionary<string, List<VoucherDetails>>>(filePath);
                    if (dic == null)
                    {
                        _logger.LogWarning("File not found or empty at path: {FilePath}", filePath);
                        return Ok(new Response
                        {
                            ResponseCode = 2,
                            ResponseMessage = msg[2]
                        });
                    }
                    dic = _storeService.UpdateByPaymentDetails(PaymentData, dic);

                    if (dic != null)
                    {
                        var options = new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            WriteIndented = true
                        };
                        System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(dic, options), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

                        _logger.LogInformation("Payment details updated successfully.");
                        return Ok(new Response { ResponseCode = 0, ResponseMessage = msg[0] });
                    }
                }
                _logger.LogWarning("Invalid PaymentData or update failed.");
                return Ok(new Response
                {
                    ResponseCode = 2,
                    ResponseMessage = msg[2]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating payment details.");
                return Ok(new Response
                {
                    ResponseCode = 2,
                    ResponseMessage = msg[2]
                });
            }
        }

        [HttpPost]
        [Route("export-details")]
        public IActionResult UpdateExportDetails([FromBody] List<PaymentData> PaymentDataArray)
        {
            _logger.LogInformation("UpdateExportDetails called with PaymentDataArray: {@PaymentDataArray}", PaymentDataArray);

            try
            {
                if (PaymentDataArray == null)
                {
                    _logger.LogWarning("PaymentDataArray is null.");
                    return Ok(new { RC = 1, MessageText = "Invalid request" });
                }
                var dic = _storeService.GetFileJson<Dictionary<string, List<VoucherDetails>>>(filePath);
                if (dic == null)
                {
                    _logger.LogWarning("File not found or empty at path: {FilePath}", filePath);
                    return Ok(new { RC = 1, MessageText = "File not found" });
                }
                foreach (var PaymentData in PaymentDataArray)
                {
                    if (dic == null)
                    {
                        _logger.LogWarning("dic became null during processing.");
                        break;
                    }
                    dic = _storeService.UpdateByPaymentDetails(PaymentData, dic);
                }
                if (dic == null)
                {
                    _logger.LogWarning("Update failed, dic is null.");
                    return Ok(new { RC = 1, MessageText = "Error details" });
                }
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                };
                System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(dic, options), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

                _logger.LogInformation("Export details updated successfully.");
                return Ok(new { RC = 0, MessageText = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating export details.");
                return Ok(new { RC = 1, MessageText = ex.Message });
            }
        }
    }
}
