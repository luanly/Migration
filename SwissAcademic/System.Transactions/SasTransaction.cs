using System.Data;
using System.Reflection;

namespace System.Transactions
{
    public static class SasTransaction
    {
        // Note: Before .NET Core, we had a static constructor that had set the transaction timeout
        // at runtime. This was a hack with reflection, see http://jupaol.blogspot.com/2015/03/changing-transactionscope-timeout-at-runtime.html
        // .NET Core does not know the two fields _cachedMaxTimeout and _maximumTimeout anymore, setting 
        // them via reflection throws an exception.
        // Since this timeout setting was experimental anyway, we simply forget about it in .NET Core.
        // If we happen to have transaction timeouts, we will have to find a new solution. Otherwise,
        // this comment can be removed.

        public static TransactionScope CreateTransactionScope(DatabaseType databaseType, TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var transactionOptions = new TransactionOptions();

            switch (databaseType)
            {
                case DatabaseType.Azure:
                    transactionOptions.IsolationLevel = isolationLevel == IsolationLevel.Unspecified ? IsolationLevel.ReadCommitted : isolationLevel;
                    break;

                case DatabaseType.SQLite:
                    transactionOptions.IsolationLevel = isolationLevel == IsolationLevel.Unspecified ? IsolationLevel.Serializable : isolationLevel;
                    break;

                case DatabaseType.SqlServer:
                    transactionOptions.IsolationLevel = isolationLevel == IsolationLevel.Unspecified ? IsolationLevel.Snapshot : isolationLevel;
                    break;
                default:
                    throw new NotImplementedException("DatabaseType {0} has no default isolation level.".FormatString(databaseType));
            }

            transactionOptions.Timeout = TransactionManager.MaximumTimeout;

            return new TransactionScope(transactionScopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
