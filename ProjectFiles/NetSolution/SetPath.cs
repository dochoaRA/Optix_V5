#region Using directives
using System;
using UAManagedCore;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.Core;
using System.Collections.Generic;
using System.Linq;
using FTOptix.WebUI;
#endregion

public class SetPath : BaseNetLogic
{

    /// The panel loader to be passed to the navigation elements. This is where the display is loaded in after clicking on an item. The PanelLoader itself(the owner)
    private PanelLoader _pnlloader;
    /// The type of the core screen. Needed to find the location of the screen in the tree.
    private ScreenType _screenType;
    /// The folder where the screen type is located in.
    private Folder _screenFolder;



    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        //Get aliases and store in variables for easier access

        setCurrentPathInfo();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }


    /// Displays the navigation according the actual loaded display
    [ExportMethod]

    public void setCurrentPathInfo()
    {


        _pnlloader = (PanelLoader)Owner;
        IUAObject act = InformationModel.GetObject(_pnlloader.CurrentPanel);
        _screenFolder = (Folder)_pnlloader.GetAlias("ScreenFolder");

        //create a list to store all path segments
        List<IUANode> p = new List<IUANode>();

        //Start from the current parent segment (the lowest level)
        _screenType = findScreenType(act.GetType().Name, _screenFolder);
        if (_screenType == null)
            throw new Exception("Screen type not found in the specified folder.");
        IUANode l = _screenType.Owner;

        //The first element will be on the most right position. This is our actual screen, so we update this information.
        p.Add(_screenType);

        //Loop up level by level until we reach the root folder and add the segments to the list
        while (l.NodeId != _screenFolder.NodeId)
        {
            if ((l is Folder) && l.BrowseName.Contains("content"))
            {
                l = l.Owner;
                p.Add(l);
            }
            l = l.Owner;
        }


        //Reverse the list so that that we have the most top path segment at position 0
        p.Reverse();

        int PathLevel = 1;

        //Loop through all path segments and create objects
        foreach (var item in p)
        {
            IUANode dest = item;
            if (item is Folder && item.Children.Any())
                dest = item.Children.First();

            
            //var visiblePathVar = LogicObject.GetVariable($"VisiblePath{PathLevel}");
            var visiblePathVar = Session.Children.GetVariable($"VisiblePath{PathLevel}");
            var PageNamePathVar = Session.Children.GetVariable($"PageNamePath{PathLevel}");
            var PageLinkPathVar = Session.Children.GetVariable($"PageLinkPath{PathLevel}");

            visiblePathVar.SetValue(true);
            //PageNamePathVar.SetValue(dest.BrowseName);
            var ScreenName = dest.Children.GetVariable("ScreenName");

            PageNamePathVar.Value = ScreenName.Value;
            
            PageLinkPathVar.SetValue(dest.NodeId);
            PathLevel++;

        }


        // Set remaining VisiblePath variables to false up to PathLevel 4
        for (int i = PathLevel; i <= 4; i++)
        {
            var visiblePathVar = Session.Children.GetVariable($"VisiblePath{i}");
            if (visiblePathVar != null)
                visiblePathVar.SetValue(false);
        }

    }
    
    private ScreenType findScreenType(string name, Folder root)
    {
        foreach (var item in root.Children)
        {
            //Found: return item
            if (item.BrowseName == name && item is not Folder)
            {
                return (ScreenType)item;

            }
            else if (item is Folder)
            {
                //If folder, look recursively 
                ScreenType st = findScreenType(name, (Folder)item);
                if (st != null)
                    return st;
            }
        }      

        return null;
        
    }

}
