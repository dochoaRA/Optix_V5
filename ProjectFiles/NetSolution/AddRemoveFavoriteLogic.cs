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
using System.Linq;
using FTOptix.WebUI;
#endregion

public class AddRemoveFavoriteLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started

        foreach (var child in Session.User.Children.OfType<IUAObject>())
        {
            /*if (
                child.GetVariable("ComponentLabel").Value == ((LocalizedText)Owner.GetVariable("ComponentNodeId").Value).TextId &&
                child.GetVariable("FunctionTextId").Value == ((LocalizedText)Owner.GetVariable("FunctionTextId").Value).TextId &&
                child.GetVariable("GroupTextId").Value == ((LocalizedText)Owner.GetVariable("GroupTextId").Value).TextId &&
                child.GetVariable("GroupSubHeaderTextId").Value == ((LocalizedText)Owner.GetVariable("GroupSubHeaderTextId").Value).TextId
            )*/
            if (child.GetVariable("ComponentNodeId").Value == Owner.GetVariable("ComponentNodeId").Value)
            {
                Owner.GetVariable("ComponentFavorite").Value = child.GetVariable("ComponentFavorite").Value;
                break;
            }
        }
        IUANode ComponentNode = InformationModel.Get(Owner.GetVariable("ComponentNodeAlias").Value);
        // Panelloader doe not seem to work, best bet is copying the structure? with CreateOrUpdateObject?
        //var PanelloaderComponent = InformationModel.Get<PanelLoader>(Owner.Children.Get("FavoriteTabPanel").Children.Get("ComponentPanel").Children.Get("PanelloaderComponent").NodeId);
        //var ComponentNodeId = ComponentNode.Children.Get("GridLayout1").Children.Get("Component").NodeId;
        //var ComponentNodeId = ComponentNode;
        //PanelloaderComponent.ChangePanel(ComponentNodeId);

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void RemoveFavorite()
    {
        var myUser = Session.User;
        // Check if we have this tab in the favorites list
        foreach (var child in Session.User.Children.OfType<IUAObject>())
        {
            if (child.GetVariable("ComponentNodeId").Value == Owner.GetVariable("ComponentNodeId").Value)
            {
                child.Delete();
                break;
            }

        }

        
    }
    [ExportMethod]
    public void AddFavorite()
    {

        var myUser = Session.User;
        IUANode ComponentNode = InformationModel.Get(Owner.GetVariable("ComponentNodeAlias").Value);
        // Create the new favorite object and add it to the user
        var newFav = InformationModel.Make<FavoriteComponent>(NodeId.Random(1).ToString());
        //newFav.GetVariable("ComponentLabel").Value = ((FTOptix.UI.Label)(new System.Linq.SystemCore_EnumerableDebugView<UAManagedCore.IUANode>(((UAManagedCore.UANode)(new System.Linq.SystemCore_EnumerableDebugView<UAManagedCore.IUANode>(((UAManagedCore.UANode)ComponentNode).Children).Items[1])).Children).Items[3])).LocalizedText.TextId;
        // In the string formatter the values of label and unit do not have their textIds. For usability and translation purposes they are extracted to a higher level
        LocalizedText Label = ComponentNode.Children.Get<IUAVariable>("Label").Value;
        LocalizedText Unit = ComponentNode.Children.Get<IUAVariable>("Unit").Value;
        
        String LabelUnit = string.IsNullOrEmpty(Unit.TextId) ? Label.TextId :
                            Label.TextId + "[" + Unit.TextId + "]";
        newFav.GetVariable("ComponentLabel").Value = LabelUnit;

        // Loop for functionTextId and GroupTextId and GroupSubHeaderTextId

        newFav.GetVariable("FunctionTextId").Value = ComponentNode.Parent.Parent.Parent.Children.Get("Header").BrowseName;
        newFav.GetVariable("GroupTextId").Value = ComponentNode.Parent.Parent.Parent.Children.Get("Header").Children.Get("HorizontalLayout2").BrowseName;
        newFav.GetVariable("GroupSubHeaderTextId").Value = ComponentNode.Parent.Parent.Parent.Children.Get("Header").Children.Get("HorizontalLayout2").BrowseName;
        newFav.GetVariable("PathTextId1").Value = Project.Current.GetVariable("Model/CurrentPath/PageNamePath1").Value;
        newFav.GetVariable("PathTextId2").Value = Project.Current.GetVariable("Model/CurrentPath/PageNamePath2").Value;
        newFav.GetVariable("PathTextId3").Value = Project.Current.GetVariable("Model/CurrentPath/PageNamePath3").Value;
        newFav.GetVariable("PathTextId4").Value = Project.Current.GetVariable("Model/CurrentPath/PageNamePath4").Value;
        newFav.GetVariable("ComponentNodeId").Value = Owner.GetVariable("ComponentNodeAlias").Value;
        newFav.GetVariable("ComponentFavorite").Value = true;
        myUser.Add(newFav);
        
       // newFav.GetVariable("GroupTextId").Value = 
       // ComponentNode.Parent.Parent.Parent.Children.Get("Header").Children.Get("HorizontalLayout2").BrowseName;
    }
    
    private void CreateOrUpdateObject(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        var existingNode = GetChild(fieldNode, parentNode, browseNamePrefix);
        // Replacing "/" with "_". Nodes with browsename "/" are not allowed
        var filedNodeBrowseName = fieldNode.BrowseName.Replace("/", "_");

        if (existingNode == null)
        {
            existingNode = InformationModel.MakeObject(browseNamePrefix + filedNodeBrowseName);
            parentNode.Add(existingNode);
        }

        foreach (var t in fieldNode.Children)
        {
            CreateModelTag(t, existingNode);
        }
    }
    //((IUAVariable)existingNode).SetDynamicLink((UAVariable)fieldNode, FTOptix.CoreBase.DynamicLinkMode.ReadWrite);

    private void CreateModelTag(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        switch (fieldNode)
        {
            case TagStructure _:
                if (!IsTagStructureArray(fieldNode))
                {
                    CreateOrUpdateObject(fieldNode, parentNode, browseNamePrefix);
                }
                else
                {
                    CreateOrUpdateObjectArray(fieldNode, parentNode, browseNamePrefix);
                }
                break;
            default:
                CreateOrUpdateVariable(fieldNode, parentNode, browseNamePrefix);
                break;
        }
    }

    private void CreateOrUpdateVariable(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        if (IsArrayDimentionsVar(fieldNode)) return;
        var existingNode = GetChild(fieldNode, parentNode, browseNamePrefix);

        if (existingNode == null)
        {
            var mTag = (IUAVariable)fieldNode;
            // Replacing "/" with "_". Nodes with browsename "/" are not allowed
            var tagBrowseName = mTag.BrowseName.Replace("/", "_");
            existingNode = InformationModel.MakeVariable(tagBrowseName, mTag.DataType, mTag.ArrayDimensions);
            parentNode.Add(existingNode);
        }
        ((IUAVariable)existingNode).SetDynamicLink((UAVariable)fieldNode, FTOptix.CoreBase.DynamicLinkMode.ReadWrite);
    }
    private void CreateOrUpdateObjectArray(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        var tagStructureArrayTemp = (TagStructure)fieldNode;
        foreach (var c in tagStructureArrayTemp.Children.Where(c => !IsArrayDimentionsVar(c)))
        {
            CreateModelTag(c, parentNode, fieldNode.BrowseName + "_");
        }
    }
    static private bool IsTagStructureArray(IUANode fieldNode) => ((TagStructure)fieldNode).ArrayDimensions.Length != 0;
    private bool IsArrayDimentionsVar(IUANode n) => n.BrowseName.ToLower().Contains("arraydimen");
    private IUANode GetChild(IUANode child, IUANode parent, string browseNamePrefix = "") => parent.Children.FirstOrDefault(c => c.BrowseName == browseNamePrefix + child.BrowseName);
}
