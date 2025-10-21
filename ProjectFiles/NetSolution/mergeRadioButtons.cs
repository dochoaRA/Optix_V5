#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.System;
using FTOptix.NetLogic;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using System.Linq;
using FTOptix.WebUI;
#endregion

public class mergeRadioButtons : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        var buttonList = Owner.FindNodesByType<RadioButton>();
        bool ButtonChecked = false;
        foreach (var button in buttonList)
        {
            button.OnUserValueChanged += Button_OnUserValueChanged;
            if (button.Checked)
            {
                ButtonChecked = true;
            }
        }

        // If no Button is selected, set the first button as checked
        if (!ButtonChecked && buttonList.Count() > 0)
        {
            foreach (var button in buttonList)
            {
                button.Checked = true; //only set the first button as checked
                break;
            }
            
        }
    }

    private void Button_OnUserValueChanged(object sender, UserValueChangedEvent e)
    {
        var senderButton = (RadioButton)sender;
        var buttonList = Owner.FindNodesByType<RadioButton>();
        foreach (var button in buttonList)
        {
            if (button.NodeId != senderButton.NodeId)
            {
                button.Checked = false;
            }
        }
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
