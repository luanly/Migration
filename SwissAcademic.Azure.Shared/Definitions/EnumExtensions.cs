using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SwissAcademic.Azure.Shared
{
    public static class EnumExtensions
    {
        public static int ToHttpStatusCode(this SaveErrorCode errorCode)
        {
            switch(errorCode)
            {
                case SaveErrorCode.ChangesetMismatch:
                    return (int)HttpStatusCode.PreconditionFailed;
                case SaveErrorCode.ProjectLocked:
                    return (int)HttpStatusCode.Conflict;
                default:
                    return (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
