#region Using directives
using System;
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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FTOptix.WebUI;
using LibUsbDotNet;
using LibUsbDotNet.Main;
#endregion

public class ReadDongle : BaseNetLogic
{
    public override void Start()
    {
        ReadDongleData();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public static void ReadDongleData()
    {


        try
        {
            // Use ResourceUri to resolve %USB1% and subfolders
            var usbDriveUri = new ResourceUri("%USB1%");
            var usbDrive = usbDriveUri.Uri;

            var fileUri = new ResourceUri("%USB1%/System Volume Information/IndexerVolumeGuid");
            var filePath = fileUri.Uri;


            string content = File.ReadAllText(filePath, System.Text.Encoding.Unicode);
            // Log.Info("File content:");
            // Log.Info(content);

        }
        catch (Exception ex)
        {
            //Log.Info($"Error reading file: {ex.Message}");
        }
    }


}
