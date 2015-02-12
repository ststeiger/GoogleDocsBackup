
namespace GoogleDocsBackup
{


    public class FormPost
    {

        // http://www.codeproject.com/Tips/143208/How-to-Submit-Google-Docs-Form-by-using-Csharp
        public static void SubmitGoogleDoc()
        {
            //Use WebClient Class to submit a new entry
            System.Net.WebClient wc = new System.Net.WebClient();
            var keyval = new System.Collections.Specialized.NameValueCollection();
            keyval.Add("entry.0.single", "XXXX");
            keyval.Add("entry.1.single", "XXXX");
            
            keyval.Add("submit", "Submit");
            
            //Create an event for Submit Complete
            wc.UploadValuesCompleted += new System.Net.UploadValuesCompletedEventHandler(onUploadValuesCompleted);
            
            //Create Additional Headers  
            wc.Headers.Add("Origin", "https://docs.google.com");
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10");
            
            //Finally Submit the Form:
            wc.UploadValuesAsync(new System.Uri("https://docs.google.com/formResponse?formkey=XXXXX&ifq") 
                ,"POST"
                , keyval, System.Guid.NewGuid().ToString()
            );
        }


        //Submit Complete
        public static void onUploadValuesCompleted(object sender, System.Net.UploadValuesCompletedEventArgs e)
        {
            MsgBox("Finished");
        }


        public static void MsgBox(object obj)
        {
            if (obj != null)
                System.Windows.Forms.MessageBox.Show(obj.ToString());
            else
                System.Windows.Forms.MessageBox.Show("obj is NULL");
        }


    }


}
