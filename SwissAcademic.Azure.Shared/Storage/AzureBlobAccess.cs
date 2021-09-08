using System;

namespace SwissAcademic.Azure.Storage
{
    public class AzureBlobAccess
        :
        AzureStorageAccess
    {
        #region Konstruktoren

        public AzureBlobAccess(Uri uri, string sharedAccessSignature, string containerName)
            :
            base(uri, sharedAccessSignature)
        {
            ContainerName = containerName;
        }

        #endregion

        #region Properties

        #region ContainerName

        public string ContainerName { get; }

        #endregion

        #endregion
    }
}
