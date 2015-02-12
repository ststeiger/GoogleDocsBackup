/*
   Copyright 2009-2012  Fabrizio Accatino

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GDocBackupLib;
using Google.Documents;
using System.Reflection;


namespace GDocBackupCMD
{
    class Program
    {
        static int Main(string[] args)
        {
            AssemblyName assembName = Assembly.GetExecutingAssembly().GetName();
            Console.WriteLine("\n" + assembName.Name + " - ver. " + assembName.Version.ToString() +
                " - " + "http://gs.fhtino.it/gdocbackup" + "\n\n");

            Dictionary<string, string> parameters = ParseParameters(args);
            if (!parameters.ContainsKey("mode"))
                parameters.Add("mode", "help");

            if (parameters["mode"] == "encodepassword")
            {
                if (parameters.ContainsKey("password"))
                {
                    string encodedpassword = GDocBackupLib.Utility.ProtectData(parameters["password"]);
                    if (parameters.ContainsKey("outfile"))
                    {
                        string outfile = parameters["outfile"];
                        File.WriteAllText(outfile, encodedpassword);
                        Console.WriteLine("Encoded password written to file " + outfile);
                    }
                    else
                    {
                        Console.WriteLine(encodedpassword);
                    }
                }
                return 0;
            }

            if (parameters["mode"] == "backup")
            {
                bool resOK;
                try
                {
                    resOK = DoBackup(parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("INTERNAL ERROR: " + ex.Message);
                    Console.WriteLine(new String('-', 40));
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(new String('-', 40));
                    resOK = false;
                }

                if (resOK)
                {
                    return 0;
                }
                else
                {
                    Console.WriteLine("\n\n****** WARNING: there are errrors! ******");
                    return 1;
                }
            }

            ShowUsage();
            return 0;
        }


        /// <summary>
        /// ...
        /// </summary>
        private static bool DoBackup(Dictionary<string, string> parameters)
        {

            // Google Apps
            bool appsMode = parameters.ContainsKey("appsMode") ? parameters["appsMode"] == "1" : false;
            string appsDomain = null;
            string appsOAuthSecret = null;
            bool appsOAuthOnly = false;
            if (appsMode)
            {
                appsDomain = parameters.ContainsKey("appsDomain") ? parameters["appsDomain"] : null;
                appsOAuthSecret = parameters.ContainsKey("appsOAuthSecret") ? parameters["appsOAuthSecret"] : null;
                appsOAuthOnly = parameters.ContainsKey("appsOAuthOnly") ? parameters["appsOAuthOnly"] == "1" : false;
                if (String.IsNullOrEmpty(appsDomain)) throw new ApplicationException("Empty appsDomain parameter");
                if (String.IsNullOrEmpty(appsOAuthSecret)) throw new ApplicationException("Empty appsOAuthSecret parameter");
            }

            // Get username
            string username = parameters.ContainsKey("username") ? parameters["username"] : null;
            if ((appsMode == false && username == null) ||
                (appsMode == true && appsOAuthOnly == false && username == null))
                throw new ApplicationException("Empty username parameter");

            // Get password
            string password = null;
            if (parameters.ContainsKey("password"))
                password = parameters["password"];
            if (parameters.ContainsKey("passwordEnc"))
                password = Utility.UnprotectData(parameters["passwordEnc"]);
            if (parameters.ContainsKey("passwordEncFile"))
                password = Utility.UnprotectData(File.ReadAllText(parameters["passwordEncFile"]));
            if ((appsMode == false && password == null) ||
                (appsMode == true && appsOAuthOnly == false && password == null))
                throw new ApplicationException("Empty password parameter");

            // Get destDir
            string destDir = parameters.ContainsKey("destDir") ? parameters["destDir"] : null;
            if (destDir == null)
                throw new ApplicationException("Empty destDir parameter");

            // Get document export format
            string docF = parameters.ContainsKey("docF") ? parameters["docF"] : null;
            string sprsF = parameters.ContainsKey("sprsF") ? parameters["sprsF"] : null;
            string presF = parameters.ContainsKey("presF") ? parameters["presF"] : null;
            string drawF = parameters.ContainsKey("drawF") ? parameters["drawF"] : null;
            if (docF == null) throw new ApplicationException("Empty docF parameter");
            if (sprsF == null) throw new ApplicationException("Empty sprsF parameter");
            if (presF == null) throw new ApplicationException("Empty presF parameter");
            if (drawF == null) throw new ApplicationException("Empty drawF parameter");
            List<Document.DownloadType> docTypes = Utility.DecodeDownloadTypeArray(docF, '+');
            List<Document.DownloadType> sprsTypes = Utility.DecodeDownloadTypeArray(sprsF, '+');
            List<Document.DownloadType> presTypes = Utility.DecodeDownloadTypeArray(presF, '+');
            List<Document.DownloadType> drawTypes = Utility.DecodeDownloadTypeArray(drawF, '+');
            string downloadAll = parameters.ContainsKey("downloadAll") ? parameters["downloadAll"] : null;

            // Get BypassHttpsCertChecks
            bool bypassHttpsCertChecks = parameters.ContainsKey("bypassHttpsCertChecks");

            // Output parameters
            Console.WriteLine(new String('-', 40));
            Console.WriteLine("Parameters: ");
            Console.WriteLine("Username:        " + username);
            Console.WriteLine("Password:        " + "[hidden]");
            Console.WriteLine("DestDir:         " + destDir);
            Console.WriteLine("Document:        " + docF);
            Console.WriteLine("Spreadsheet:     " + sprsF);
            Console.WriteLine("Presentation:    " + presF);
            Console.WriteLine("Drawing:         " + drawF);
            Console.WriteLine("DownloadAll:     " + downloadAll);
            Console.WriteLine("appsMode:        " + appsMode);
            Console.WriteLine("appsDomain:      " + appsDomain);
            Console.WriteLine("appsOAuthSecret: " + appsOAuthSecret);
            Console.WriteLine("appsOAuthOnly:   " + appsOAuthOnly);
            Console.WriteLine(new String('-', 40));

            // Exec backup
            Config config = new Config(
                username, password,
                destDir,
                downloadAll == "yes",
                docTypes.ToArray(), sprsTypes.ToArray(), presTypes.ToArray(), drawTypes.ToArray(),
                null,
                bypassHttpsCertChecks,
                false,
                null,
                appsMode,
                appsDomain,
                appsOAuthSecret,
                appsOAuthOnly);

            Backup backup = new Backup(config);
            backup.Feedback += new EventHandler<FeedbackEventArgs>(backup_Feedback);
            bool resOK = backup.Exec();

            return resOK;
        }


        /// <summary>
        /// ...
        /// </summary>
        private static void backup_Feedback(object sender, FeedbackEventArgs e)
        {
            int percent = (int)(e.PerCent * 100);
            Console.WriteLine("LOG> " + percent.ToString("000") + " : " + e.Message);
            if (e.FeedbackObj != null)
                Console.WriteLine("FBK> " + percent.ToString("000") + " : " + e.FeedbackObj.ToString());
        }


                /// <summary>
        /// ...
        /// </summary>
        private static void ShowUsage()
        {
            Console.WriteLine("Command line options: http://gs.fhtino.it/gdocbackup/quickguide-cmd");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        /// <summary>
        /// ...
        /// </summary>
        private static void ShowUsage_OLD()
        {
            Console.WriteLine("USAGE   (draft)");
            Console.WriteLine("");
            Console.WriteLine("GDocBackupCMD.exe -mode=backup|encodepassword");
            Console.WriteLine("");
            Console.WriteLine(">>> mode=backup <<<");
            Console.WriteLine("  -username: google username");
            Console.WriteLine("  -password: password (clear text)");
            Console.WriteLine("  -passwordEnc: encoded password");
            Console.WriteLine("  -passwordEncFile: file containing the encoded password");
            Console.WriteLine("  -destDir: path to the local destination directory");
            Console.WriteLine("  -docF : export format (for Documents)");
            Console.WriteLine("  -sprsF: export format (for Spreadsheets)");
            Console.WriteLine("  -presF: export format (for Presentations)");
            Console.WriteLine("  -drawF: export format (for Drawings)");
            Console.WriteLine("  -downloadall: if \"yes\" download all documents");
            Console.WriteLine("  -bypassHttpsCertChecks:  bypass the checks of https certificate (at your own risk!!!)");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine(">>> mode=encodepassword <<<");
            Console.WriteLine("  -password: string to be encoded");
            Console.WriteLine("  -outfile: output file name");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("");
            Console.WriteLine("GDocBackupCMD.exe -mode=backup -username=foo -password=bar -destDir=c:\\temp\\docs\\ -docF=pdf -sprsF=csv -presF=ppt");
            Console.WriteLine("");
            Console.WriteLine("GDocBackupCMD.exe -mode=backup -username=foo -password=bar -destDir=c:\\temp\\docs\\ -docF=pdf -sprsF=csv -presF=ppt -downloadall=yes");
            Console.WriteLine("");
            Console.WriteLine("GDocBackupCMD.exe -mode=backup -username=foo -passwordEncFile=pass.txt -destDir=c:\\temp\\docs\\ -docF=pdf -sprsF=csv -presF=ppt");
            Console.WriteLine("");
            Console.WriteLine("GDocBackupCMD.exe -mode=backup -bypassHttpsCertChecks -username=foo -passwordEncFile=pass.txt -destDir=c:\\temp\\docs\\ -docF=pdf -sprsF=csv -presF=ppt");
            Console.WriteLine("");
            Console.WriteLine("GDocBackupCMD.exe -mode=encodepassword -password=foo");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }


        /// <summary>
        /// ...
        /// </summary>
        private static Dictionary<string, string> ParseParameters(string[] args)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (string arg in args)
            {
                if (arg.Length > 1)
                {
                    if (arg[0] == '-')
                    {
                        string paramKey;
                        string paramValue;
                        int eqSignPosition = arg.IndexOf('=');
                        if (eqSignPosition != -1)
                        {
                            paramKey = arg.Substring(1, eqSignPosition - 1);
                            paramValue = arg.Substring(eqSignPosition + 1);
                        }
                        else
                        {
                            paramKey = arg.Substring(1);    // ignore the first char '-'                            
                            paramValue = null;
                        }
                        if (!parameters.ContainsKey(paramKey))
                            parameters.Add(paramKey, paramValue);
                    }
                }
            }
            return parameters;
        }

    }
}
