using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.XPath;

using Microsoft.Build.Framework;

namespace SandcastleGui
{
    public partial class FrmMain : Form
    {
        private AddLogCallback m_AddLogCallback;
        private List<LanguageEntity> m_Languages;
        private ProjectFile m_project;
        private string m_projFileName;
        private SetEnableCallback m_SetEnableCallback;

        public FrmMain()
        {
            this.InitializeComponent();
            this.m_AddLogCallback = new AddLogCallback(this.AddLog);
            this.m_SetEnableCallback = new SetEnableCallback(this.SetBtnBuildEnable);
            this.ReadLanguage();
        }

        private void AddLog(string msg)
        {
            if (!this.rtbLog.InvokeRequired)
            {
                this.rtbLog.AppendText("\r\n" + msg);
                this.rtbLog.ScrollToCaret();
            }
            else
            {
                base.Invoke(this.m_AddLogCallback, new object[] { msg });
            }
        }

        private void btnAdd1_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int tag = Convert.ToInt32(button.Tag);
            ListBox listBoxByTag = this.GetListBoxByTag(tag);
            switch (tag)
            {
                case 1:
                case 3:
                    this.fileDialog.Filter = "Assembly (*.dll;*.exe)|*.dll;*.exe";
                    break;

                case 2:
                    this.fileDialog.Filter = "Comments file (*.xml)|*.xml";
                    break;
            }
            this.fileDialog.FileName = string.Empty;
            if (this.fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!listBoxByTag.Items.Contains(this.fileDialog.FileName))
                {
                    listBoxByTag.Items.Add(this.fileDialog.FileName);
                }
                else
                {
                    this.ShowError(this.fileDialog.FileName + " is already added.");
                }
            }
        }

        private void btnAddFolder1_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int tag = Convert.ToInt32(button.Tag);
            ListBox listBoxByTag = this.GetListBoxByTag(tag);
            if (this.folderDialog.ShowDialog() == DialogResult.OK)
            {
                string str = this.folderDialog.SelectedPath + @"\*.*";
                if (!listBoxByTag.Items.Contains(str))
                {
                    listBoxByTag.Items.Add(str);
                }
                else
                {
                    this.ShowError(str + " is already added.");
                }
            }
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (this.ValidateControls())
            {
                this.mniSave_Click(null, null);
                if (!string.IsNullOrEmpty(this.m_projFileName))
                {
                    MsBuildRunner runner = new MsBuildRunner {
                        ProjectFile = this.m_projFileName,
                        ProjectName = this.txtName.Text
                    };
                    runner.TargetStarted += new Microsoft.Build.Framework.TargetStartedEventHandler(this.TargetStartedEventHandler);
                    runner.MsgRaised += new BuildMessageEventHandler(this.BuildMsgHandler);
                    runner.PrjFinished += new Microsoft.Build.Framework.ProjectFinishedEventHandler(this.ProjectFinishedEventHandler);
                    Thread thread = new Thread(new ThreadStart(runner.Run));
                    try
                    {
                        thread.Start();
                    }
                    catch (Exception exception)
                    {
                        this.ShowError(exception.Message);
                    }
                    this.SetBtnBuildEnable(false);
                }
            }
        }

        private void btnDelete1_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int tag = Convert.ToInt32(button.Tag);
            ListBox listBoxByTag = this.GetListBoxByTag(tag);
            if (listBoxByTag.SelectedIndex != -1)
            {
                listBoxByTag.Items.RemoveAt(listBoxByTag.SelectedIndex);
            }
            else
            {
                this.ShowError("Please select an item to delete.");
            }
        }

        public void BuildMsgHandler(object sender, BuildMessageEventArgs e)
        {
            this.AddLog(e.Message);
        }

        private void ClearControls()
        {
            this.lstDll.Items.Clear();
            this.lstComments.Items.Clear();
            this.lstDependent.Items.Clear();
            this.txtName.Text = string.Empty;
            this.ckbChm.Checked = true;
            this.ckbHxs.Checked = false;
            this.ckbWeb.Checked = false;
            this.cmbTopicStyle.SelectedIndex = 0;
            if (this.cmbLanguages.Items.Count > 0)
            {
                this.cmbLanguages.SelectedIndex = 0;
            }
            this.rtbLog.Clear();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.ClearControls();
        }

        private void GetControlsValue()
        {
            this.m_project.Name = this.txtName.Text;
            this.m_project.LanguageId = this.m_Languages[this.cmbLanguages.SelectedIndex].ID;
            this.m_project.TopicStyle = this.cmbTopicStyle.Text;
            this.GetLstValues(this.m_project.Dlls, this.lstDll);
            this.GetLstValues(this.m_project.Comments, this.lstComments);
            this.GetLstValues(this.m_project.Dependents, this.lstDependent);
            this.m_project.HasChm = this.ckbChm.Checked;
            this.m_project.HasHxs = this.ckbHxs.Checked;
            this.m_project.HasWeb = this.ckbWeb.Checked;
        }

        private ListBox GetListBoxByTag(int tag)
        {
            switch (tag)
            {
                case 1:
                    return this.lstDll;

                case 2:
                    return this.lstComments;

                case 3:
                    return this.lstDependent;
            }
            return null;
        }

        private void GetLstValues(StringCollection strColl, ListBox lst)
        {
            strColl.Clear();
            foreach (string str in lst.Items)
            {
                strColl.Add(str);
            }
        }

        private void LoadProject()
        {
            this.m_project = new ProjectFile();
            this.m_project.Load(this.m_projFileName);
            this.SetControlsValue();
        }

        private void mniExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mniNew_Click(object sender, EventArgs e)
        {
            this.m_projFileName = string.Empty;
            this.ClearControls();
        }

        private void mniOpen_Click(object sender, EventArgs e)
        {
            if (this.openDialog.ShowDialog() == DialogResult.OK)
            {
                this.m_projFileName = this.openDialog.FileName;
                this.LoadProject();
            }
        }

        private void mniSave_Click(object sender, EventArgs e)
        {
            if (this.ValidateControls())
            {
                if (string.IsNullOrEmpty(this.m_projFileName) && (this.saveDialog.ShowDialog() == DialogResult.OK))
                {
                    this.m_projFileName = this.saveDialog.FileName;
                }
                this.SaveProject();
            }
        }

        public void ProjectFinishedEventHandler(object sender, ProjectFinishedEventArgs e)
        {
            MessageBox.Show("Build is finished.", "SandcastleGui");
            this.SetBtnBuildEnable(true);
        }

        private void ReadLanguage()
        {
            XPathDocument document = new XPathDocument(Environment.ExpandEnvironmentVariables(@"%DxRoot%\ProductionTools\ChmBuilder.config"));
            XPathNodeIterator iterator = document.CreateNavigator().Select("/configuration/languages/language");
            Regex regex = new Regex(@"^0x\S* ");
            this.cmbLanguages.Items.Clear();
            this.m_Languages = new List<LanguageEntity>();
            foreach (XPathNavigator navigator2 in iterator)
            {
                string attribute = navigator2.GetAttribute("id", string.Empty);
                string input = navigator2.GetAttribute("name", string.Empty);
                input = regex.Replace(input, "");
                this.m_Languages.Add(new LanguageEntity(int.Parse(attribute), input));
                this.cmbLanguages.Items.Add(attribute + " - " + input);
            }
            if (this.cmbLanguages.Items.Count > 0)
            {
                this.cmbLanguages.SelectedIndex = 0;
            }
        }

        private bool SaveProject()
        {
            if (!this.ValidateControls())
            {
                return false;
            }
            this.m_project = new ProjectFile();
            this.GetControlsValue();
            this.m_project.Save(this.m_projFileName);
            return true;
        }

        private void SetBtnBuildEnable(bool enable)
        {
            if (!this.btnBuild.InvokeRequired)
            {
                this.btnBuild.Enabled = enable;
            }
            else
            {
                base.Invoke(this.m_SetEnableCallback, new object[] { enable });
            }
        }

        private void SetControlsValue()
        {
            this.ClearControls();
            this.txtName.Text = this.m_project.Name;
            for (int i = 0; i < this.m_Languages.Count; i++)
            {
                if (this.m_Languages[i].ID == this.m_project.LanguageId)
                {
                    this.cmbLanguages.SelectedIndex = i;
                    break;
                }
            }
            this.cmbTopicStyle.Text = this.m_project.TopicStyle;
            this.SetLstValues(this.m_project.Dlls, this.lstDll);
            this.SetLstValues(this.m_project.Comments, this.lstComments);
            this.SetLstValues(this.m_project.Dependents, this.lstDependent);
            this.ckbChm.Checked = this.m_project.HasChm;
            this.ckbHxs.Checked = this.m_project.HasHxs;
            this.ckbWeb.Checked = this.m_project.HasWeb;
        }

        private void SetLstValues(StringCollection strColl, ListBox lst)
        {
            lst.Items.Clear();
            foreach (string str in strColl)
            {
                lst.Items.Add(str);
            }
        }

        private void ShowError(string errorMsg)
        {
            MessageBox.Show(errorMsg, "Error");
        }

        public void TargetStartedEventHandler(object sender, TargetStartedEventArgs e)
        {
            this.AddLog("Target:" + e.TargetName);
        }

        private bool ValidateControls()
        {
            if (this.lstDll.Items.Count == 0)
            {
                this.ShowError("Please add the assemblies to doc.");
                return false;
            }
            if (this.txtName.Text.Trim() == "")
            {
                this.ShowError("Please input Name");
                return false;
            }
            if (this.txtName.Text.Contains(" "))
            {
                this.ShowError("Name can't contain spaces.");
                return false;
            }
            if ((!this.ckbChm.Checked && !this.ckbHxs.Checked) && !this.ckbWeb.Checked)
            {
                this.ShowError("Please select default targets.");
                return false;
            }
            return true;
        }

        private delegate void AddLogCallback(string msg);

        private delegate void SetEnableCallback(bool enable);

        /// <summary>
        /// Go to the project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(lnkProjectSite.Text);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  " + "Reason: " + ex.Message,
                    "Sandcastle Example GUI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
