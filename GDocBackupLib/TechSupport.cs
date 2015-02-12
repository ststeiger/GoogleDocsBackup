﻿using System;
using System.Collections.Generic;
using System.Text;
using Google.GData.Client;
using Google.Documents;
using System.IO;


namespace GDocBackupLib
{
    public class TechSupport
    {
        public static void ExportDocList(string outFolder, string username, string password)
        {
            GDataCredentials credentials = new GDataCredentials(username, password);
            RequestSettings settings = new RequestSettings("GDocBackup", credentials);
            settings.AutoPaging = true;
            settings.PageSize = 100;
            DocumentsRequest request = new DocumentsRequest(settings);

            Feed<Document> feed = request.GetEverything();
            List<Document> docs = new List<Document>();
            foreach (Document entry in feed.Entries)
                docs.Add(entry);

            using (StreamWriter outFile = new StreamWriter(Path.Combine(outFolder, "doclist.txt"), false),
                outFile2 = new StreamWriter(Path.Combine(outFolder, "doclistdetails.txt"), false))
            {
                foreach (Document doc in docs)
                {
                    string s = doc.Title + "\t" + doc.ResourceId;
                    outFile.WriteLine(s);
                    outFile2.WriteLine(s);
                    foreach (string pf in doc.ParentFolders)
                        outFile2.WriteLine("\t\t\t" + pf);
                }
                outFile.Close();
                outFile2.Close();
            }

        }
    }
}
