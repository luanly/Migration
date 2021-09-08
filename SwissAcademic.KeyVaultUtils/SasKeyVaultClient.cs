using Flurl;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Azure.Services.AppAuthentication;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.KeyVaultUtils
{
    public class SasKeyVaultClient
    {

        #region Fields

        ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        string _keyVaultBaseUrl;
        readonly KeyVaultClient _client;

        #endregion

        #region Constructor

        public SasKeyVaultClient(string keyVaultBaseUrl, AzureServiceTokenProvider azureServiceTokenProvider)
        {
            _client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            _keyVaultBaseUrl = keyVaultBaseUrl;
        }

        public SasKeyVaultClient(string keyVaultBaseUrl)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            _client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            _keyVaultBaseUrl = keyVaultBaseUrl;
        }

        #endregion

        #region Properties

        #region SecretsUrl

        public string SecretsUrl => Url.Combine(_keyVaultBaseUrl, "/secrets");

        #endregion

        #endregion

        #region Methods

        #region GetSecretAsync

        public async Task<string> GetSecretAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return value;
            }

            try
            {
                value = await CallKeyVaultSecretApiAsync(key);
                _cache.TryAdd(key, value);
            }
            catch(Exception ex)
			{
                Telemetry.TrackException(ex, property1: (nameof(key), key));
			}
            return value;
        }

        #endregion

        #region CallKeyVaultSecretApiAsync

        async Task<string> CallKeyVaultSecretApiAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await _client.GetSecretAsync(Url.Combine(SecretsUrl, $"/{key}"), cancellationToken))?.Value;
        }

        #endregion

        #region DecryptAsync
        public Task<string> DecryptAsync(string encryptedValue, KnownCryptoKey knownCryptoKey, CancellationToken cancellationToken = default(CancellationToken)) =>
            DecryptAsync(encryptedValue, knownCryptoKey.GetCryptoKey(), cancellationToken);


        public Task<string> DecryptAsync(string encryptedValue, string key, CancellationToken cancellationToken = default(CancellationToken)) =>
            DecryptAsync(Convert.FromBase64String(encryptedValue), key, cancellationToken);


        public async Task<string> DecryptAsync(byte[] encryptedValue, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _client.DecryptAsync(_keyVaultBaseUrl, key, string.Empty, JsonWebKeyEncryptionAlgorithm.RSAOAEP, encryptedValue, cancellationToken);
            return Encoding.UTF8.GetString(result.Result);
        }

        #endregion

        #region EncryptAsync

        public Task<string> EncryptAsync(string value, KnownCryptoKey knownCryptoKey, CancellationToken cancellationToken = default(CancellationToken)) =>
           EncryptAsync(value, knownCryptoKey.GetCryptoKey(), cancellationToken);

        public Task<string> EncryptAsync(string value, string key, CancellationToken cancellationToken = default(CancellationToken)) =>
            EncryptAsync(Encoding.UTF8.GetBytes(value), key, cancellationToken);

        public async Task<string> EncryptAsync(byte[] value, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _client.EncryptAsync(_keyVaultBaseUrl, key, string.Empty, JsonWebKeyEncryptionAlgorithm.RSAOAEP, value, cancellationToken);
            return Convert.ToBase64String(result.Result);
        }

        #endregion

        #endregion
    }
}
