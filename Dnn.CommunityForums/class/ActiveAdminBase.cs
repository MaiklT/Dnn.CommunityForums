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
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.ActiveForums
{
    public class ActiveAdminBase : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        private string _currentView = string.Empty;
        private DateTime _CacheUpdatedTime;
        public const string RequiredImage = Globals.ModulePath + "images/error.gif";
        
        #region Constants
        internal const string ViewKey = "afcpView";
        internal const string ParamKey = "afcpParams";
        internal const string DefaultView = "home";
        #endregion

        public string Params { get; set; } = string.Empty;
        public bool IsCallBack { get; set; }

        public string HostURL
        {
            get
            {
                object obj = DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.HostUrl, ModuleId));
                if (obj == null)
                {
                    string sURL;
                    if (Request.IsSecureConnection)
                    {
                        sURL = string.Concat("https://", Common.Globals.GetDomainName(Request), "/");
                    }
                    else
                    {
                        sURL = string.Concat("http://", Common.Globals.GetDomainName(Request), "/");
                    }
                    DataCache.SettingsCacheStore(ModuleId,string.Format(CacheKeys.HostUrl, ModuleId), sURL, DateTime.UtcNow.AddMinutes(30));
                    return sURL;
                }
                return Convert.ToString(obj);
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public string GetWarningImage(string ImageId, string WarningMessage)
        {
            return string.Concat("<img id=\"", ImageId, "\" onmouseover=\"showTip(this,'", WarningMessage, "');\" onmouseout=\"hideTip();\" alt=\"", WarningMessage, "\" height=\"16\" width=\"16\" src=\"", Page.ResolveUrl(string.Concat(Globals.ModulePath, "images/warning.gif")), "\" />");
        }
        protected string GetSharedResource(string key)
        {
            return Utilities.GetSharedResource(key, true);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public Hashtable ActiveSettings
        {
            get
            {
                return MainSettings.MainSettings;
            }
        }
        public SettingsInfo MainSettings
        {
            get
            {
                return new SettingsInfo { MainSettings = new ModuleController().GetModule(moduleID: ModuleId).ModuleSettings };
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public DateTime CacheUpdatedTime
        {
            get
            {
                object obj = DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.CacheUpdate, ModuleId));
                if (obj != null)
                {
                    return Convert.ToDateTime(obj);
                }
                return DateTime.UtcNow;
            }
            set
            { 
                DataCache.SettingsCacheStore(ModuleId, string.Format(CacheKeys.CacheUpdate, ModuleId), value);
                _CacheUpdatedTime = value;
            }
        }
        protected override void OnInit(EventArgs e)
        {
 	        base.OnInit(e);
            LocalResourceFile = Globals.ControlPanelResourceFile;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        internal string ScriptEscape(string escape)
        {
            escape = escape.Replace("'", "\\'");
            escape = escape.Replace("\"", "\\\"");
            return escape;
        }
        public string LocalizeControl(string controlText)
        {
            return Utilities.LocalizeControl(controlText, true);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        protected override void Render(HtmlTextWriter writer)
        {
            var stringWriter = new System.IO.StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = LocalizeControl(html);
            writer.Write(html);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public Controls.ClientTemplate GetLoadingTemplate()
        {
            var template = new Controls.ClientTemplate {ID = "LoadingTemplate"};
            template.Controls.Add(new LiteralControl(string.Concat("<div class=\"amloading\"><div class=\"amload\"><img src=\"", Page.ResolveUrl("~/DesktopModules/ActiveForums/images/spinner.gif"), "\" align=\"absmiddle\" alt=\"Loading\" />Loading...</div></div>")));
            return template;
        }
        public Controls.ClientTemplate GetLoadingTemplateSmall()
        {
            var template = new Controls.ClientTemplate {ID = "LoadingTemplate"};
            template.Controls.Add(new LiteralControl(string.Concat("<div style=\"text-align:center;font-family:Tahoma;font-size:10px;\"><img src=\"", Page.ResolveUrl("~/DesktopModules/ActiveForums/images/spinner.gif"), "\" align=\"absmiddle\" alt=\"Loading\" />Loading...</div>")));
            return template;
        }
        public void BindTemplateDropDown(DropDownList drp, Templates.TemplateTypes TemplateType, string DefaultText, string DefaultValue)
        {
            var tc = new TemplateController();
            drp.DataTextField = "Title";
            drp.DataValueField = "TemplateID";
            drp.DataSource = tc.Template_List(PortalId, ModuleId, TemplateType);
            drp.DataBind();
            drp.Items.Insert(0, new ListItem(DefaultText, DefaultValue));
        }
        public string CurrentView
        {
            get
            {
                if (Session[ViewKey] != null)
                {
                    return Session[ViewKey].ToString();
                }
                if (_currentView != string.Empty)
                {
                    return _currentView;
                }
                return DefaultView;
            }
            set
            {
                Session[ViewKey] = value;
                _currentView = value;
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public string ProductEditon
        {
            get
            {
                return string.Empty;
            }

        }
    }
}