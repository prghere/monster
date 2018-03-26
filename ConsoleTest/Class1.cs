//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Data.SqlClient;
//using System.Data;
////For the TextFieldParse class
////Be sure to add it as a reference for your project!
//using Microsoft.VisualBasic.FileIO;
//using System;

//namespace ConsoleApplication1
//{
//    /// &lt;summary&gt;
//    ///Simple code snippets to read a file, process the lines in parallel, 
//    ///and upload the results into a database table.
//    /// 
//    ///Obviously if you don't need to copy the data into a database, just modify the code
//    ///as appropriate.
//    /// 
//    /// NOTE that this will have compile and runtime errors as it's just a 
//    /// rough code snippet to help you get started with your own implementation.
//    /// 
//    /// Think of it like a guide.
//    /// 
//    /// Code source from http://cc.davelozinski.com
//    /// &lt;/summary&gt;
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            //Basic usage to help you get started:
//            ProcessFileTaskItem(
//                new string[] { "\\\\the\\full\\oath\\to\\the\\first\\file.txt", "\\\\full\\path\\to\\second\\file.txt" }
//                , "Data Source=YourDatabaseServerName;Initial Catalog=YourDatabaseName;Integrated Security=SSPI;"
//                , "YourDatabaseTableToUploadTheFileDataInto");
//        }

//        /// &lt;summary&gt;
//        /// Process File - Rough code outline.
//        /// This will read an array of input files, process the lines in parallel, and upload
//        /// everything into the specified destination database table.
//        /// 
//        /// NOTE that this will have compile and runtime errors as it's just a 
//        /// rough code snippet to help you get started with your own implementation.
//        /// 
//        /// Think of it like a guide.
//        /// 
//        /// Code source from http://cc.davelozinski.com
//        /// &lt;/summary&gt;
//        /// &lt;param name="SourceFiles"&gt;Array containing the files to be read, including the full path.&lt;/param&gt;
//        /// &lt;param name="DatabaseConnectionString"&gt;Connection string to your database if you're going to upload data to it&lt;/param&gt;
//        /// &lt;param name="DestinationTable"&gt;The database table to upload the file information into&lt;/param&gt;
//        public static void ProcessFileTaskItem(
//            string[] SourceFiles, string DatabaseConnectionString, 
//            string DestinationTable
//            )
//        {
//            //Make sure there's files to read
//            if (SourceFiles != null && SourceFiles.Length > 0)
//            {
//                //Loop through the file array
//                //Depending on your system's set up, if it's fast enough you can
//                //implement this in a Parallel.For loop as well to read multiple
//                //files simultaneously.
//                //Parallel.For(0, SourceFiles.Length, x =&gt;
//                for (int x = 0; x < SourceFiles.Length; x++)
//                {
//                    //Make sure the file exists and if so open it for reading.
//                    if (File.Exists(SourceFiles[x]))
//                    {
//                        //Use Microsoft's built in TextFieldParser class for parsing CSV files.
//                        using (TextFieldParser tfp = new TextFieldParser(SourceFiles[x]))
//                        {
//                            //If you're going to upload the data in your files to a database, this is a good
//                            //place to open the connection string so it's only opened and used once instead of
//                            //opening/closing/opening/closing.
//                            //
//                            //If you're not uploading to a database, then obviously comment out or delete the
//                            //relevant lines of code.
//                            using (SqlConnection connectionDest = new SqlConnection(DatabaseConnectionString))
//                            {
//                                connectionDest.Open();

//                                //Configure everything to upload to the database via bulk copy.
//                                using (SqlBulkCopy sbc = new SqlBulkCopy(connectionDest, SqlBulkCopyOptions.TableLock, null))
//                                {
//                                    //Configure the bulk copy settings
//                                    sbc.DestinationTableName = DestinationTable;
//                                    sbc.BulkCopyTimeout = 28800; //8 hours

//                                    //Now read and process the file
//                                    ProcessAllLinesInInputFile(tfp, SourceFiles[x], connectionDest, sbc);
//                                }

//                                connectionDest.Close();

//                            } //using (SqlConnection connectionDest = new SqlConnection(DatabaseConnectionString))

//                            //If you're not uploading to a database then you can just uncomment and use this:
//                            //ProcessAllLinesInInputFile(tfp, SourceFiles[x]);

//                            tfp.Close();

//                        } //using (TextFieldParser tfp = new TextFieldParser(SourceFiles[x]))

//                    } //if File.Exists(SourceFiles[x])
//                    else
//                    {	//The file doesn't exist
//                        //Do whatever you need to here
//                    }
//                } //for
//                  //); //End Parallel reading of files

//                //Explicitly clean up before exiting
//                Array.Clear(SourceFiles, 0, SourceFiles.Length);
//            }
//            else
//            {
//                //Do whatever you need to here if SourceFiles == null || SourceFiles.Length == 0)
//            }
//        } //ProcessFileTaskItem

//        /// &lt;summary&gt;
//        /// Processes every line in the source input file.
//        /// 
//        /// NOTE that this will have compile and runtime errors as it's just a 
//        /// rough code snippet to help you get started with your own implementation.
//        /// 
//        /// Think of it like a guide.
//        /// 
//        /// Code source from http://cc.davelozinski.com
//        /// &lt;/summary&gt;
//        /// &lt;param name="tfp"&gt;The open textfieldparser used to read each line of the file&lt;/param&gt;
//        /// &lt;param name="SourceFile"&gt;The collection of information on the source file&lt;/param&gt;
//        /// &lt;param name="connectionDest"&gt;The open SQL Server connection to the destination SQL server for bulk copying&lt;/param&gt;
//        /// &lt;param name="sbc"&gt;The open SQL Server Bulk Copy object&lt;/param&gt;
//        private static void ProcessAllLinesInInputFile(TextFieldParser tfp, string SourceFile, SqlConnection connectionDest, SqlBulkCopy sbc)
//        {
//            //Put these here so new objects aren't created with each loop iteration

