﻿//
// Community Forums
// Copyright (c) 2013-2021
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

using System.Web;
using System.Web.Services;
using System.Text;
using System.Xml;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using TopicInfo = DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;

namespace DotNetNuke.Modules.ActiveForums.Handlers
{
    public class forumhelper : HandlerBase
	{
		public enum Actions: int
		{
			None,
			UserPing, /* no longer used */
			GetUsersOnline,/* no longer used */
            TopicSubscribe,/* no longer used */
            ForumSubscribe,/* no longer used */
            RateTopic,
			DeleteTopic,/* no longer used */
            MoveTopic,/* no longer used */
            PinTopic,/* no longer used */
            LockTopic,/* no longer used */
            MarkAnswer,/* no longer used */
            TagsAutoComplete,
			DeletePost,
			LoadTopic,/* no longer used */
            SaveTopic,
			ForumList,/* no longer used */
            LikePost /*no longer used*/

		}
		public override void ProcessRequest(HttpContext context)
		{
			AdminRequired = false;
			base.AdminRequired = false;
			base.ProcessRequest(context);
			string sOut = "{\"result\":\"success\"}";
			Actions action = Actions.None;
			if (Params != null && Params.Count > 0)
			{
				if (Params["action"] != null && SimulateIsNumeric.IsNumeric(Params["action"]))
				{
					action = (Actions)(Convert.ToInt32(Params["action"].ToString()));
				}
			}
			else if (HttpContext.Current.Request.QueryString["action"] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString["action"]))
			{
				if (int.Parse(HttpContext.Current.Request.QueryString["action"]) == 11)
				{
					action = Actions.TagsAutoComplete;
				}
			}
			switch (action)
			{
				case Actions.UserPing:
					throw new NotImplementedException();
					////sOut = UserOnline();
					////break;
                case Actions.GetUsersOnline:
                    throw new NotImplementedException();
					////sOut = GetUserOnlineList();
					////break;
				case Actions.TopicSubscribe:
                    throw new NotImplementedException();
     //               sOut = SubscribeTopic();
					//break;
				case Actions.ForumSubscribe:
                    throw new NotImplementedException();
     //               sOut = SubscribeForum();
					//break;
				case Actions.RateTopic:
					sOut = RateTopic();
					break;
				case Actions.DeleteTopic:
                    throw new NotImplementedException();
     //               sOut = DeleteTopic();
					//break;
				case Actions.MoveTopic:
                    throw new NotImplementedException();
     //               sOut = MoveTopic();
					//break;
				case Actions.PinTopic:
                    throw new NotImplementedException();
     //               sOut = PinTopic();
					//break;
				case Actions.LockTopic:
                    throw new NotImplementedException();
     //               sOut = LockTopic();
					//break;
				case Actions.MarkAnswer:
                    throw new NotImplementedException();
     //               sOut = MarkAnswer();
					//break;
				case Actions.TagsAutoComplete:
					sOut = TagsAutoComplete();
					break;
				case Actions.DeletePost:
					sOut = DeletePost();
					break;
				case Actions.LoadTopic:
                    throw new NotImplementedException();
     //               sOut = LoadTopic();
					//break;
				case Actions.SaveTopic:
					sOut = SaveTopic();
					break;
				case Actions.ForumList:
                    throw new NotImplementedException();
     //               sOut = ForumList();
					//break;
                case Actions.LikePost:
					throw new NotImplementedException();
                    //sOut = LikePost();
                    //break;
			}
			context.Response.ContentType = "text/plain";
			context.Response.Write(sOut);
		}
		private string RateTopic()
		{
			int r = 0;
			int topicId = -1;
			if (Params.ContainsKey("rate") && SimulateIsNumeric.IsNumeric(Params["rate"]))
			{
				r = int.Parse(Params["rate"].ToString());
			}
			if (Params.ContainsKey("topicid") && SimulateIsNumeric.IsNumeric(Params["topicid"]))
			{
				topicId = int.Parse(Params["topicid"].ToString());
			}
			if (r >= 1 && r <= 5 && topicId > 0)
			{
				DataProvider.Instance().Topics_AddRating(topicId, UserId, r, string.Empty, HttpContext.Current.Request.UserHostAddress.ToString());
			}
			r = DataProvider.Instance().Topics_GetRating(topicId);
			return BuildOutput(r.ToString(), OutputCodes.Success, true, false);
		}
		private string TagsAutoComplete()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string q = string.Empty;
			if (! (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["q"])))
			{
				q = HttpContext.Current.Request.QueryString["q"].Trim();
				q = Utilities.Text.RemoveHTML(q);
				q = Utilities.Text.CheckSqlString(q);
				if (! (string.IsNullOrEmpty(q)))
				{
					if (q.Length > 20)
					{
						q = q.Substring(0, 20);
					}
				}
			}
			int i = 0;
			if (! (string.IsNullOrEmpty(q)))
			{
				using (IDataReader dr = DataProvider.Instance().Tags_Search(PortalId, ModuleId, q))
				{
					while (dr.Read())
					{
						sb.AppendLine("{\"id\":\"" + dr["TagId"].ToString() + "\",\"name\":\"" + dr["TagName"].ToString() + "\",\"type\":\"0\"},");
						i += 1;
					}
					dr.Close();
				}
			}
			string @out = "[";
			if (i > 0)
			{
				@out += sb.ToString().Trim();
				@out = @out.Substring(0, @out.Length - 1);
			}
			@out += "]";
			return @out;
		}
		private string DeletePost()
		{
			int replyId = -1;
			int TopicId = -1;
			if (Params.ContainsKey("topicid") && SimulateIsNumeric.IsNumeric(Params["topicid"]))
			{
				TopicId = int.Parse(Params["topicid"].ToString());
			}
			if (Params.ContainsKey("replyid") && SimulateIsNumeric.IsNumeric(Params["replyid"]))
			{
				replyId = int.Parse(Params["replyid"].ToString());
			}
			int forumId = -1;
			Data.ForumsDB db = new Data.ForumsDB();
			forumId = db.Forum_GetByTopicId(TopicId);
			ForumController fc = new ForumController();
			Forum f = fc.Forums_Get(forumId, this.UserId, true);

			// Need to get the list of attachments BEFORE we remove the post recods
			var attachmentController = new Data.AttachController();
			var attachmentList = (MainSettings.DeleteBehavior == 0)
									 ? attachmentController.ListForPost(TopicId, replyId)
									 : null;


			if (TopicId > 0 & replyId < 1)
			{
				TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = tc.Topics_Get(PortalId, ModuleId, TopicId);

				if (Permissions.HasAccess(f.Security.ModDelete, ForumUser.UserRoles) || (Permissions.HasAccess(f.Security.Delete, ForumUser.UserRoles) && ti.Content.AuthorId == UserId && ti.IsLocked == false))
				{
					DataProvider.Instance().Topics_Delete(forumId, TopicId, MainSettings.DeleteBehavior);
					string journalKey = string.Format("{0}:{1}", forumId.ToString(), TopicId.ToString());
					JournalController.Instance.DeleteJournalItemByKey(PortalId, journalKey);
				}
				else
				{
					return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
				}
			}
			else
			{
				ReplyController rc = new ReplyController();
				DotNetNuke.Modules.ActiveForums.ReplyInfo ri = rc.Reply_Get(PortalId, ModuleId, TopicId, replyId);
				if (Permissions.HasAccess(f.Security.ModDelete, ForumUser.UserRoles) || (Permissions.HasAccess(f.Security.Delete, ForumUser.UserRoles) && ri.Content.AuthorId == UserId))
				{
					DataProvider.Instance().Reply_Delete(forumId, TopicId, replyId, MainSettings.DeleteBehavior);
					string journalKey = string.Format("{0}:{1}:{2}", forumId.ToString(), TopicId.ToString(), replyId.ToString());
					JournalController.Instance.DeleteJournalItemByKey(PortalId, journalKey);

				}
				else
				{
					return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
				}

			}

			// If it's a hard delete, delete associated attachments
			// attachmentList will only be populated if the DeleteBehavior is 0
			if (attachmentList != null)
			{      
				var fileManager = FileManager.Instance;
				var folderManager = FolderManager.Instance;
				var attachmentFolder = folderManager.GetFolder(PortalId, "activeforums_Attach");

				foreach (var attachment in attachmentList)
				{
					attachmentController.Delete(attachment.AttachmentId);

					var file = attachment.FileId.HasValue
								   ? fileManager.GetFile(attachment.FileId.Value)
								   : fileManager.GetFile(attachmentFolder, attachment.FileName);

					// Only delete the file if it exists in the attachment folder
					if (file != null && file.FolderId == attachmentFolder.FolderID)
						fileManager.DeleteFile(file);
				}
			}

			// Return the result
			DataCache.CacheClearPrefix(ModuleId, string.Format(CacheKeys.ForumViewPrefix, ModuleId));
			return BuildOutput(TopicId + "|" + replyId, OutputCodes.Success, true);
		}
		
		private string SaveTopic()
		{
			int topicId = -1;
			int forumId = -1;
			if (Params.ContainsKey("topicid") && SimulateIsNumeric.IsNumeric(Params["topicid"]))
			{
				topicId = int.Parse(Params["topicid"].ToString());
			}
			if (topicId > 0)
			{
				TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(PortalId, ModuleId, topicId);
				Data.ForumsDB db = new Data.ForumsDB();
				forumId = db.Forum_GetByTopicId(topicId);
				ForumController fc = new ForumController();
				Forum ForumInfo = fc.Forums_Get(PortalId, -1, forumId, this.UserId, true, false, -1);
				if (Permissions.HasPerm(ForumInfo.Security.ModEdit, ForumUser.UserRoles))
				{
					string subject = Params["subject"].ToString();
					subject = Utilities.XSSFilter(subject, true);
                    t.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrl(PortalId: PortalId, ModuleId: ForumInfo.ModuleId, TopicId: topicId, subject: subject, forumInfo: ForumInfo);
                    
					t.Content.Subject = subject;
					t.IsPinned = bool.Parse(Params["pinned"].ToString());
					t.IsLocked = bool.Parse(Params["locked"].ToString());
					t.Priority = int.Parse(Params["priority"].ToString());
					t.StatusId = int.Parse(Params["status"].ToString());
					if (ForumInfo.Properties != null)
					{
						StringBuilder tData = new StringBuilder();
						tData.Append("<topicdata>");
						tData.Append("<properties>");
						foreach (PropertiesInfo p in ForumInfo.Properties)
						{
							string pkey = "prop-" + p.PropertyId.ToString();

							tData.Append("<property id=\"" + p.PropertyId.ToString() + "\">");
							tData.Append("<name><![CDATA[");
							tData.Append(p.Name);
							tData.Append("]]></name>");
							if (Params[pkey] != null)
							{
								tData.Append("<value><![CDATA[");
								tData.Append(Utilities.XSSFilter(Params[pkey].ToString()));
								tData.Append("]]></value>");
							}
							else
							{
								tData.Append("<value></value>");
							}
							tData.Append("</property>");
						}
						tData.Append("</properties>");
						tData.Append("</topicdata>");
						t.TopicData = tData.ToString();
					}
				}
				tc.TopicSave(PortalId, ModuleId, t);
                tc.UpdateModuleLastContentModifiedOnDate(ModuleId);
                if (Params["tags"] != null)
				{
					DataProvider.Instance().Tags_DeleteByTopicId(PortalId, ForumInfo.ModuleId, topicId);
					string tagForm = string.Empty;
					if (Params["tags"] != null)
					{
						tagForm = Params["tags"].ToString();
					}
					if (! (tagForm == string.Empty))
					{
						string[] Tags = tagForm.Split(',');
						foreach (string tag in Tags)
						{
							string sTag = Utilities.CleanString(PortalId, tag.Trim(), false, EditorTypes.TEXTBOX, false, false, ForumInfo.ModuleId, string.Empty, false);
							DataProvider.Instance().Tags_Save(PortalId, ForumInfo.ModuleId, -1, sTag, 0, 1, 0, topicId, false, -1, -1);
						}
					}
				}

				if (Params["categories"] != null)
				{
					string[] cats = Params["categories"].ToString().Split(';');
					DataProvider.Instance().Tags_DeleteTopicToCategory(PortalId, ForumInfo.ModuleId, -1, topicId);
					foreach (string c in cats)
					{
						int cid = -1;
						if (! (string.IsNullOrEmpty(c)) && SimulateIsNumeric.IsNumeric(c))
						{
							cid = Convert.ToInt32(c);
							if (cid > 0)
							{
								DataProvider.Instance().Tags_AddTopicToCategory(PortalId, ForumInfo.ModuleId, cid, topicId);
							}
						}
					}
				}
			}


			return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
		}
		
	}
}