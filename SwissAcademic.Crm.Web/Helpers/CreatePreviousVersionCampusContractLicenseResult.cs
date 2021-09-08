namespace SwissAcademic.Crm.Web
{
    public class CreatePreviousVersionCampusContractLicenseResult
    {
        public CitaviLicense License { get; set; }
        public CreatePreviousVersionCampusContractLicenseResultType Status { get; set; }
    }

    public enum CreatePreviousVersionCampusContractLicenseResultType
    {
        LicenseAlreadyExists,
        NoCampusContractLicenseFound,
        Success
    }
}
