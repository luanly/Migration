using System;

namespace SwissAcademic.Azure.Storage
{
    public class AzureTableAccess
        :
        AzureStorageAccess
    {
        #region Konstruktoren

        public AzureTableAccess(Uri uri, string sharedAccessSignature, string partitionKey)
            :
            base(uri, sharedAccessSignature)
        {
            PartitionKey = partitionKey;
        }

        #endregion

        #region Properties

        #region PartitionKey

        public string PartitionKey { get; private set; }

        #endregion

        #endregion
    }
}
