using System.Security.Cryptography;
using System.Text;

namespace HardwareStore.Infrastructure.ExternalServices
{
    public class MercadoPagoWebhookValidator
    {
        private readonly string _secretKey;

        public MercadoPagoWebhookValidator(string secretKey)
        {
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        }

        public bool ValidateSignature(string xSignature, string xRequestId, string dataId)
        {
            if (string.IsNullOrEmpty(xSignature) || string.IsNullOrEmpty(xRequestId) || string.IsNullOrEmpty(dataId))
                return false;

            // Parsear x-signature: "ts=1234567890,v1=hash"
            var signatureParts = ParseSignatureHeader(xSignature);
            if (signatureParts == null)
                return false;

            var (timestamp, receivedHash) = signatureParts.Value;

            // Construir manifest template: id:{data.id};request-id:{x-request-id};ts:{timestamp};
            var manifest = $"id:{dataId};request-id:{xRequestId};ts:{timestamp};";

            // Calcular HMAC-SHA256
            var calculatedHash = CalculateHmacSha256(manifest, _secretKey);

            // Comparar hashes
            return calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        private (string timestamp, string hash)? ParseSignatureHeader(string xSignature)
        {
            try
            {
                var parts = xSignature.Split(',');
                string? timestamp = null;
                string? hash = null;

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length != 2)
                        continue;

                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    if (key == "ts")
                        timestamp = value;
                    else if (key == "v1")
                        hash = value;
                }

                if (timestamp == null || hash == null)
                    return null;

                return (timestamp, hash);
            }
            catch
            {
                return null;
            }
        }

        private string CalculateHmacSha256(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            var keyBytes = encoding.GetBytes(secret);
            var messageBytes = encoding.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
