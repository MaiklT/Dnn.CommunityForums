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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class af_searchadvanced : ForumBase
    {
        #region Private Members

        private string _searchText;
        private string _tags;
        private int? _searchType;
        private int? _authorUserId;
        private string _authorUsername;
        private int? _searchColumns;
        private string _forums;
        private int? _searchDays;
        private int? _resultType;
        private int? _sort;
        
        #endregion

        #region Properties

        private string SearchText
        {
            get
            {
                if(_searchText == null)
                {
                    _searchText = Request.Params[SearchParamKeys.Query] + string.Empty;
                    _searchText = Utilities.XSSFilter(_searchText);
                    _searchText = Utilities.StripHTMLTag(_searchText);
                    _searchText = Utilities.CheckSqlString(_searchText);
                    _searchText = _searchText.Trim();
                }

                return _searchText;
            }
        }

        private string Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = Request.Params[SearchParamKeys.Tag] + string.Empty;
                    _tags = Utilities.XSSFilter(_tags);
                    _tags = Utilities.StripHTMLTag(_tags);
                    _tags = Utilities.CheckSqlString(_tags);
                    _tags = _tags.Trim();
                }

                return _tags;
            }
        }

        private int SearchType
        {
            get
            {
                if(!_searchType.HasValue)
                {
                    int parsedSearchType;
                    _searchType = int.TryParse(Request.Params[SearchParamKeys.SearchType], out parsedSearchType) ? parsedSearchType : 0;
                }

                return _searchType.Value;
            }
        }

        private string AuthorUsername
        {
            get
            {
                if (_authorUsername == null)
                {
                    _authorUsername = Request.Params[SearchParamKeys.Author] + string.Empty;
                    _authorUsername = Utilities.XSSFilter(_authorUsername);
                    _authorUsername = Utilities.StripHTMLTag(_authorUsername);
                    _authorUsername = Utilities.CheckSqlString(_authorUsername);
                    _authorUsername = _authorUsername.Trim();
                }

                return _authorUsername;
            }
        }

        private int SearchColumns
        {
            get
            {
                if (!_searchColumns.HasValue)
                {
                    int parsedSearchColumns;
                    _searchColumns = int.TryParse(Request.Params[SearchParamKeys.Columns], out parsedSearchColumns) ? parsedSearchColumns : 0;
                }

                return _searchColumns.Value;
            }
        }

        private string Forums
        {
            get
            {
                if (_forums == null)
                {
                    _forums = Request.Params[SearchParamKeys.Forums] + string.Empty;
                    _forums = Utilities.XSSFilter(_forums);
                    _forums = Utilities.StripHTMLTag(_forums);
                    _forums = Utilities.CheckSqlString(_forums);
                    _forums = _forums.Trim();
                }

                return _forums;
            }
        }

        private int SearchDays
        {
            get
            {
                if (!_searchDays.HasValue)
                {
                    int parsedValue;
                    _searchDays = int.TryParse(Request.Params[SearchParamKeys.TimeSpan], out parsedValue) ? parsedValue : 0;
                }

                return _searchDays.Value;
            }
        }

        private int ResultType
        {
            get
            {
                if (!_resultType.HasValue)
                {
                    int parsedValue;
                    _resultType = int.TryParse(Request.Params[SearchParamKeys.ResultType], out parsedValue) ? parsedValue : 0;
                }

                return _resultType.Value;
            }
        }

        private int Sort
        {
            get
            {
                if (!_sort.HasValue)
                {
                    int parsedValue;
                    _sort = int.TryParse(Request.Params[SearchParamKeys.Sort], out parsedValue) ? parsedValue : 0;
                }

                return _sort.Value;
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            ClientResourceManager.RegisterScript(Page, Globals.ModulePath + "scripts/jquery-forumSelector.js");

            try
            {

                if (Request.QueryString[Literals.GroupId] != null && SimulateIsNumeric.IsNumeric(Request.QueryString[Literals.GroupId]))
                {
                    SocialGroupId = Convert.ToInt32(Request.QueryString[Literals.GroupId]);
                }

                btnSearch.Click += btnSearch_Click;
                btnSearch2.Click += btnSearch_Click;

                if (Page.IsPostBack) 
                    return;

                // Bind the intial values for the forum

                // Options

                litInputError.Text = GetSharedResource("[RESX:SearchInputError]");

                litOptions.Text = GetSharedResource("[RESX:SearchOptions]");

                lblSearch.Text = GetSharedResource("[RESX:SearchKeywords]");
                txtSearch.Text = SearchText;

                ListItem selectedItem = drpSearchColumns.Items.FindByValue(SearchColumns.ToString());
                if (selectedItem != null)
                    selectedItem.Selected = true;

                selectedItem = drpSearchType.Items.FindByValue(SearchType.ToString());
                if (selectedItem != null)
                    selectedItem.Selected = true;

                lblUserName.Text = GetSharedResource("[RESX:SearchByUser]");
                txtUserName.Text = AuthorUsername;

                lblTags.Text = GetSharedResource("[RESX:SearchByTag]");
                txtTags.Text = Tags;


                // Additional Options

                litAdditionalOptions.Text = GetSharedResource("[RESX:SearchOptionsAdditional]");

                lblForums.Text = GetSharedResource("[RESX:SearchInForums]");
                BindForumList();

                lblSearchDays.Text = GetSharedResource("[RESX:SearchTimeFrame]");
                BindSearchRange();

                lblResultType.Text = GetSharedResource("[RESX:SearchResultType]");
                selectedItem = drpResultType.Items.FindByValue(ResultType.ToString());
                if (selectedItem != null)
                    selectedItem.Selected = true;

                lblSortType.Text = GetSharedResource("[RESX:SearchSort]");
                selectedItem = drpSort.Items.FindByValue(Sort.ToString());
                if (selectedItem != null)
                    selectedItem.Selected = true;

                // Buttons

                btnSearch.Text =  GetSharedResource("[RESX:Search]");
                btnSearch2.Text = GetSharedResource("[RESX:Search]");

                btnReset.InnerText = GetSharedResource("[RESX:Reset]");
                btnReset2.InnerText = GetSharedResource("[RESX:Reset]");
                              
                // Update Meta Data
                var basePage = BasePage;
                Environment.UpdateMeta(ref basePage, "[VALUE] - " + GetSharedResource("[RESX:SearchAdvanced]"), "[VALUE]", "[VALUE]");
            }
            catch (Exception ex)
            {
                Controls.Clear();
                RenderMessage("[RESX:ERROR]", "[RESX:ERROR:Search]", ex.Message, ex);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchText = txtSearch.Text;
            var authorUsername = txtUserName.Text;
            var tags = txtTags.Text;

            if (string.IsNullOrWhiteSpace(searchText) && string.IsNullOrWhiteSpace(authorUsername) && string.IsNullOrWhiteSpace(tags))
                return;
            
            // Query
            if(!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = Utilities.XSSFilter(searchText);
                searchText = Utilities.StripHTMLTag(searchText);
                searchText = Utilities.CheckSqlString(searchText);
                searchText = searchText.Trim();  
            }

            // Author Name
            if (!string.IsNullOrWhiteSpace(authorUsername))
            {
                authorUsername = txtUserName.Text;
                authorUsername = Utilities.CheckSqlString(authorUsername);
                authorUsername = Utilities.XSSFilter(authorUsername);
                authorUsername = authorUsername.Trim();
            }

            // Tags
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tags = Utilities.XSSFilter(tags);
                tags = Utilities.StripHTMLTag(tags);
                tags = Utilities.CheckSqlString(tags);
                tags = tags.Trim();
            }

            // Search Type, Search Column & Search Days
            var searchType = Convert.ToInt32(drpSearchType.SelectedItem.Value);
            var searchColumns = Convert.ToInt32(drpSearchColumns.SelectedItem.Value);
            var searchDays = Convert.ToInt32(drpSearchDays.SelectedItem.Value);
            var resultType = Convert.ToInt32(drpResultType.SelectedItem.Value);
            var sort = Convert.ToInt32(drpSort.SelectedValue);
            
            // Selected Forums
            var forums = string.Empty;

            // If the "All" item is selected, we don't need pass the forums parameter
            if(!lbForums.Items[0].Selected)
            {
                var forumId = 0;

                foreach (var item in lbForums.Items.Cast<ListItem>().Where(item => item.Selected && int.TryParse(Regex.Replace(item.Value, @"F(\d+)G\d+", "$1"), out forumId)))
                {
                    if(forums != string.Empty)
                        forums += ":";

                    forums += forumId;
                }
            }

            var @params = new List<string> { $"{ParamKeys.ViewType}={Views.Search}" };

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                @params.Add($"{SearchParamKeys.Query}={System.Web.HttpUtility.UrlEncode(searchText)}");
            }

            if (!string.IsNullOrWhiteSpace(tags))
            {
                @params.Add($"{SearchParamKeys.Tag}={System.Web.HttpUtility.UrlEncode(tags)}");
            }

            if (searchType > 0)
            {
                @params.Add($"{SearchParamKeys.SearchType}={searchType}");
            }

            if (searchColumns > 0)
            {
                @params.Add($"{SearchParamKeys.Columns}={searchColumns}");
            }

            if(searchDays > 0)
            {
                @params.Add($"{SearchParamKeys.TimeSpan}={searchDays}");
            }

            if(resultType > 0)
            {
                @params.Add($"{SearchParamKeys.ResultType}={resultType}");
            }

            if(sort > 0)
            {
                @params.Add($"{SearchParamKeys.Sort}={sort}");
            }

            if(!string.IsNullOrWhiteSpace(authorUsername))
            {
                @params.Add($"{SearchParamKeys.Author}={System.Web.HttpUtility.UrlEncode(authorUsername)}");
            }

            if (!string.IsNullOrWhiteSpace(forums))
            {
                @params.Add($"{SearchParamKeys.Forums}={System.Web.HttpUtility.UrlEncode(forums)}");
            }

            if (SocialGroupId > 0)
            {
                @params.Add($"{Literals.GroupId}={SocialGroupId}");
            }

            Response.Redirect(NavigateUrl(TabId, string.Empty, @params.ToArray()));
        }




        #endregion

        #region Private Methods

        private void BindSearchRange()
        {
            var sHours = GetSharedResource("SearchRangeHours.Text");
            var sDays = GetSharedResource("SearchRangeDays.Text");
            var sAll = GetSharedResource("SearchRangeAll.Text");

            drpSearchDays.Items.Clear();
            drpSearchDays.Items.Add(new ListItem(sAll, "0"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sHours, "12"), "12"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sHours, "24"), "24"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "3"), "72"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "5"), "124"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "10"), "240"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "15"), "360"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "30"), "720"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "45"), "1080"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "60"), "1440"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "90"), "2160"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "120"), "2880"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "240"), "5760"));
            drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "365"), "8640"));

            var selectItem = drpSearchDays.Items.FindByValue(SearchDays.ToString());
            if (selectItem != null)
                selectItem.Selected = true;
        }

        private void BindForumList()
        {
            lbForums.Items.Clear();

            int parseId;
            var forumsToSearch = Forums.Split(':').Where(o => int.TryParse(o, out parseId) && parseId > 0).Select(int.Parse).ToList();

            // Add the "All Forums" item
            lbForums.Items.Add(new ListItem("All Forums", "0") { Selected = forumsToSearch.Count == 0 && ForumId <= 0 });

            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ForumModuleId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(forums, ForumUser,
                fi => {
                lbForums.Items.Add(new ListItem(fi.GroupName, $"G{fi.ForumGroupId}"));
                },
                fi => {
                lbForums.Items.Add(new ListItem($"{fi.ForumName}", $"F{fi.ForumID}G{fi.ForumGroupId}") { Selected = (forumsToSearch.Contains(fi.ForumID) || ForumId == fi.ForumID) });
                },
                fi => {
                lbForums.Items.Add(new ListItem($"--{fi.ForumName}", $"F{fi.ForumID}G{fi.ForumGroupId}") { Selected = (forumsToSearch.Contains(fi.ForumID) || ForumId == fi.ForumID) } );
                }
                );
        }

        #endregion
    }
}
