using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Atomus.Control.Toolbar
{
    public partial class FindForm : Form
    {
        int lastFound = 0;
        public RichTextBox RtbInstance { set; get; } = null;

        public string InitialText
        {
            set { txtSearchText.Text = value; }
            get { return txtSearchText.Text; }
        }

        public FindForm()
        {
            InitializeComponent();
            this.TopMost = true;
        }

        void rtbInstance_SelectionChanged(object sender, EventArgs e)
        {
            lastFound = RtbInstance.SelectionStart;
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.RtbInstance.SelectionChanged -= rtbInstance_SelectionChanged;
            this.Close();
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            RichTextBoxFinds options = RichTextBoxFinds.None;
            if (chkMatchCase.Checked) options |= RichTextBoxFinds.MatchCase;
            if (chkWholeWord.Checked) options |= RichTextBoxFinds.WholeWord;

            int index = RtbInstance.Find(txtSearchText.Text, lastFound, options);
            lastFound += txtSearchText.Text.Length;
            if (index >= 0)
            {
                RtbInstance.Parent.Focus();
            }
            else
            {
                MessageBox.Show("Search string not found", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lastFound = 0;
            }

        }

        private void FindForm_Load(object sender, EventArgs e)
        {
            if (RtbInstance != null)
                this.RtbInstance.SelectionChanged += new EventHandler(rtbInstance_SelectionChanged);
        }
    }
}
