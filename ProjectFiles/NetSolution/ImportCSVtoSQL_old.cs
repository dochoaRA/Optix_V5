#region Using directives
using System;
using FTOptix.Core;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.OPCUAServer;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.NetLogic;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FTOptix.CommunicationDriver;
using FTOptix.Modbus;
using FTOptix.RAEtherNetIP;
using FTOptix.WebUI;
#endregion

public class CsvToGrid : BaseNetLogic
{

    [ExportMethod]
    public void readCsvFile()
    {

    // Get the Database object from the current project
    var myStore = Project.Current.Get<Store>("DataStores/EmbeddedDatabase");
    // Get a specific table by name
    var myTable = myStore.Tables.Get<Table>("Rovema_translations_db");
    // Prepare the header for the insert query (list of columns)
    string[] columns = { "ID", "German (de)", "English (en)", "Description", "Polish (pl)", "Netherlands (nl)",
                            "Danish (da)", "Spanish (es)","Italian (it)", "French (fr)", "Chinese (zh)", "Hungarian (hu)",
                            "Portuguese (pt)", "Russian (ru)", "Romanian (ro)", "Swedish (sv)",  "Finnish (fi)",
                            "Norwegian (no)", "Turkish (tr)", "Bulgarian (bg)", "Greek (el)", "Croatian (hr)", "Czech (cs)",
                            "Japanese (ja)",  "Indonesian (id)", "Slovenian (sl)", "Estonian (et)","Hebrew (he)", "Serbian (sr)",
                            "Slovakian (sk)", "Portuguese (pt-BR)"};

    // Create the new object, a bidimensional array where the first element
    // is the number of rows to be added, the second one is the number
    // of columns to be added (same size of the columns array)

    //var values = new object[1, 31];

        var filePath = LogicObject.GetVariable("CsvFile");
        FTOptix.Core.ResourceUri filePathValue = new FTOptix.Core.ResourceUri(filePath.Value);
        string fileSeparator = LogicObject.GetVariable("CsvSeparator").Value;
        char[] characters = fileSeparator.ToCharArray();
        
        int rowNumber = 0;

        using (var reader = new StreamReader(filePathValue.Uri))
        {

            while (!reader.EndOfStream)
            {
                /*var line = reader.ReadLine();
                var stringValues = line.Split(characters[0]);
                var objectValues = stringValues.Cast<object>().ToArray(); // Convert string array to object array
                myTable.Insert(columns, objectValues);
                
                rowNumber++;*/

                var line = reader.ReadLine();
                var stringValues = line.Split(characters[0]);
                
                var objectValues = new object[1, columns.Length];
                for (int i = 0; i < columns.Length; i++)
                {
                    objectValues[0, i] = stringValues[i];
                }

                myTable.Insert(columns, objectValues);
                
                rowNumber++;

            }
        }
    }
}
