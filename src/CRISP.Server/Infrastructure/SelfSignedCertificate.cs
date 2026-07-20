using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CRISP.Server.Infrastructure;

/// <summary>
/// Produces an ephemeral self-signed certificate used to terminate TLS on the
/// network leg between the load balancer and this container.
/// </summary>
/// <remarks>
/// The AWS load balancer re-encrypts traffic to the target but does not validate
/// the target's certificate. A self-signed certificate generated fresh on each
/// start therefore satisfies the Security Hub ELB encryption controls (target
/// group routing protocol and health-check protocol must be HTTPS) without any
/// certificate to store, rotate, or mount. Generating it in memory also keeps the
/// container's root filesystem read-only.
/// </remarks>
public static class SelfSignedCertificate
{
    // OID for the Server Authentication enhanced key usage.
    private const string ServerAuthenticationOid = "1.3.6.1.5.5.7.3.1";

    /// <summary>
    /// Creates a self-signed RSA certificate suitable for use as a Kestrel TLS
    /// server certificate.
    /// </summary>
    public static X509Certificate2 CreateForTls()
    {
        using var rsa = RSA.Create(2048);

        var request = new CertificateRequest(
            "CN=CRISP-internal-target",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: true));

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                [new Oid(ServerAuthenticationOid)],
                critical: false));

        var now = DateTimeOffset.UtcNow;
        using var certificate = request.CreateSelfSigned(now.AddMinutes(-5), now.AddYears(10));

        // Round-trip through PKCS#12 so the private key is reliably bound to the
        // certificate for the TLS stack. Loading from the raw object can leave the
        // key unusable by Kestrel on some platforms.
        return X509CertificateLoader.LoadPkcs12(
            certificate.Export(X509ContentType.Pfx),
            password: null);
    }
}
