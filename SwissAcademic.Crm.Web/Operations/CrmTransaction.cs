using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmTransaction
        :
        IDisposable
    {
        #region Ereigisse

        public event EventHandler Disposed;

        #endregion

        #region Konstruktor

        internal CrmTransaction(CrmDbContext context)
        {
            Context = context;
        }

        #endregion

        #region Eigenschaften

        #region Context

        CrmDbContext Context { get; }

        #endregion

        #region Id

        public string Id { get; } = Guid.NewGuid().ToString();

        #endregion

        #region IsDisposed

        public bool IsDisposed { get; private set; }

        #endregion

        #endregion

        #region Methoden

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsDisposed)
                {
                    return;
                }
                IsDisposed = true;
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #endregion
    }
}
