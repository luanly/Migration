namespace SwissAcademic
{
    #region Sex

    public enum Sex
    {
        Unknown,
        Female,
        Male,
        Neutral
    }

    #endregion

    #region SyncFolderType

    // Note: if this list is expanded, 
    // SwissAcademic.Environment.GetInstalledSyncFolders() 
    // and Path2.GetSyncFolderPath 
    // has to be expanded, too.
    public enum SyncFolderType
    {
        None,
        DropBox,
        GoogleDrive,
        OneDriveBusiness,
        OneDrive,
        DropBoxBusiness
    }

    #endregion


    public enum CitaviSerializationContextType
    {
#if Web
        Breeze,
#endif
        Entities = 1,
        OnlineSearch = 2,
        DesktopCloud = 3,
        WordAddIn = 4,
        Database = 5
    }
}
