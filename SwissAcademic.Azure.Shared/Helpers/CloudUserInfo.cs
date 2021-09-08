using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using SwissAcademic.Azure.Storage;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SwissAcademic.Azure
{
    public class CloudUserInfo :
        TableEntity,
        IDisposable
    {

        #region Fields

        protected bool _disposed;

        #endregion

        #region Constructors

        public CloudUserInfo() { }
        public CloudUserInfo(string contactKey, string projectKey)
        {
            ContactKey = contactKey;
            ProjectKey = projectKey;
        }

        #endregion

        #region Properties

        #region BlocksSave

        [JsonIgnore]
        [IgnoreProperty]
        public bool BlocksSave { get; set; }

        #endregion

        #region ContactKey
        [JsonProperty("ContactKey")]
        [IgnoreProperty]
        public string ContactKey { get => RowKey; set => RowKey = value; }
        #endregion

        #region DataCenter

        [JsonProperty]
        public DataCenter DataCenter { get; set; }

        #endregion
        #region DataCenterShortName

        [JsonProperty]
        public string DataCenterShortName { get; set; }

        #endregion

        #region Email

        [JsonProperty("Email")]
        public string Email { get; set; }

        #endregion

        #region FirstName
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }
        #endregion

        #region FullName
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
        #endregion

        #region LastName
        [JsonProperty("LastName")]
        public string LastName { get; set; }
        #endregion

        #region ProjectKey

        [JsonIgnore]
        public string ProjectKey { get => PartitionKey; set => PartitionKey = value; }

        #endregion

        #region SignalRConnectionId
        [JsonProperty("SignalRConnectionId")]
        public string SignalRConnectionId { get; set; }
        #endregion

        #region UserImageData
        [JsonIgnore]
        [IgnoreProperty]
        public byte[] UserImageData => !string.IsNullOrEmpty(UserImageDataBase64) ? Convert.FromBase64String(UserImageDataBase64) : null;
        #endregion

        #region UserImageDataBase64
        [JsonProperty("UserImageDataBase64")]
        [IgnoreProperty]
        public string UserImageDataBase64 { get; set; }
        #endregion

        #region UserSettingsAccess
        [JsonProperty("UserSettingsAccess")]
        [IgnoreProperty]
        public AzureTableAccess UserSettingsAccess { get; set; }
        #endregion

        #region UserImage
        private Image _userImage;
        [JsonIgnore]
        [IgnoreProperty]
        public Image UserImage
        {
            get
            {
                if (UserImageData == null || !UserImageData.Any()) return null;
                if (_userImage != null) return _userImage;

                try
                {
                    using (var memoryStream = new MemoryStream(UserImageData))
                    {
                        _userImage = Image.FromStream(memoryStream);
                    }
                }
                catch
                {
                    _userImage = null;
                }
                return _userImage;
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Dispose

        public void Dispose()
        {
            Disposing(true);
            _disposed = true;
        }

        protected virtual void Disposing(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _userImage?.Dispose();
            }
        }

        #endregion

        #region Equals
        public override bool Equals(object obj)
        {
            var user = obj as CloudUserInfo;
            return user?.ContactKey == ContactKey;
        }
        #endregion

        #region GetHashCode
        public override int GetHashCode()
        {
            return ContactKey.GetHashCode();
        }
        #endregion

        #endregion
    }

}
