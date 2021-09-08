namespace SwissAcademic.KeyVaultUtils
{
    public enum KnownCryptoKey
    {
        CitaviWeb,
        DbServer,
        Shibboleth,
        Backoffice
    }
    public static class KnownCryptoKeyExtensions
    {
        public static string GetCryptoKey(this KnownCryptoKey knownCryptoKey) => knownCryptoKey.ToString();
    }
}
