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
#endregion

public class ReadRecipes : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        UpdateSQLdataSlowTask = new LongRunningTask(UpdateSQLdata, LogicObject);
        UpdateSQLdataSlowTask.Start();

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        UpdateSQLdataSlowTask?.Dispose();
    }


    
    [ExportMethod]
    public void UpdateSQLdata()
    {

        //var stWorkfileStruct = Project.Current.GetVariable("CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/Workfile/Recipe");
        // Read the new values
        //var stWorkfile = stWorkfileStruct.RemoteRead();
        // Convert Arraysize to Integer
        //int intArraysize = Convert.ToInt32(((UAManagedCore.Struct)stWorkfile.Value).Values[1]);

        // Get the Database object from the current project
        var dbRecipes = Project.Current.Get<Store>("DataStores/dbRecipes");
        // Get a specific table by name
        var tbSQLiteRecipeList = dbRecipes.Tables.Get<Table>("SQLiteRecipeList");
        //Clear data
        Object[,] ResultSet;
        String[] Header;
        dbRecipes.Query("DELETE FROM SQLiteRecipeList", out Header, out ResultSet);

        // Prepare the header for the insert query (list of columns)
        string[] columns = { "RecipeNumber", "RecipeName", "Timestamp", "DeleteLoad" };
        // Create the new object, a bidimensional array where the first element
        // is the number of rows to be added, the second one is the number
        // of columns to be added (same size of the columns array)

        //var stRecipeList = ((object[])((UAManagedCore.Struct[])((object[])((UAManagedCore.Struct)stWorkfile.Value).Values)[0])

        // Set some values for each column 
        var RecipeArraysizeTag = Project.Current.GetVariable("CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Controller Tags/Workfile/ArraySize/Recipe");
        int RecipeArraysize = RecipeArraysizeTag.RemoteRead();
        //var stRecipeList = (UAManagedCore.Struct[])((object[])((UAManagedCore.Struct)stWorkfile.Value).Values)[0];

        for (int i = 0; i < RecipeArraysize; i++)
        {
            // Process each recipe
            var RecipeNumberTag = Project.Current.GetVariable($"CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Controller Tags/Workfile/Recipe/Set/{i}/RecipeNumber");
            var RecipeNumberRead = RecipeNumberTag.RemoteRead();
            int RecipeNumber = Convert.ToInt32(RecipeNumberRead.Value);
            if (RecipeNumber == 0)
            {
                // Skip the row if RecipeNumber is 0
                continue;
            }


            var RecipeNameTag = Project.Current.GetVariable($"CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Controller Tags/Workfile/Recipe/Set/{i}/RecipeName");
            var RecipeNameRead = RecipeNameTag.RemoteRead();
            string RecipeName = RecipeNameRead.Value.ToString();

            var RecipeDateTimeTag = Project.Current.GetVariable($"CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Controller Tags/Workfile/Recipe/Set/{i}/DateTime");
            var RecipeDateTime = RecipeDateTimeTag.RemoteRead().Value;

            // Extract the fields accordingly:
            var rowValues = new object[4];
            rowValues[0] = RecipeNumber;
            rowValues[1] = RecipeName;
            rowValues[2] = RecipeDateTime;
            rowValues[3] = 0; // Placeholder for DeleteLoad, can be set later if needed

            // Insert the row
            tbSQLiteRecipeList.Insert(columns, new object[,] {
                { rowValues[0], rowValues[1], rowValues[2], rowValues[3] }
            });
        }

        
    }
    private LongRunningTask UpdateSQLdataSlowTask;
}