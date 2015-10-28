using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Bytloos.Web
{
    /// <summary>
    /// Certificate acception crutch.
    /// </summary>
    internal static class SSLValidator
    {
        private static bool OnValidateCertificate(
            object          sender,
            X509Certificate certificate,
            X509Chain       chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal static void OverrideValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;
            ServicePointManager.Expect100Continue = true;
        }
    }
}