//            //The number of lines to read before processing all of them in parallel.
//            //You obviously could make this a parameter too.
//            int BatchSize = 50000;

//            //Will hold each line and each column of each line read
//            string[][] CurrentLines = new string[BatchSize][];

//            //Create a local data table. Should be the same name as the table
//            //in the database you'll be uploading everything to.
//            //Obviously this could be a parameter too.
//            DataTable CurrentRecords = new DataTable("YourDestinationTableNameInYourDatabase");

//            //The column names. They should match what's in the database table.
//            //If the column names are in the text file, you'll have to write your own routine 
//            //to read the first line of your file and extract/parse the column names to include here
//            //as I'm not doing everything for you unless you give me a good rate. ;-)
//            string[] ColumnNames = new string[] { "Column1", "Column2", "etc" };

//            //The number of records currently processed for SQL bulk copy
//            int BatchCount = 0;
//            //The total number of records processed. Could be used for logging purposes.
//            int RecordCount = 0;
//            //More lines to process in the file?
//            bool blnFileHasMoreLines = true;
//            //The number of lines read thus far
//            int intLineReadCounter = 0;
//            //used for thread locking purposes
//            object oSyncLock = new object();

//            //Could be used for logging and stat keeping purposes.
//            DateTime batchStartTime = DateTime.Now;
//            DateTime batchEndTime = DateTime.Now;
//            TimeSpan batchTimeSpan = batchEndTime - batchStartTime;

//            //Set the next line as appropriate if using a CSV and there's data enclused in quotes.
//            //tfp.HasFieldsEnclosedInQuotes = true;

//            //If the file is Delimited, set these values
//            tfp.TextFieldType = FieldType.Delimited;
//            tfp.Delimiters = new string[] { "," };

//            //If the file is FixedWidth, you'll have to write your own code. 
//            //tfp.TextFieldType = FieldType.FixedWidth;
//            //tfp.SetFieldWidths( );

//            //Create the datatable with the column names.
//            for (int x = 0; x < ColumnNames.Length; x++)
//                CurrentRecords.Columns.Add(ColumnNames[x], typeof(string));

//            //Of note: it's faster to read all the lines we are going to act on and 
//            //then process them in parallel instead of reading and processing line by line.
//            while (blnFileHasMoreLines)
//            {
//                batchStartTime = DateTime.Now;  //Reset the timer

//                //Read in all the lines up to the BatchCopy size or
//                //until there's no more lines in the file
//                while (intLineReadCounter < BatchSize && !tfp.EndOfData)
//                {
//                    CurrentLines[intLineReadCounter] = tfp.ReadFields();
//                    intLineReadCounter += 1;
//                    BatchCount += 1;
//                    RecordCount += 1;
//                }

//                batchEndTime = DateTime.Now;    //record the end time of the current batch
//                batchTimeSpan = batchEndTime - batchStartTime;  //get the timespan for stats

//                //Now process each line in parallel.
//                Parallel.For(0, intLineReadCounter, x =>
//                //for (int x=0; x &lt; intLineReadCounter; x++)    //Or the slower single threaded version for debugging
//                {
//                    List<object> values = null; //so each thread gets its own copy. 

//                    if (tfp.TextFieldType == FieldType.Delimited)
//                    {
//                        if (CurrentLines[x].Length != CurrentRecords.Columns.Count)
//                        {
//                            //Do what you need to if the number of columns in the current line
//                            //don't match the number of expected columns
//                            return; //stop now and don't add this record to the current collection of valid records.
//                        }

//                        //Number of columns match so copy over the values into the datatable
//                        //for later upload into a database
//                        values = new List<object>(CurrentRecords.Columns.Count);
//                        for (int i = 0; i < CurrentLines[x].Length; i++)
//                            values.Add(CurrentLines[x][i].ToString());

//                        //OR do your own custom processing here if not using a database.
//                    }
//                    else if (tfp.TextFieldType == FieldType.FixedWidth)
//                    {
//                        //Implement your own processing if the file columns are fixed width.
//                    }

//                    //Now lock the data table before saving the results so there's no thread bashing on the datatable
//                    lock (oSyncLock)
//                    {
//                        CurrentRecords.LoadDataRow(values.ToArray(), true);
//                    }

//                    values.Clear();

//                }
//                ); //Parallel.For   

//                //If you're not using a database, you obviously won't need this next piece of code.
//                if (BatchCount >= BatchSize)
//                {   //Do the SQL bulk copy and save the info into the database
//                    sbc.BatchSize = CurrentRecords.Rows.Count;
//                    sbc.WriteToServer(CurrentRecords);

//                    BatchCount = 0;         //Reset these values
//                    CurrentRecords.Clear(); //  "
//                }

//                if (CurrentLines[intLineReadCounter] == null)
//                    blnFileHasMoreLines = false;    //we're all done, so signal while loop to stop

//                intLineReadCounter = 0; //reset for next pass
//                Array.Clear(CurrentLines, 0, CurrentLines.Length);

//            } //while blnhasmorelines

//            //Write out the last of the good records to the database
//            if (CurrentRecords.Rows.Count > 0)
//            {
//                sbc.BatchSize = CurrentRecords.Rows.Count;
//                sbc.WriteToServer(CurrentRecords);
//            }

//            //Clean up
//            if (CurrentRecords != null)
//                CurrentRecords.Clear();
//            if (CurrentLines != null)
//                Array.Clear(CurrentLines, 0, CurrentLines.Length);
//            oSyncLock = null;
//        }

//    }
//}