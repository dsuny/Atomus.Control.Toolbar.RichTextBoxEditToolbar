using Atomus.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Atomus.Control.Toolbar
{
    public enum RicherTextBoxToolStripGroups
    {
        SaveAndLoad = 0x1,
        FontNameAndSize = 0x2,
        BoldUnderlineItalic = 0x4,
        Alignment = 0x8,
        FontColor = 0x10,
        IndentationAndBullets = 0x20,
        Insert = 0x40,
        Zoom = 0x80
    }

    public partial class RichTextBoxEditToolbar : UserControl, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;

        private RichTextBox richTextBox;

        private bool IsMouseDown = false;

        private ImageList imageList;
        
        #region Init
        public RichTextBoxEditToolbar()
        {
            InitializeComponent();

            this.imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };

            try
            {
                this.imageList.ImageSize = this.GetAttributeSize("ImageSize");
            }
            catch (Exception _Exception)
            {
                DiagnosticsTool.MyTrace(_Exception);
                this.imageList.ImageSize = new Size(32, 32);
            }
        }
        #endregion

        #region Dictionary
        #endregion

        #region Spread
        #endregion

        #region IO
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            try
            {
                if (e.Action != "RichTextBox.Add" & e.Action != "RichTextBox.Remove")
                    this.beforeActionEventHandler?.Invoke(this, e);

                switch (e.Action)
                {
                    case "RichTextBox.Remove":
                        this.richTextBox = null;

                        return true;

                    case "RichTextBox.Add":
                        this.RichTextBox = (RichTextBox)e.Value;
                        this.richTextBox.HideSelection = false;

                        return true;

                    default:
                            return true;
                }
            }
            finally
            {
                if (e.Action != "RichTextBox.Add" & e.Action != "RichTextBox.Remove")
                    this.afterActionEventHandler?.Invoke(this, e);
            }
        }
        #endregion

        #region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private void DefaultMenu_Load(object sender, EventArgs e)
        {
            try
            {
                this.DoubleBuffered(true);

                // load system fonts
                foreach (FontFamily family in FontFamily.Families)
                {
                    this.tscmbFont.Items.Add(family.Name);
                }

                this.tscmbFont.SelectedItem = this.Font.FontFamily;

                this.tscmbFontSize.SelectedItem = this.Font.Size;

                this.tstxtZoomFactor.Text = Convert.ToString(richTextBox.ZoomFactor * 100);
                this.WordWrap.Checked = this.richTextBox.WordWrap;

                this.AddImageList(this, this.Save);
                this.AddImageList(this, this.Open);
                this.AddImageList(this, this.ChooseFont);
                this.AddImageList(this, this.Bold);
                this.AddImageList(this, this.Italic);
                this.AddImageList(this, this.Underline);
                this.AddImageList(this, this.AlignLeft);
                this.AddImageList(this, this.AlignCenter);
                this.AddImageList(this, this.AlignRight);
                this.AddImageList(this, this.FontColor);
                this.AddImageList(this, this.BackColor);
                this.AddImageList(this, this.WordWrap);
                this.AddImageList(this, this.Indent);
                this.AddImageList(this, this.Outdent);
                this.AddImageList(this, this.Bullets);
                this.AddImageList(this, this.InsertPicture);
                this.AddImageList(this, this.ZoomIn);
                this.AddImageList(this, this.ZoomOut);
                this.AddImageList(this, this.Find);
                this.AddImageList(this, this.Replace);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(exception);
            }
        }

        private void RtbDocument_MouseUp(object sender, MouseEventArgs e)
        {
            this.IsMouseDown = false;
        }
        private void RtbDocument_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.IsMouseDown = true;
        }

        private void rtbDocument_SelectionChanged(object sender, EventArgs e)
        {
            if (richTextBox?.SelectionFont != null)
            {
                Bold.Checked = richTextBox.SelectionFont.Bold;
                Italic.Checked = richTextBox.SelectionFont.Italic;
                Underline.Checked = richTextBox.SelectionFont.Underline;

                boldToolStripMenuItem.Checked = richTextBox.SelectionFont.Bold;
                italicToolStripMenuItem.Checked = richTextBox.SelectionFont.Italic;
                underlineToolStripMenuItem.Checked = richTextBox.SelectionFont.Underline;

                switch (richTextBox.SelectionAlignment)
                {
                    case HorizontalAlignment.Left:
                        AlignLeft.Checked = true;
                        AlignCenter.Checked = false;
                        AlignRight.Checked = false;

                        leftToolStripMenuItem.Checked = true;
                        centerToolStripMenuItem.Checked = false;
                        rightToolStripMenuItem.Checked = false;
                        break;

                    case HorizontalAlignment.Center:
                        AlignLeft.Checked = false;
                        AlignCenter.Checked = true;
                        AlignRight.Checked = false;

                        leftToolStripMenuItem.Checked = false;
                        centerToolStripMenuItem.Checked = true;
                        rightToolStripMenuItem.Checked = false;
                        break;

                    case HorizontalAlignment.Right:
                        AlignLeft.Checked = false;
                        AlignCenter.Checked = false;
                        AlignRight.Checked = true;

                        leftToolStripMenuItem.Checked = false;
                        centerToolStripMenuItem.Checked = false;
                        rightToolStripMenuItem.Checked = true;
                        break;
                }

                Bullets.Checked = richTextBox.SelectionBullet;
                bulletsToolStripMenuItem.Checked = richTextBox.SelectionBullet;

                tscmbFont.SelectedItem = richTextBox.SelectionFont.FontFamily.Name;

                if (tscmbFontSize.Items.Contains(richTextBox.SelectionFont.Size.ToString()))
                    tscmbFontSize.SelectedItem = richTextBox.SelectionFont.Size.ToString();
                else
                    tscmbFontSize.Text = richTextBox.SelectionFont.Size.ToString();
            }
        }

        #region Toolstrip items handling
        private void tsbtnSave_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Filter = "Rich text format|*.rtf";
                    dlg.FilterIndex = 0;
                    dlg.OverwritePrompt = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            richTextBox.SaveFile(dlg.FileName, RichTextBoxStreamType.RichText);
                        }
                        catch (IOException exc)
                        {
                            MessageBox.Show("Error writing file: \n" + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (ArgumentException exc_a)
                        {
                            MessageBox.Show("Error writing file: \n" + exc_a.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
        }

        private void tsbtnOpen_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Rich text format|*.rtf";
                    dlg.FilterIndex = 0;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            richTextBox.LoadFile(dlg.FileName, RichTextBoxStreamType.RichText);
                        }
                        catch (IOException exc)
                        {
                            MessageBox.Show("Error reading file: \n" + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (ArgumentException exc_a)
                        {
                            MessageBox.Show("Error reading file: \n" + exc_a.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                }
        }

        private void tscmbFont_Click(object sender, EventArgs e)
        {
            // font
            try
            {
                if (!(richTextBox?.SelectionFont == null))
                {
                    Font currentFont = richTextBox.SelectionFont;
                    FontFamily newFamily = new FontFamily(tscmbFont.SelectedItem.ToString());

                    richTextBox.SelectionFont = new Font(newFamily, currentFont.Size, currentFont.Style);

                    richTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tscmbFontSize_Click(object sender, EventArgs e)
        {
            // font size
            try
            {
                if (!(richTextBox?.SelectionFont == null))
                {
                    Font currentFont = richTextBox.SelectionFont;
                    float newSize;

                    if (this.tscmbFontSize.SelectedItem != null)
                        newSize = Convert.ToSingle(this.tscmbFontSize.SelectedItem.ToString());
                    else
                        newSize = Convert.ToSingle(richTextBox?.SelectionFont.Size);

                    richTextBox.SelectionFont = new Font(currentFont.FontFamily, newSize, currentFont.Style);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tscmbFontSize_TextChanged(object sender, EventArgs e)
        {
            // font size custom
            try
            {
                if (!(richTextBox?.SelectionFont == null) && !IsMouseDown)
                {
                    Font currentFont = richTextBox.SelectionFont;
                    float newSize = Convert.ToSingle(this.tscmbFontSize.Text);

                    richTextBox.SelectionFont = new Font(currentFont.FontFamily, newSize, currentFont.Style);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void btnChooseFont_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                using (FontDialog dlg = new FontDialog())
                {
                    if (richTextBox.SelectionFont != null) dlg.Font = richTextBox.SelectionFont;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        richTextBox.SelectionFont = dlg.Font;
                    }
                }
        }

        private void tsbtnBIU_Click(object sender, EventArgs e)
        {
            ImeMode ImeMode;

            // bold, italic, underline
            try
            {
                if (!(richTextBox?.SelectionFont == null))
                {
                    ImeMode = richTextBox.ImeMode;
                    richTextBox.ImeMode = ImeMode.Disable;
                    richTextBox.ImeMode = ImeMode;

                    Font currentFont = richTextBox.SelectionFont;
                    FontStyle newFontStyle = richTextBox.SelectionFont.Style;
                    string txt = (sender as ToolStripButton).Name;
                    if (txt.IndexOf("Bold") >= 0)
                        newFontStyle = richTextBox.SelectionFont.Style ^ FontStyle.Bold;
                    else if (txt.IndexOf("Italic") >= 0)
                        newFontStyle = richTextBox.SelectionFont.Style ^ FontStyle.Italic;
                    else if (txt.IndexOf("Underline") >= 0)
                        newFontStyle = richTextBox.SelectionFont.Style ^ FontStyle.Underline;

                    richTextBox.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);

                    richTextBox.Focus();

                    rtbDocument_SelectionChanged(richTextBox, null);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tsbtnAlignment_Click(object sender, EventArgs e)
        {
            // alignment: left, center, right
            try
            {
                string txt = (sender as ToolStripButton).Name;

                if (richTextBox != null)
                    if (txt.IndexOf("Left") >= 0)
                    {
                        richTextBox.SelectionAlignment = HorizontalAlignment.Left;
                        AlignLeft.Checked = true;
                        AlignCenter.Checked = false;
                        AlignRight.Checked = false;
                    }
                    else if (txt.IndexOf("Center") >= 0)
                    {
                        richTextBox.SelectionAlignment = HorizontalAlignment.Center;
                        AlignLeft.Checked = false;
                        AlignCenter.Checked = true;
                        AlignRight.Checked = false;
                    }
                    else if (txt.IndexOf("Right") >= 0)
                    {
                        richTextBox.SelectionAlignment = HorizontalAlignment.Right;
                        AlignLeft.Checked = false;
                        AlignCenter.Checked = false;
                        AlignRight.Checked = true;
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tsbtnFontColor_Click(object sender, EventArgs e)
        {
            // font color
            try
            {
                if (richTextBox != null)
                    using (ColorDialog dlg = new ColorDialog())
                    {
                        dlg.Color = richTextBox.SelectionColor;
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            richTextBox.SelectionColor = dlg.Color;

                            Graphics graphics;
                            Image image;

                            image = FontColor.Image;

                            graphics = Graphics.FromImage(image);

                            graphics.FillRectangle(new SolidBrush(richTextBox.SelectionColor), 6, 27, 20, 5);

                            graphics.Dispose();

                            //tsbtnFontColor.Image = image;
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            // font color
            try
            {
                if (richTextBox != null)
                    using (ColorDialog dlg = new ColorDialog())
                    {
                        dlg.Color = richTextBox.SelectionColor;
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            richTextBox.SelectionBackColor = dlg.Color;

                            Graphics graphics;
                            Image image;

                            image = BackColor.Image;

                            graphics = Graphics.FromImage(image);

                            graphics.FillRectangle(new SolidBrush(richTextBox.SelectionBackColor), 6, 27, 20, 5);

                            graphics.Dispose();
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tsbtnWordWrap_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                richTextBox.WordWrap = WordWrap.Checked;
        }

        private void tsbtnBulletsAndNumbering_Click(object sender, EventArgs e)
        {
            // bullets, indentation
            try
            {
                string name = (sender as ToolStripButton).Name;

                if (richTextBox != null)
                    if (name.IndexOf("Bullets") >= 0)
                        richTextBox.SelectionBullet = Bullets.Checked;
                    else if (name.IndexOf("Indent") >= 0)
                        richTextBox.SelectionIndent += this.INDENT;
                    else if (name.IndexOf("Outdent") >= 0)
                        richTextBox.SelectionIndent -= this.INDENT;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tsbtnInsertPicture_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Insert picture";
                    dlg.DefaultExt = "jpg";
                    dlg.Filter = "Bitmap Files|*.bmp|JPEG Files|*.jpg|GIF Files|*.gif|All files|*.*";
                    dlg.FilterIndex = 1;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string strImagePath = dlg.FileName;
                            Image img = Image.FromFile(strImagePath);
                            Clipboard.SetDataObject(img);
                            DataFormats.Format df;
                            df = DataFormats.GetFormat(DataFormats.Bitmap);
                            if (this.richTextBox.CanPaste(df))
                            {
                                this.richTextBox.Paste(df);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Unable to insert image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                if (richTextBox.ZoomFactor < 64.0f - 0.20f)
                {
                    richTextBox.ZoomFactor += 0.20f;
                    tstxtZoomFactor.Text = String.Format("{0:F0}", richTextBox.ZoomFactor * 100);
                }
        }

        private void tsbtnZoomOut_Click(object sender, EventArgs e)
        {
            if (richTextBox != null)
                if (richTextBox.ZoomFactor > 0.16f + 0.20f)
                {
                    richTextBox.ZoomFactor -= 0.20f;
                    tstxtZoomFactor.Text = String.Format("{0:F0}", richTextBox.ZoomFactor * 100);
                }
        }

        private void tstxtZoomFactor_Leave(object sender, EventArgs e)
        {
            try
            {
                if (richTextBox != null)
                    richTextBox.ZoomFactor = Convert.ToSingle(tstxtZoomFactor.Text) / 100;
            }
            catch (FormatException)
            {
                MessageBox.Show("Enter valid number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tstxtZoomFactor.Focus();
                tstxtZoomFactor.SelectAll();
            }
            catch (OverflowException)
            {
                MessageBox.Show("Enter valid number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tstxtZoomFactor.Focus();
                tstxtZoomFactor.SelectAll();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Zoom factor should be between 20% and 6400%", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tstxtZoomFactor.Focus();
                tstxtZoomFactor.SelectAll();
            }
        }
        #endregion

        #region Find and Replace
        private void tsbtnFind_Click(object sender, EventArgs e)
        {
            FindForm findForm = new FindForm();
            findForm.RtbInstance = this.richTextBox;
            findForm.InitialText = this.tstxtSearchText.Text;
            findForm.Show();
        }

        private void tsbtnReplace_Click(object sender, EventArgs e)
        {
            ReplaceForm replaceForm = new ReplaceForm();
            replaceForm.RtbInstance = this.richTextBox;
            replaceForm.InitialText = this.tstxtSearchText.Text;
            replaceForm.Show();
        }
        #endregion

        private void RichTextBoxEditToolbar_DockChanged(object sender, EventArgs e)
        {
            this.toolStrip1.Dock = this.Dock;

            //if (this.toolStrip1.Dock == DockStyle.Left || this.toolStrip1.Dock == DockStyle.Right)
            //{
            //    this.toolStrip1.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
            //    this.toolStrip1.Width = 50;
            //}
        }
        private void toolStrip1_SizeChanged(object sender, EventArgs e)
        {
            this.Size = new Size(this.toolStrip1.Width, this.toolStrip1.Height - 2);
        }
        #endregion

        #region "ETC"

        #region Settings
        [Category("Settings")]
        [Description("Value indicating the number of characters used for indentation")]
        public int INDENT { get; set; } = 10;

        [Category("Settings")]
        [Description("RichTextBox")]
        public RichTextBox RichTextBox
        {
            set
            {
                this.richTextBox = value;

                this.richTextBox.AcceptsTab = true;
                this.richTextBox.ContextMenuStrip = this.contextMenu;
                //this.rtbDocument.Dock = System.Windows.Forms.DockStyle.Fill;
                this.richTextBox.EnableAutoDragDrop = true;
                //this.rtbDocument.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                //this.rtbDocument.Location = new System.Drawing.Point(0, 51);
                //this.rtbDocument.Name = "rtbDocument";
                //this.rtbDocument.Size = new System.Drawing.Size(667, 262);
                //this.rtbDocument.TabIndex = 0;
                //this.rtbDocument.Text = "";
                this.richTextBox.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
                this.richTextBox.SelectionChanged += new System.EventHandler(this.rtbDocument_SelectionChanged);

                //this.rtbDocument.MouseDown += RtbDocument_MouseDown;
                this.richTextBox.MouseUp += RtbDocument_MouseUp;
                this.richTextBox.MouseMove += RtbDocument_MouseMove;
            }
            get
            {
                return this.richTextBox;
            }
        }
        #endregion

        #region Properties for toolstrip items visibility
        [Category("Toolstip items visibility")]
        public bool GroupSaveAndLoadVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.SaveAndLoad); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.SaveAndLoad, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupFontNameAndSizeVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.FontNameAndSize); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.FontNameAndSize, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupBoldUnderlineItalicVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.BoldUnderlineItalic); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.BoldUnderlineItalic, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupAlignmentVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.Alignment); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.Alignment, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupFontColorVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.FontColor); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.FontColor, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupIndentationAndBulletsVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.IndentationAndBullets); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.IndentationAndBullets, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupInsertVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.Insert); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.Insert, value); }
        }
        [Category("Toolstip items visibility")]
        public bool GroupZoomVisible
        {
            get { return IsGroupVisible(RicherTextBoxToolStripGroups.Zoom); }
            set { HideToolstripItemsByGroup(RicherTextBoxToolStripGroups.Zoom, value); }
        }
        [Category("Toolstip items visibility")]
        public bool ToolStripVisible
        {
            get { return toolStrip1.Visible; }
            set { toolStrip1.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SaveVisible
        {
            get { return Save.Visible; }
            set { Save.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool LoadVisible
        {
            get { return Open.Visible; }
            set { Open.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorSaveLoadVisible
        {
            get { return toolStripSeparator6.Visible; }
            set { toolStripSeparator6.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool FontFamilyVisible
        {
            get { return tscmbFont.Visible; }
            set { tscmbFont.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool FontSizeVisible
        {
            get { return tscmbFontSize.Visible; }
            set { tscmbFontSize.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool ChooseFontVisible
        {
            get { return ChooseFont.Visible; }
            set { ChooseFont.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorFontVisible
        {
            get { return toolStripSeparator1.Visible; }
            set { toolStripSeparator1.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool BoldVisible
        {
            get { return Bold.Visible; }
            set { Bold.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool ItalicVisible
        {
            get { return Italic.Visible; }
            set { Italic.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool UnderlineVisible
        {
            get { return Underline.Visible; }
            set { Underline.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorBoldUnderlineItalicVisible
        {
            get { return toolStripSeparator2.Visible; }
            set { toolStripSeparator2.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool AlignLeftVisible
        {
            get { return AlignLeft.Visible; }
            set { AlignLeft.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool AlignRightVisible
        {
            get { return AlignRight.Visible; }
            set { AlignRight.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool AlignCenterVisible
        {
            get { return AlignCenter.Visible; }
            set { AlignCenter.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorAlignVisible
        {
            get { return toolStripSeparator3.Visible; }
            set { toolStripSeparator3.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool FontColorVisible
        {
            get { return FontColor.Visible; }
            set { FontColor.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool WordWrapVisible
        {
            get { return WordWrap.Visible; }
            set { WordWrap.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorFontColorVisible
        {
            get { return toolStripSeparator4.Visible; }
            set { toolStripSeparator4.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool IndentVisible
        {
            get { return Indent.Visible; }
            set { Indent.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool OutdentVisible
        {
            get { return Outdent.Visible; }
            set { Outdent.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool BulletsVisible
        {
            get { return Bullets.Visible; }
            set { Bullets.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorIndentAndBulletsVisible
        {
            get { return toolStripSeparator5.Visible; }
            set { toolStripSeparator5.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool InsertPictureVisible
        {
            get { return InsertPicture.Visible; }
            set { InsertPicture.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool SeparatorInsertVisible
        {
            get { return toolStripSeparator7.Visible; }
            set { toolStripSeparator7.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool ZoomInVisible
        {
            get { return ZoomIn.Visible; }
            set { ZoomIn.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool ZoomOutVisible
        {
            get { return ZoomOut.Visible; }
            set { ZoomOut.Visible = value; }
        }
        [Category("Toolstrip single items visibility")]
        public bool ZoomFactorTextVisible
        {
            get { return tstxtZoomFactor.Visible; }
            set { tstxtZoomFactor.Visible = value; }
        }
        #endregion

        #region data properties
        [Category("Document data")]
        [Description("RicherTextBox content in plain text")]
        [Browsable(true)]
        public override string Text
        {
            get { return richTextBox?.Text; }
            set { richTextBox.Text = value; }
        }
        [Category("Document data")]
        [Description("RicherTextBox content in rich-text format")]
        public string Rtf
        {
            get { return richTextBox?.Rtf; }
            set
            {
                try
                {
                    if (richTextBox != null)
                        richTextBox.Rtf = value ?? "";
                }
                catch (ArgumentException)
                {
                    if (richTextBox != null)
                        richTextBox.Text = value ?? "";
                }
            }
        }
        #endregion

        #region Changing visibility of toolstrip items

        public void HideToolstripItemsByGroup(RicherTextBoxToolStripGroups group, bool visible)
        {
            if ((group & RicherTextBoxToolStripGroups.SaveAndLoad) != 0)
            {
                Save.Visible = visible;
                Open.Visible = visible;
                toolStripSeparator6.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.FontNameAndSize) != 0)
            {
                tscmbFont.Visible = visible;
                tscmbFontSize.Visible = visible;
                ChooseFont.Visible = visible;
                toolStripSeparator1.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.BoldUnderlineItalic) != 0)
            {
                Bold.Visible = visible;
                Italic.Visible = visible;
                Underline.Visible = visible;
                toolStripSeparator2.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.Alignment) != 0)
            {
                AlignLeft.Visible = visible;
                AlignRight.Visible = visible;
                AlignCenter.Visible = visible;
                toolStripSeparator3.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.FontColor) != 0)
            {
                FontColor.Visible = visible;
                WordWrap.Visible = visible;
                toolStripSeparator4.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.IndentationAndBullets) != 0)
            {
                Indent.Visible = visible;
                Outdent.Visible = visible;
                Bullets.Visible = visible;
                toolStripSeparator5.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.Insert) != 0)
            {
                InsertPicture.Visible = visible;
                toolStripSeparator7.Visible = visible;
            }
            if ((group & RicherTextBoxToolStripGroups.Zoom) != 0)
            {
                ZoomOut.Visible = visible;
                ZoomIn.Visible = visible;
                tstxtZoomFactor.Visible = visible;
            }
        }

        public bool IsGroupVisible(RicherTextBoxToolStripGroups group)
        {
            switch (group)
            {
                case RicherTextBoxToolStripGroups.SaveAndLoad:
                    return Save.Visible && Open.Visible && toolStripSeparator6.Visible;

                case RicherTextBoxToolStripGroups.FontNameAndSize:
                    return tscmbFont.Visible && tscmbFontSize.Visible && ChooseFont.Visible && toolStripSeparator1.Visible;

                case RicherTextBoxToolStripGroups.BoldUnderlineItalic:
                    return Bold.Visible && Italic.Visible && Underline.Visible && toolStripSeparator2.Visible;

                case RicherTextBoxToolStripGroups.Alignment:
                    return AlignLeft.Visible && AlignRight.Visible && AlignCenter.Visible && toolStripSeparator3.Visible;

                case RicherTextBoxToolStripGroups.FontColor:
                    return FontColor.Visible && WordWrap.Visible && toolStripSeparator4.Visible;

                case RicherTextBoxToolStripGroups.IndentationAndBullets:
                    return Indent.Visible && Outdent.Visible && Bullets.Visible && toolStripSeparator5.Visible;

                case RicherTextBoxToolStripGroups.Insert:
                    return InsertPicture.Visible && toolStripSeparator7.Visible;

                case RicherTextBoxToolStripGroups.Zoom:
                    return ZoomOut.Visible && ZoomIn.Visible && tstxtZoomFactor.Visible;

                default:
                    return false;
            }
        }
        #endregion

        #region Public methods for accessing the functionality of the RicherTextBox

        public void SetFontFamily(FontFamily family)
        {
            if (family != null)
            {
                tscmbFont.SelectedItem = family.Name;
            }
        }

        public void SetFontSize(float newSize)
        {
            tscmbFontSize.Text = newSize.ToString();
        }

        public void ToggleBold()
        {
            Bold.PerformClick();
        }

        public void ToggleItalic()
        {
            Italic.PerformClick();
        }

        public void ToggleUnderline()
        {
            Underline.PerformClick();
        }

        public void SetAlign(HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    AlignCenter.PerformClick();
                    break;

                case HorizontalAlignment.Left:
                    AlignLeft.PerformClick();
                    break;

                case HorizontalAlignment.Right:
                    AlignRight.PerformClick();
                    break;
            }
        }

        //public void Indent()
        //{
        //    tsbtnIndent.PerformClick();
        //}

        //public void Outdent()
        //{
        //    tsbtnOutdent.PerformClick();
        //}

        //public void ToggleBullets()
        //{
        //    Bullets.PerformClick();
        //}

        //public void ZoomIn()
        //{
        //    tsbtnZoomIn.PerformClick();
        //}

        //public void ZoomOut()
        //{
        //    tsbtnZoomOut.PerformClick();
        //}

        //public void ZoomTo(float factor)
        //{
        //    if (richTextBox != null)
        //        richTextBox.ZoomFactor = factor;
        //}

        //public void SetWordWrap(bool activated)
        //{
        //    if (richTextBox != null)
        //        richTextBox.WordWrap = activated;
        //}
        #endregion

        #region Context menu handlers
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Clear();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.SelectAll();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Redo();
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlignLeft.PerformClick();

            leftToolStripMenuItem.Checked = true;
            centerToolStripMenuItem.Checked = false;
            rightToolStripMenuItem.Checked = false;


        }

        private void centerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlignCenter.PerformClick();

            leftToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = true;
            rightToolStripMenuItem.Checked = false;
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlignRight.PerformClick();

            leftToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = false;
            rightToolStripMenuItem.Checked = true;
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bold.PerformClick();
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Italic.PerformClick();
        }

        private void underlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Underline.PerformClick();
        }

        private void increaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Indent.PerformClick();
        }

        private void decreaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Outdent.PerformClick();
        }

        private void bulletsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bullets.PerformClick();
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomIn.PerformClick();
        }

        private void zoomOuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomOut.PerformClick();
        }

        private void insertPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsertPicture.PerformClick();
        }

        #endregion


        private void AddImageList(ICore core, ToolStripButton button)
        {
            string tmp;

            try
            {
                tmp = string.Format("{0}.{1}", button.Name, "Image");

                switch (tmp)
                {
                    case "AlignCenter.Image": button.Image = Properties.Resources.center; break;
                    case "AlignLeft.Image": button.Image = Properties.Resources.left; break;
                    case "AlignRight.Image": button.Image = Properties.Resources.right; break;
                    case "BackColor.Image": button.Image = Properties.Resources.backcolor; break;
                    case "Bold.Image": button.Image = Properties.Resources.bold; break;
                    case "Bullets.Image": button.Image = Properties.Resources.bullets; break;
                    case "ChooseFont.Image": button.Image = Properties.Resources.font; break;
                    case "Find.Image": button.Image = Properties.Resources.find; break;
                    case "FontColor.Image": button.Image = Properties.Resources.forecolor; break;
                    case "Indent.Image": button.Image = Properties.Resources.indent; break;
                    case "InsertPicture.Image": button.Image = Properties.Resources.picture; break;
                    case "Italic.Image": button.Image = Properties.Resources.italic; break;
                    case "Open.Image": button.Image = Properties.Resources.open; break;
                    case "Outdent.Image": button.Image = Properties.Resources.outdent; break;
                    case "Replace.Image": button.Image = Properties.Resources.replace; break;
                    case "Save.Image": button.Image = Properties.Resources.save; break;
                    case "Underline.Image": button.Image = Properties.Resources.underline; break;
                    case "WordWrap.Image": button.Image = Properties.Resources.wordwrap; break;
                    case "ZoomIn.Image": button.Image = Properties.Resources.zoomin; break;
                    case "ZoomOut.Image": button.Image = Properties.Resources.zoomout; break;
                }
                //button.Image = await core.GetAttributeWebImage(tmp);

            }
            catch (Exception exception)
            {
                DiagnosticsTool.MyTrace(exception);
            }
        }
        
        private void RemoveImageList(ICore core, string name)
        {
            string _Tmp;
            string _Key;

            try
            {
                _Tmp = string.Format("{0}.{1}", name, "Image");
                _Key = string.Format("{0}.{1}", ((System.Windows.Forms.Control)core).Name, _Tmp);
                this.imageList.Images.RemoveByKey(_Key);
            }
            catch (Exception _Exception)
            {
                DiagnosticsTool.MyTrace(_Exception);
            }

            try
            {
                _Tmp = string.Format("{0}.{1}", name, "ImageOn");
                _Key = string.Format("{0}.{1}", ((System.Windows.Forms.Control)core).Name, _Tmp);
                this.imageList.Images.RemoveByKey(_Key);
            }
            catch (Exception _Exception)
            {
                DiagnosticsTool.MyTrace(_Exception);
            }
        }
        #endregion

    }
}
