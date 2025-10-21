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

public class GoToNrNetlogic : BaseNetLogic
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
    public void GoToNr()
    {
        // Insert code to be executed by the method
        // InformationModel.GetVariable(Owner.GetVariable("AxisNameTextId").Value);
    
        var dataGrid = InformationModel.Get<DataGrid>((NodeId)LogicObject.GetVariable("RecipeDataGrid").Value.Value);
        string TextBoxRecipe = Owner.Get<TextBox>("TextBox1").Text;


        var RecipeArraysizeTag = Project.Current.GetVariable("CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Controller Tags/Workfile/ArraySize/Recipe");
        int RecipeArraysize = RecipeArraysizeTag.RemoteRead();

        if (dataGrid != null)
        {
            //read textbox and check whether it can be converted to an integer
            if (int.TryParse(TextBoxRecipe, out int rowIndex))
            {
                // Ensure the row index is within valid range
                if (rowIndex > 0 && rowIndex <= RecipeArraysize)
                {
                    // Get the Database object from the current project
                    //var dbRecipes = Project.Current.Get<Store>("DataStores/dbRecipes");
                    // Get a specific table by name
                    //var tbSQLiteRecipeList = dbRecipes.Tables.Get<Table>("SQLiteRecipeList");
                    //Clear data
                    //Object[,] ResultSet;
                    //String[] Header;
                    //dbRecipes.Query($"SELECT * FROM SQLiteRecipeList WHERE RecipeNumber = {rowIndex} LIMIT 1", out Header, out ResultSet);

                    //Get NodeId
                    //var dataGridChildren = (ChildNodeCollection)InformationModel.Get(dataGrid.Model).Children;
                    //dataGrid.SelectedIndex = rowIndex; //dataGridChildren.ElementAt(rowIndex).NodeId;
                    //NodeIDofRow = X
                    //Scrollview.scrollTo(NodeIDofRow)
                }
            }

        }
    }
}
