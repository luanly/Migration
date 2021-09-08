﻿
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserLinkedAccounts
    {
        #region Constructors

        public GetUserLinkedAccounts(Guid contactId)
        {
            ContactId = contactId.ToString();
        }

        #endregion

        #region Properties

        public string ContactId { get; private set; }

        #endregion
    }
}
