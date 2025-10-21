#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.WebUI;
using FTOptix.System;
using FTOptix.CoreBase;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using System.Linq;
#endregion

public class UserListNetlogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        UpdateUsers();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void UpdateUsers()
    {
        // Get the Database object from the current project
        var dbUsers = Project.Current.Get<Store>("DataStores/dbUsers");
        // Get a specific table by name
        var tbSQLiteUserList = dbUsers.Tables.Get<Table>("SQLiteUserList");
        //Clear data
        Object[,] ResultSet;
        String[] Header;
        dbUsers.Query("DELETE FROM SQLiteUserList", out Header, out ResultSet);

        // Prepare the header for the insert query (list of columns)
        string[] columns = { "User", "Role", "LocaleId", "DateOfCreation", "Creator" };
        // Create the new object, a bidimensional array where the first element
        // is the number of rows to be added, the second one is the number
        // of columns to be added (same size of the columns array)

        // Important data information
        // Optix changes the language with the locale, even though there is a variable labelled "Language"
        // But in the HMI the column is labelled as language for better understanding

        // A similar issue are groups and roles. While the variable is labelled "Role" in this script and the HMI
        // it is actually working with groups

        // Set some values for each column
        var UserFolder = Project.Current.Get<Folder>("Security/Users");

        //Loop through all path segments and create objects
        foreach (var user in UserFolder.Children)
        {
            string UserName = user.BrowseName;
            var UserGroups = user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false);
            string UserRole = "";
            foreach (var group in UserGroups)
            {
                UserRole = group.BrowseName.ToString();
            }
            string UserLocale = "";
            try
            {
                // If the user has Locale, we set it to the fallback value
                UserLocale = user.GetVariable("LocaleId").Value.Value.ToString();
            }
            catch
            {
                UserLocale = "en-US";
            }

            var UserDateOfCreation = user.GetVariable("DateOfCreation").Value.Value;
            string UserCreator = user.GetVariable("Creator").Value.Value.ToString();

            // Extract the fields accordingly:
            var rowValues = new object[5];
            rowValues[0] = UserName;
            rowValues[1] = UserRole;
            rowValues[2] = UserLocale;
            rowValues[3] = UserDateOfCreation;
            rowValues[4] = UserCreator;

            // Insert the row
            tbSQLiteUserList.Insert(columns, new object[,] {
                { rowValues[0], rowValues[1], rowValues[2], rowValues[3], rowValues[4] }
            });
        }

    }

    [ExportMethod]
    public void SetRFID()
    {
        // Check the RFID value from the plc
        // if unsuccessful, continue for 5 seconds and then throw an error

        // Search the user from the alias and set the RFID parameter

        // Open Info that it was successful


    }

    [ExportMethod]
    public void AddNewUser(string username, string password, string password2)
    {
        // Add a new user to the system with the parameters from the HMI

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password2))
        {
            //Log.Error("EditUserDetailPanelLogic", "Username or password is empty");
            //Open Dialog - PasswordEmpty

            var InfoWindow = (DialogType)Project.Current.Get("UI/Component library/Page/Miscellaneous/DialogBox/InfoWindowOKOnly");
            var parentPanel = (FTOptix.UI.Panel)Owner;
            UICommands.OpenDialog(parentPanel, InfoWindow);
            return;
        }

        var UserFolder = Project.Current.Get<Folder>("Security/Users");

        foreach (var child in UserFolder.Children.OfType<UserCore>())
        {
            if (child.BrowseName.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                //Log.Error("EditUserDetailPanelLogic", "Username already exists");
                //Open Dialog - UsernameAlreadyExists
                return;
            }
        }




        var user = InformationModel.MakeObject<UserCore>(username);
        var rfidId = 123123123; //Temporary static value, to be replaced with a function reading from PLC
        user.GetVariable("RFID_Id").Value = rfidId;
        Folder users = Project.Current.Get<Folder>("Security/Users");
        users.Add(user);

        //Apply LocaleId
        var locale = (string)Owner.GetVariable("LocaleId").Value;
        if (!string.IsNullOrEmpty(locale))
            user.LocaleId = locale;

        //Apply groups
        ApplyGroups(user);

        //Apply password
        var result = Session.ChangePassword(username, password, string.Empty);

        switch (result.ResultCode)
        {
            case FTOptix.Core.ChangePasswordResultCode.Success:
                break;
            case FTOptix.Core.ChangePasswordResultCode.WrongOldPassword:
                //Not applicable
                break;
            case FTOptix.Core.ChangePasswordResultCode.PasswordAlreadyUsed:
                //Not applicable
                break;
            case FTOptix.Core.ChangePasswordResultCode.PasswordChangedTooRecently:
                //Not applicable
                break;
            case FTOptix.Core.ChangePasswordResultCode.PasswordTooShort:
                //ShowMessage(6);
                users.Remove(user);
                //return NodeId.Empty;
                break;
            case FTOptix.Core.ChangePasswordResultCode.UserNotFound:
                //Not applicable
                break;
            case FTOptix.Core.ChangePasswordResultCode.UnsupportedOperation:
                //ShowMessage(8);
                users.Remove(user);
                //return NodeId.Empty;
                break;

        }

        //return user.NodeId;


    }

    private void ApplyGroups(FTOptix.Core.User user)
    {
        var groupsPanel = Owner.Get<Panel>("HorizontalLayout1/GroupsPanel1");
        var editable = groupsPanel.GetVariable("Editable");
        var groups = groupsPanel.GetAlias("Groups");
        var panel = groupsPanel.Children.Get("ScrollView").Get("Container");

        if (editable.Value)
            return;

        if (user == null || groups == null || panel == null)
            return;

        var userNode = InformationModel.Get(user.NodeId);
        if (userNode == null)
            return;

        var groupCheckBoxes = panel.Refs.GetObjects(OpcUa.ReferenceTypes.HasOrderedComponent, false);

        foreach (var groupCheckBoxNode in groupCheckBoxes)
        {
            var group = groups.Get(groupCheckBoxNode.BrowseName);
            if (group == null)
                return;

            bool userHasGroup = UserHasGroup(user, group.NodeId);

            if (groupCheckBoxNode.GetVariable("Checked").Value && !userHasGroup)
                userNode.Refs.AddReference(FTOptix.Core.ReferenceTypes.HasGroup, group);
            else if (!groupCheckBoxNode.GetVariable("Checked").Value && userHasGroup)
                userNode.Refs.RemoveReference(FTOptix.Core.ReferenceTypes.HasGroup, group.NodeId, false);
        }
    }

    private bool UserHasGroup(IUANode user, NodeId groupNodeId)
    {
        if (user == null)
            return false;
        return user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false).Any(u => u.NodeId == groupNodeId);
    }


    [ExportMethod]
    public void EditUserRole()
    {
        // Edit an existing user role and locale


    }

    [ExportMethod]
    public void UserChangePassword()
    {
        // Change the password for the specified user

        // Check whether the user is active or not - old password required if active

    }

    [ExportMethod]
    public void ActiveUserChangePassword()
    {
        // Change the password for the specified user

        // Check whether the user is active or not - old password required if active

    }

    [ExportMethod]
    public void DeleteUser()
    {
        // Delete the specified user

        // Check whether the user is active or not - should be disabled in the HMI, but just in case

    }
    
    [ExportMethod]
    public void OpenMyDialogBox()
    {
        // Source type definition of the DialogBox
        var myDialogType = (DialogType)Project.Current.Get("UI/Templates/DialogBox1");
        // DialogBoxes needs a graphical container as parent in order to
        // understand to which session they have to spawn
        var parentPanel = (FTOptix.UI.Panel)Owner;
        UICommands.OpenDialog(parentPanel, myDialogType);
    }
}
