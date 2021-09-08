using SwissAcademic.Resources;
using System;


namespace SwissAcademic
{
    public class NotConnectedToCloudException 
        : 
        ApplicationException
    {
        public override string Message => Strings.DesktopAlert_LostCloudConnection_Caption;

        public NotConnectedToCloudException() { }
        public NotConnectedToCloudException(Exception innerExcpetion) : base(Strings.DesktopAlert_LostCloudConnection_Caption, innerExcpetion) { }
    }
}
