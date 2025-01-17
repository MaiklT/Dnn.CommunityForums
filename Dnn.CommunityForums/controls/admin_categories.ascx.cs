﻿//
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.ActiveForums
{
	public partial class admin_categories : ActiveAdminBase
	{
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            agCategories.Callback += agCategories_Callback;
            agCategories.ItemBound += agCategories_ItemBound;
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			BindGroups();
		}

		private void agCategories_Callback(object sender, Controls.CallBackEventArgs e)
		{
			try
			{
				if (! (e.Parameters[4] == ""))
				{
					string sAction = e.Parameters[4].Split(':')[0];

					switch (sAction.ToUpper())
					{
						case "DELETE":
						{
							int TagId = Convert.ToInt32(e.Parameters[4].Split(':')[1]);
							if (SimulateIsNumeric.IsNumeric(TagId))
							{
                                new DotNetNuke.Modules.ActiveForums.Controllers.TagController().DeleteById(TagId);
                            }
                            break;
						}
						case "SAVE":
						{
							string[] sParams = e.Parameters[4].Split(':');
							string TagName = sParams[1].Trim();
							int TagId = 0;
							int ForumId = -1;
							int ForumGroupId = -1;
							if (sParams.Length > 2)
							{
								TagId = Convert.ToInt32(sParams[2]);
							}
							if (sParams[3].Contains("FORUM"))
							{
								ForumId = Convert.ToInt32(sParams[3].Replace("FORUM", string.Empty));
							}
							if (sParams[3].Contains("GROUP"))
							{
								ForumGroupId = Convert.ToInt32(sParams[3].Replace("GROUP", string.Empty));
							}

							if (! (TagName == string.Empty))
							{
								DataProvider.Instance().Tags_Save(PortalId, ModuleId, TagId, TagName, 0, 0, 0, -1, true, ForumId, ForumGroupId);
							}



							break;
						}
					}

				}
				agCategories.DefaultParams = string.Empty;
				int PageIndex = Convert.ToInt32(e.Parameters[0]);
				int PageSize = Convert.ToInt32(e.Parameters[1]);
				string SortColumn = e.Parameters[2].ToString();
				string Sort = e.Parameters[3].ToString();
				agCategories.Datasource = DataProvider.Instance().Tags_List(PortalId, ModuleId, true, PageIndex, PageSize, Sort, SortColumn, -1, -1);
				agCategories.Refresh(e.Output);
			}
			catch (Exception ex)
			{

			}
		}
		private void BindGroups()
		{
			drpForums.Items.Add(new ListItem(Utilities.GetSharedResource("DropDownSelect"), "-1"));
			DotNetNuke.Modules.ActiveForums.Entities.ForumCollection allForums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ModuleId);
			var filteredForums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ModuleId).Where(f => f.ForumGroup.Active && f.Active && f.ParentForumId == 0);
			int tmpGroupId = -1;
			foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in filteredForums)
			{
				if (! (tmpGroupId == f.ForumGroupId))
				{
					drpForums.Items.Add(new ListItem(f.GroupName, "GROUP" + f.ForumGroupId.ToString()));
					tmpGroupId = f.ForumGroupId;
				}
				drpForums.Items.Add(new ListItem(" - " + f.ForumName, "FORUM" + f.ForumID.ToString()));
				if (f.SubForums != null && f.SubForums.Count > 0)
				{
					foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ff in f.SubForums)
					{
						drpForums.Items.Add(new ListItem(" ---- " + ff.ForumName, "FORUM" + ff.ForumID.ToString()));
					}
				}
			}
		}
		private void agCategories_ItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
		{
            //e.Item(1) = Server.HtmlEncode(e.Item(1).ToString)
            //e.Item(2) = Server.HtmlEncode(e.Item(2).ToString)
            e.Item[6] = "<img src=\"" + Page.ResolveUrl(Globals.ModulePath + "images/delete16.png") + "\" alt=\"" + GetSharedResource("[RESX:Delete]") + "\" height=\"16\" width=\"16\" />";
        }
        private DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetSubForums(DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums, int forumId)
		{
			DotNetNuke.Modules.ActiveForums.Entities.ForumCollection subforums = null;
			foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo s in forums)
			{
				if (s.ParentForumId == forumId)
				{
					if (subforums == null)
					{
						subforums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
					}
					subforums.Add(s);
				}
			}
			return subforums;
		}
	}
}
