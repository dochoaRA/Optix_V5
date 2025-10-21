#region Using directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.System;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using FTOptix.WebUI;
#endregion



public class UpdateAlarmHistory : BaseNetLogic
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
    public void UpdateSQLdata()
    {
        var stAlarmHistoryStruct = Project.Current.GetVariable("CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Program:ProjectSpecific_Panelview/VP_AlarmHistory");
        // Read the new values
        var stAlarmHistory = stAlarmHistoryStruct.RemoteRead();
        // Convert Arraysize to Integer
        int intArraysize = Convert.ToInt32(((UAManagedCore.Struct)stAlarmHistory.Value).Values[1]);
              
        // Get the Database object from the current project
        var dbAlarmHistory = Project.Current.Get<Store>("DataStores/dbAlarmHistory");
        // Get a specific table by name
        var tbSQLiteAlarmHistory = dbAlarmHistory.Tables.Get<Table>("SQLiteAlarmHistory");
        //Clear data
        Object[,] ResultSet;
        String[] Header;
        dbAlarmHistory.Query("DELETE FROM SQLiteAlarmHistory", out Header, out ResultSet);

        // Prepare the header for the insert query (list of columns)
        string[] columns = { "Category", "DateTime", "DateTimeAck", "PartMachine", "DeviceTag", "ReasonFunction", "LT", "PackML_ID", "Message_ID", "Reason", "Function" };
        // Create the new object, a bidimensional array where the first element
        // is the number of rows to be added, the second one is the number
        // of columns to be added (same size of the columns array)
        var values = new object[intArraysize, 7];
        // get Alarm Array
        var stAlarmHistoryList = ((UAManagedCore.Struct)stAlarmHistory.Value).Values[0];
        
        // Set some values for each column 

        for (int i = 0; i < intArraysize + 1; i++)
        {
            var alarmRow = ((UAManagedCore.Struct[])stAlarmHistoryList)[i].Values;



            // Translate each DINT part (Uses ActualID, not Language!)
            var AlarmMessageParts = alarmRow[6].ToString().Split('.');
            string AlarmReason = AlarmMessageParts[0];
            string AlarmFunction = AlarmMessageParts[1];
            string AlarmMachinePart = AlarmMessageParts[2];
   
            // Build concatenated string with only non-empty parts
            string messageId = "";

            if (!string.IsNullOrEmpty(AlarmReason))
                messageId = AlarmReason;
            else
                messageId = "0"; // If AlarmReason is empty, set to "0" to avoid empty Message_ID

            if (!string.IsNullOrEmpty(AlarmFunction))
                messageId = messageId + "." + AlarmFunction;
            else
                messageId = messageId + ".0";

            if (!string.IsNullOrEmpty(AlarmMachinePart))
                messageId = string.IsNullOrEmpty(messageId) ? AlarmMachinePart : messageId + "." + AlarmMachinePart;


            LocalizedText AlarmReasonLT = new LocalizedText(AlarmReason);
            LocalizedText AlarmFunctionLT = new LocalizedText(AlarmFunction);
            LocalizedText AlarmMachinePartLT = new LocalizedText(AlarmMachinePart);

            // Extract the fields accordingly:
             var rowValues = new object[11];
            rowValues[0] = Convert.ToInt32(alarmRow[4]); // Category
            rowValues[1] = alarmRow[7]; // DateTime
            rowValues[2] = alarmRow[8]; // DateTimeAck
            rowValues[3] = InformationModel.LookupTranslation(AlarmMachinePartLT).Text; // PartMachine
            rowValues[4] = AlarmMessageParts[3].ToString(); // DeviceTag
            string reasonText = InformationModel.LookupTranslation(AlarmReasonLT).Text;
            string functionText = InformationModel.LookupTranslation(AlarmFunctionLT).Text;
            if (!string.IsNullOrEmpty(reasonText) && !string.IsNullOrEmpty(functionText))
                rowValues[5] = reasonText + " / " + functionText; // Reason / Function
            else if (!string.IsNullOrEmpty(reasonText))
                rowValues[5] = AlarmReasonLT; //Reason
            else
                rowValues[5] = AlarmFunctionLT; //Function

            rowValues[6] = ""; // Sign to go to AlarmDetail
            rowValues[7] = Convert.ToInt32(alarmRow[3]); // PackML_ID
            rowValues[8] = messageId; // Message_ID
            rowValues[9] = AlarmReasonLT; // Reason - seperate from ReasonFunction becuase in the Alarmdetail they are displayed separately
            rowValues[10] = AlarmFunctionLT; // Function - but in the Alarmhistory they are displayed in the same column - avoids multiple converters in Optix for that column

            // Insert the row
            tbSQLiteAlarmHistory.Insert(columns, new object[,] {
                { rowValues[0], rowValues[1], rowValues[2], rowValues[3], rowValues[4], rowValues[5], rowValues[6], rowValues[7], rowValues[8], rowValues[9], rowValues[10] }
            });
        }
    
    }

}
