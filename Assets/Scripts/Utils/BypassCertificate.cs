using UnityEngine.Networking;

namespace Utils
{
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // Always returns true, indicating that the certificate is valid
            return true;
        }
    }
}