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
using System.Threading;
using System.Linq;
using FTOptix.WebUI;
#endregion

public class AlternatingColors : BaseNetLogic
{
    public override void Start()
    {
        
        // Insert code to be executed when the user-defined logic is started
        var ObjectList = Owner.FindNodesByType<Panel>();
        bool LightBackground = true;

        //Thread.Sleep(350); // Delay for loading the first time - otherwise the alternating colors are only for the first few cams
        // it seem even 350 isnt enough to always work - Decision - faster page but only limited colors.

        foreach (var panel in ObjectList)
        {
            if (panel.Visible)
            {
                // Set the background color of the panel
                foreach (var rectangle in panel.Children.OfType<Rectangle>())
                {
                    rectangle.FillColor = LightBackground ? LightGray : DarkGray;
                }
                LightBackground = !LightBackground;
            }
        }
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    private Color LightGray = new Color(255, HexToRgb("#F5F6F7").R, HexToRgb("#F5F6F7").G, HexToRgb("#F5F6F7").B);
    private Color DarkGray = new Color(255, HexToRgb("#EBEEEF").R, HexToRgb("#EBEEEF").G, HexToRgb("#EBEEEF").B);

    public static (byte R, byte G, byte B) HexToRgb(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return (r, g, b);
        }
        throw new ArgumentException("Invalid hex color format");
    }
}
