
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace GoogleDocsBackup
{


    public partial class frmMain : Form
    {


        public frmMain()
        {
            InitializeComponent();
        }


        private void btnSubmitForm_Click(object sender, EventArgs e)
        {
            FormPost.SubmitGoogleDoc();
        }


    }


}
