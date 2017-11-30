using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.SqlServer.XEvent.Linq;

// Context: https://blogs.msdn.microsoft.com/extended_events/2011/07/20/introducing-the-extended-events-reader/

namespace XelGrepConsole
{
    class Program
    {
        static int TotalOutputEvents { get; set;  }

        static void OutputPublishedEvent(PublishedEvent @event) /* event is reserved C# keyword */
        {
            TotalOutputEvents++;

            Console.WriteLine("----------------------------------------------------------------------------------");

            foreach (PublishedEventField field in @event.Fields)
            {
                var fieldName = field.Name;

                switch (fieldName)
                {
                  //  case "statement": // string
                                      // 
                 //       break;
                
                 //   case "affected_rows": // int
                                          // 
                 //       break;

                  //  case "server_principal_name": // string
                                                  // Value	session_server_principal_name => findbadmin
                                                  //          server_principal_name => findbadmin
                    //    var sUser = field.Value as string;
                    //    if (sUser != null)
                     //   {
                      //      var saveConsoleForegroundColor = Console.ForegroundColor;
                       //     Console.ForegroundColor = ConsoleColor.Green;
                     //       Console.WriteLine($"server_principal_name: {sUser}");
                     //       Console.ForegroundColor = saveConsoleForegroundColor;
                     //   }
                     //   break;

                 //   case "database_principal_name": // string
                                                    // Value	"ReaderWriterNoMask"	object {string}
                   //     var dbUser = field.Value as string;
                     //   if (dbUser != null)
                     //   {
                     //       var saveConsoleForegroundColor = Console.ForegroundColor;
                     //       Console.ForegroundColor = ConsoleColor.Green;
                     //       Console.WriteLine($"database_principal_name: {dbUser}");
                     //       Console.ForegroundColor = saveConsoleForegroundColor;
                     //   }
                     //   break;

                    default:
                        if (field.Value != null)
                        {
                            var valueAsString = String.Empty;
                            if (field.Value as String != null) valueAsString = (String)field.Value;
                            else if (field.Value as DateTimeOffset? != null) valueAsString = (((DateTimeOffset?)field.Value).HasValue ? Convert.ToString(((DateTimeOffset?)field.Value).Value) : "[DateTimeOffset? that has not value]");
                            else if (field.Value as Boolean? != null) valueAsString = (((Boolean?)field.Value).HasValue ? Convert.ToString(((Boolean?)field.Value).Value) : "[Boolean? that has not value]");
                            else if (field.Value as Int16? != null) valueAsString = (((Int16?)field.Value).HasValue ? Convert.ToString(((Int16?)field.Value).Value) : "[Int16? that has not value]");
                            else if (field.Value as Int32? != null) valueAsString = (((Int32?)field.Value).HasValue ? Convert.ToString(((Int32?)field.Value).Value) : "[Int32? that has not value]");
                            else if (field.Value as Int64? != null) valueAsString = (((Int64?)field.Value).HasValue ? Convert.ToString(((Int64?)field.Value).Value) : "[Int64? that has not value]");
                            else if (field.Value as UInt64? != null) valueAsString = (((UInt64?)field.Value).HasValue ? Convert.ToString(((UInt64?)field.Value).Value) : "[UInt64? that has not value]");
                            else if (field.Value as Guid? != null) valueAsString = (((Guid?)field.Value).HasValue ? Convert.ToString(((Guid?)field.Value).Value) : "[Guid? that has not value]");
                            else Console.WriteLine($"{field.Name} is type {field.Type}");
                            if (valueAsString != String.Empty) Console.WriteLine($"{field.Name}: {valueAsString}");
                            break;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Outputs records that match the search.
        /// 
        /// Usage: xelgrep filepath string1 [string2 [string3]] [-v stringN]
        /// - filepath - can be a single file or a wildcard
        /// - up to 3 search strings can be provided
        /// - v stringN - excludes records matching stringN (similar to grep -v, but only applies to individual records)
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var saveConsoleForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("usage: xelgrep directory-or-path");
                Console.ForegroundColor = saveConsoleForegroundColor;
                if (System.Diagnostics.Debugger.IsAttached) Console.ReadKey();
                return;
            }

            try
            {
#if false
                string[] xelFiles = { @"c:\dev\*.xel" };
#else
                var xelFiles = new string[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    if (Directory.Exists(args[i]))
                    {
                        var path = Path.Combine(args[i], "*.xel");
                        Console.WriteLine($"turned {args[i]} and *.xel into [{path}]");
                        xelFiles[i] = path;
                    }
                    else // assume a file or wildcard
                    {
                        xelFiles[i] = args[i];
                    }
                }
#endif
                Console.WriteLine("xelFiles: ");
                foreach (var xel in xelFiles) Console.WriteLine($"\t{xel}");

                QueryableXEventData events = new QueryableXEventData(xelFiles);

                var minAffectedRows = 1;
                var noiseServerUser = "ReaderWriterNoMask";
                var killString = "exec sp_executesql N'SELECT  COUNT(*)";
                var statementKeyword1 = "DistributionPortalRoleMap";
                var statementKeyword2 = "DELETE";

                var eventNum = 0;

                foreach (PublishedEvent @event in events)
                {
                    eventNum++;
                    var includeWithOutput = false;
                    var excludeFromOutput = false;

                    foreach (PublishedEventField fld in @event.Fields)
                    {
                        var fieldName = fld.Name;

                        switch (fieldName)
                        {
                            case "statement": // string
                                if ((fld.Value as string).Contains(killString))
                                {
                                    excludeFromOutput = true;
                                    break; ;
                                }
                                else if ((fld.Value as string).Contains(statementKeyword1)
                                     && (fld.Value as string).Contains(statementKeyword2))
                                {
                                    includeWithOutput = true;
                                }
                                break;

                            case "affected_rows": // int
                                if ((fld.Value as int? != null) && ((int?)fld.Value).Value < minAffectedRows)
                                {
                                    excludeFromOutput = true;
                                    break;
                                }
                                break;

                            case "server_principal_name": // string
                                // Value	session_server_principal_name => findbadmin
                                //          server_principal_name => findbadmin
                                var sUser = fld.Value as string;
                                if (sUser == noiseServerUser)
                                {
                                    // Console.WriteLine($"\t\t WILL BE not SHOWING THIS DB USER => {sUser} [{eventNum}]");

                                    excludeFromOutput = true;
                                    break;
                                }
                                else
                                {
                                    //Console.WriteLine($"\t\t *MIGHT* BE SHOWING THIS DB USER => {sUser} [{eventNum}]");
                                    var x = sUser;
                                    //includeWithOutput = true;
                                }
                                break;

                            default:
                                // do nothing - though might choose to do something more when outputting content
                                break;
                        }
                    }

                    if (!excludeFromOutput && includeWithOutput)
                    {
                        OutputPublishedEvent(@event);
                    }
                }

                Console.WriteLine($"Done outputting ({TotalOutputEvents}/{eventNum} event messages.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetBaseException().Message} ({ex})");
            }

            if (System.Diagnostics.Debugger.IsAttached) Console.ReadKey();
        }
    }
}
