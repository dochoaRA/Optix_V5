#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.OPCUAServer;
using FTOptix.WebUI;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.CoreBase;
using FTOptix.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.ODBCStore;
#endregion

public class raSDK1_NL_NavUsingTag_1734 : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }


    [ExportMethod]

    public void NavTag()
    {
        //constant for library and type location in Logix Controller
        const string library = "Inf_Lib";
        const string libraryType = "Inf_Type";

        //Define strings for library and type to be read from Logix Controller
        string lib = "";
        string lType = "";

        //Define nodes used
        DialogType dBFromString = null;
        IUANode logixTag = null;
        IUAObject lButton = null;
        IUAObject launchAliasObj = null;
        string faceplateTypeName = "";

        //  Find the button owner object and the Ref_* tags associated with it.
        try
        {
            // Get button object
            lButton = Owner.Owner.GetObject(this.Owner.BrowseName);

            // Make Launch Object that will contain aliases
            launchAliasObj = InformationModel.MakeObject("LaunchAliasObj");
            // Get each alias from Launch Button and add them into Launch Object, and assign NodeId values 
            foreach (var inpTag in lButton.Children)
            {
                if (inpTag.BrowseName.Contains("Ref_"))
                {
                    if (inpTag.BrowseName == "Ref_Tag" )
                    {
                        logixTag = InformationModel.Get(((UAVariable)inpTag).Value);
                    }
                    // Make a variable with same name as alias of type NodeId
                    var newVar = InformationModel.MakeVariable(inpTag.BrowseName, OpcUa.DataTypes.NodeId);
                    // Assign alias value to new variable
                    
                    var aliasValue = ((UAManagedCore.UAVariable)inpTag).Value;
                    if (aliasValue.Value == null)
                    {
                        Log.Warning(this.GetType().Name, $"Null value encountered for alias {inpTag.BrowseName}. Skipping this alias.");
                        continue;
                    }
                    newVar.Value = aliasValue;
                    
                    
                    
                    //newVar.Value = ((UAManagedCore.UAVariable)inpTag).Value;
                    // Add variable int launch object
                    launchAliasObj.Add(newVar);
                }
            }
        }
        catch
        {
            Log.Warning(this.GetType().Name, "Error creating alias Ref_* objects");
            return;
        }

        // Make sure the Logix Tag is valid before continuing
        if (logixTag == null)
        {
            Log.Warning(this.GetType().Name, "Failed to get logix tag from Ref_* objects");
            return;
        }


        // Build the dialog box name and return the object
        try
        {
            var fpType = lButton.GetVariable("Cfg_DisplayType").Value;

            // From Logix Tag assemble the name of Faceplate
            lib = ((string)logixTag.Children.GetVariable(library).RemoteRead().Value).Replace('-', '_');
            lType = (string)logixTag.Children.GetVariable(libraryType).RemoteRead().Value;
            faceplateTypeName = lib + '_' + lType + '_' + fpType;


            // Find DialogBox from assembled Faceplate string
            var foundFp = Project.Current.Find(faceplateTypeName);
            if ( foundFp == null )
            {
                Log.Warning(this.GetType().Name, "Dialog Box '" + faceplateTypeName + "' not found for tag '" + logixTag.BrowseName + "'. Check tag members '" + library + "' (" + lib + ") and '" + libraryType + "' (" + lType + ")");
                return;
            }

            // if found is DialogType, than it is a faceplate type
            if (foundFp.GetType() == typeof(DialogType))
            {
                dBFromString = (DialogType)foundFp;
            }
            else // found current instance of faceplate
            {
                // Get faceplate type from instance
                System.Reflection.PropertyInfo objType = foundFp.GetType().GetProperty("ObjectType");
                dBFromString = (DialogType)(objType.GetValue(foundFp, null));
            }

        }
        catch
        {
            Log.Warning(this.GetType().Name, "Error retrieving Dialog Box for tag '" + logixTag.BrowseName + "'. Check tag members '" + library + "' (" + lib + ") and '" + libraryType + "' (" + lType + ")");
            return;
        }


        // Launch the faceplate
        try
        {
            // Launch DialogBox passing Launch Object that contains the aliases as an alias 
            UICommands.OpenDialog(lButton, dBFromString, launchAliasObj.NodeId);
        }
        catch
        {
            Log.Warning(this.GetType().Name, "Failed to launch dialog box '" + faceplateTypeName + "' for tag '" + logixTag.BrowseName + "'.");
            return;
        }


        // If configured, close the dialog box containing launch button
        try
        {
            bool cfgCloseCurrent = lButton.GetVariable("Cfg_CloseCurrentDisplay").Value;
            if (cfgCloseCurrent)
            {
                CloseCurrentDB(Owner);
            }
        }
        catch
        {
            Log.Warning(this.GetType().Name, "Failed to close current dialog box");
        }
    }

    public void CloseCurrentDB(IUANode inputNode)
    {
        // if input node is of type Dialog, close it
        if (inputNode.GetType().BaseType.BaseType == typeof(Dialog))
        {
            // close dialog box
            ((Dialog)inputNode).Close();
            return;
        }
        // if input node is Main Window, no dialog box was found, return
        if (inputNode.GetType() == typeof(MainWindow))
        {
            return;
        }
        // continue search for Dialog or Main Window
        CloseCurrentDB(inputNode.Owner);
    }
}
