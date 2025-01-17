//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Web.UI;
using System.Web;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System.Text;
using DotNetNuke.Framework;
using System.Runtime.InteropServices;

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class ControlPanel : ActiveAdminBase
    {
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cbShell.CallbackEvent += cbShell_Callback;
            cbModal.CallbackEvent += cbModal_Callback;
        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            IsCallBack = cbShell.IsCallback;

            btnReturn.ClientSideScript = "window.location.href = '" + Utilities.NavigateURL(TabId) + "';";
            cbModal.LoadingTemplate = GetLoadingTemplateSmall();
            Hashtable Settings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: ModuleId, tabId: TabId, ignoreCache: false).ModuleSettings;
            if (Convert.ToBoolean(Settings["AFINSTALLED"]) == false)
            {
                try
                {
                    var fc = new ForumsConfig();
                    bool configComplete = fc.ForumsInit(PortalId, ModuleId);
                    DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(ModuleId, "AFINSTALLED", configComplete.ToString());
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/ActiveForums/ControlPanel.css");
            ClientResourceManager.RegisterStyleSheet(Page, string.Concat("~/DesktopModules/ActiveForums/themes/", MainSettings.Theme, "/jquery-ui.min.css"));
            ClientResourceManager.RegisterStyleSheet(Page, filePath: $"{Globals.ModulePath}Resources/font-awesome-4.7.0/css/font-awesome.min.css", priority: 10);


            lblProd.Visible = true;
            lblCopy.Visible = true;
            //TODO: this should be resources instead of harcoded text?
            lblProd.Text = string.Concat("DNN Community Forums ", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            lblCopy.Text = string.Concat("&copy; ", DateTime.UtcNow.Year, " DNN Community");

            try
            {
                if (!Page.IsPostBack)
                {
                    GetControl(CurrentView, Params, IsCallBack);
                }

            }
            catch (Exception ex)
            {

                if (Request.QueryString["cptry"] == null)
                {
                    string sURL = EditUrl(string.Empty, string.Empty, "EDIT", "cptry=1");
                    Response.Redirect(sURL);
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
            }

            ClientResourceManager.RegisterScript(Page, "~/desktopmodules/activeforums/scripts/json2009.min.js", 101);
            ClientResourceManager.RegisterScript(Page, "~/desktopmodules/activeforums/scripts/jquery.history.js", 102);
            ClientResourceManager.RegisterScript(Page, "~/desktopmodules/activeforums/scripts/afadmin.js", 103);
            ClientResourceManager.RegisterScript(Page, "~/desktopmodules/activeforums/scripts/jquery.listreorder.js", 104);
            ClientResourceManager.RegisterScript(Page, "~/desktopmodules/activeforums/active/amlib.js", 105);

            string lang = "en-US";
            if (Request.QueryString["language"] != null)
            {
                lang = Request.QueryString["language"];
            }
            if (string.IsNullOrEmpty(lang))
            {
                lang = PortalSettings.DefaultLanguage;
            }
            if (string.IsNullOrEmpty(lang))
            {
                lang = "en-US";
            }
            string adminHandler = VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/handlers/adminhelper.ashx") + "?TabId=" + TabId.ToString() + "&PortalId=" + PortalId.ToString() + "&moduleid=" + ModuleId + "&language=" + lang;
            var sb = new StringBuilder();
            sb.AppendLine("var asScriptPath = '" + VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/scripts/") + "';");
            sb.AppendFormat("var afAdminHandlerURL = '{0}';", adminHandler);
            sb.AppendLine("var af_imgPath = '" + VirtualPathUtility.ToAbsolute(Globals.ModuleImagesPath) + "';");
            string sLoadImg;
            sLoadImg = "var afSpinLg = new Image();afSpinLg.src='" + VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/images/spinner-lg.gif") + "';";
            sLoadImg += "var afSpin = new Image();afSpin.src='" + VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/images/spinner.gif") + "';";
            sb.AppendLine(sLoadImg);
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "afscripts", sb.ToString(), true);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

        }
        public void cbShell_Callback(object sender, Controls.CallBackEventArgs e)
        {
            try
            {
                string sOptions = string.Empty;
                if (e.Parameters[1] != null)
                {
                    sOptions = e.Parameters[1];
                }
                GetControl(e.Parameters[0], sOptions, true);
                if (e.Parameters.Length != 3)
                {
                    var stringWriter = new System.IO.StringWriter();
                    var htmlWriter = new HtmlTextWriter(stringWriter);
                    plhControlPanel.RenderControl(e.Output);
                }

            }
            catch (Exception ex)
            {

                if (Request.QueryString["cptry"] == null)
                {
                    string sURL = EditUrl(string.Empty, string.Empty, "EDIT", "cptry=1");
                    Response.Redirect(sURL);
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
            }

        }
        private void cbModal_Callback(object sender, Controls.CallBackEventArgs e)
        {
            switch (e.Parameters[0].ToLowerInvariant())
            {
                case "load":
                    plhModal.Controls.Clear();
                    string ctlPath = string.Empty;
                    string ctrl = e.Parameters[1].ToLowerInvariant();
                    string ctlParams = e.Parameters[2].ToLowerInvariant();
                    LoadModal(ctrl, ctlParams);
                    break;
                case "clear":
                    plhModal.Controls.Clear();
                    break;
            }
            plhModal.RenderControl(e.Output);
        }
        #endregion
        #region Private Methods
        private void GetControl(string view, string options, bool IsCallback)
        {
            try
            {
                plhControlPanel.Controls.Clear();
                string ctlPath;
                if (view == "undefined")
                {
                    view = "home";
                }
                CurrentView = view;

                Params = options;
                ctlPath = string.Concat("~/DesktopModules/ActiveForums/controls/admin_", view, ".ascx");
                var ctl = (ActiveAdminBase)(LoadControl(ctlPath));
                ctl.ID = view;
                ctl.ModuleConfiguration = ModuleConfiguration;

                if (options != string.Empty)
                {
                    ctl.Params = options;
                }
                if (!(plhControlPanel.Controls.Contains(ctl)))
                {
                    plhControlPanel.Controls.Add(ctl);
                }
            }
            catch (Exception ex)
            {
                CurrentView = null;
                Params = null;

                if (Request.QueryString["cptry"] == null)
                {
                    string sURL = EditUrl(string.Empty, string.Empty, "EDIT", "cptry=1");
                    Response.Redirect(sURL);
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
            }


        }
        private void LoadModal(string ctrl, string @params = "")
        {
            plhModal.Controls.Clear();
            string ctlPath;

            ctlPath = string.Concat("~/DesktopModules/activeforums/controls/", ctrl, ".ascx");
            var ctl = (ActiveAdminBase)(LoadControl(ctlPath));
            ctl.ID = ctrl;
            ctl.ModuleConfiguration = ModuleConfiguration;

            if (@params != string.Empty)
            {
                ctl.Params = @params;
            }
            if (!(plhModal.Controls.Contains(ctl)))
            {
                plhModal.Controls.Add(ctl);
            }
        }
        #endregion

    }
}