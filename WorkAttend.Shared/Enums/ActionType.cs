namespace WorkAttend.Shared.Enums
{

    public enum ActionTypeEnum
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        View = 4,
        Export = 5,
        JSON = 6,
    }
    public enum RightsEnum
    {
        C = 1,
        R = 2,
        U = 3,
        D = 4,
    }

    public enum SystemRole
    {
        SuperAdmin = 1
    }

    public enum EntityEnum : int
    {
        MenuNavigations = 2,
        Roles = 3,

    }
    public enum NotificationTypeEnum : int
    {

        TeamMate = 4,
    }


    public enum FileExtensionsEnum : int
    {
        pdf = 1,
        doc = 2,
        docx = 3,
        xlsx = 4,
        xls = 5,
        jpeg = 6,
        jpg = 7,
        png = 8,
        gif = 9,
        txt = 10
    }

    public enum SystemConnectionStringEnum : int
    {
        StoreConnectionString = 1
    }
}
