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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.UI.WebControls;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class TopicController : ControllerBase<TopicController>
    {
        public struct TopicDto1
        {
            public int ForumId { get; set; }
            public int TopicId { get; set; }
        }
        public struct TopicDto2
        {
            public int ForumId { get; set; }
            public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic { get; set; }
        }

        /// <summary>
        /// Subscribes to a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Subscribe</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Subscribe)]
        public HttpResponseMessage Subscribe(TopicDto1 dto)
        {
            if (dto.TopicId > 0 && dto.ForumId > 0)
            {

                string userRoles = new DotNetNuke.Modules.ActiveForums.UserProfileController()
                    .Profiles_Get(ActiveModule.PortalID, ForumModuleId, UserInfo.UserID).Roles;
                int subscribed = new SubscriptionController().Subscription_Update(ActiveModule.PortalID,
                    ForumModuleId, dto.ForumId, dto.TopicId, 1, UserInfo.UserID, userRoles);
                return Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

#pragma warning disable CS1570
        /// <summary>
        /// Gets Subscriber count for a Topic
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/SubscriberCount?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCount(int ForumId, int TopicId)
        {
            if (ForumId > 0 && TopicId > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(
                        ActiveModule.PortalID, ForumModuleId, ForumId, TopicId));
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

#pragma warning disable CS1570
        /// <summary>
        /// Gets Subscriber count string for a Topic
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCountString(int ForumId, int TopicId)
        {
            if (ForumId > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    $"{new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(ActiveModule.PortalID, ForumModuleId, ForumId, TopicId)} {Utilities.GetSharedResource("[RESX:TOPICSUBSCRIBERCOUNT]", false)}");
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
          
    /// <summary>
    /// Pins a Topic
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Pin</remarks>
    [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModPin)]
        [ForumsAuthorize(SecureActions.Pin)]
        public HttpResponseMessage Pin(TopicDto1 dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    ti.IsPinned = !ti.IsPinned;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                    return Request.CreateResponse(HttpStatusCode.OK, value: ti.IsPinned);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Locks a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Lock</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModLock)]
        [ForumsAuthorize(SecureActions.Lock)]
        public HttpResponseMessage Lock(TopicDto1 dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    ti.IsLocked = !ti.IsLocked;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                    return Request.CreateResponse(HttpStatusCode.OK, ti.IsLocked);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        /// <summary>
        /// Moves a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Move</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModMove)]
        public HttpResponseMessage Move(TopicDto1 dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(topicId, forumId);
                    DataCache.CacheClearPrefix(ForumModuleId, string.Format(CacheKeys.CacheModulePrefix, ForumModuleId));
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
#pragma warning disable CS1570
        /// <summary>
        /// Loads a Topic
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.Entities.TopicInfo"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Load?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Read)]
        public HttpResponseMessage Load(int ForumId, int TopicId)
        {
            if (TopicId > 0 && ForumId > 0)
            {
                if (ServicesHelper.IsAuthorized(PortalSettings.PortalId, ForumModuleId, ForumId, SecureActions.Read, UserInfo))
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
                    if (t != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new object[] { t });
                    }
                    else 
                    {
                        Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
#pragma warning disable CS1570
        /// <summary>
        /// Deletes a Topic
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Delete?forumId=xxx&topicId=yyy</remarks>
#pragma warning restore CS1570
        [HttpDelete]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Delete)]
        [ForumsAuthorize(SecureActions.ModDelete)]
        public HttpResponseMessage Delete(int forumId, int topicId)
        {
            if (forumId > 0 && topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController tc = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = tc.GetById(topicId);
                if (ti != null)
                {
                    tc.DeleteById(topicId);
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Rates a topic
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="rating" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Rate</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        [ForumsAuthorize(SecureActions.ModEdit)]
        public HttpResponseMessage Rate(TopicDto1 dto, int rating)
        {
            if (dto.TopicId > 0 && (rating >= 1 && rating <= 5))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(UserInfo.UserID, dto.TopicId, rating, HttpContext.Current.Request.UserHostAddress ?? string.Empty));
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
        /// <summary>
        /// Updates an existing topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.Entities.TopicInfo"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Update</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        [ForumsAuthorize(SecureActions.ModEdit)]
        public HttpResponseMessage Update(TopicDto2 dto)
        {
            int forumId = dto.ForumId;
            int topicId = dto.Topic.TopicId;

            if (topicId > 0 && forumId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo originalTopic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (originalTopic != null)
                {
                    string subject = Utilities.XSSFilter(dto.Topic.Content.Subject, true);
                    originalTopic.Content.Subject = subject;
                    originalTopic.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrl(PortalId: ActiveModule.PortalID, ModuleId: ForumModuleId, TopicId: topicId, subject: subject, forumInfo: originalTopic.Forum);

                    if (dto.Topic.IsLocked != originalTopic.IsLocked &&
                        (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Lock, string.Join(";", UserInfo.Roles)) ||
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.ModLock, string.Join(";", UserInfo.Roles))
                        )
                        )
                    {
                        originalTopic.IsLocked = dto.Topic.IsLocked;
                    };

                    if (dto.Topic.IsPinned != originalTopic.IsPinned &&
                        (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Pin, string.Join(";", UserInfo.Roles)) ||
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.ModPin, string.Join(";", UserInfo.Roles))
                        )
                        )
                    {
                        originalTopic.IsLocked = dto.Topic.IsLocked;
                    };

                    originalTopic.Priority = dto.Topic.Priority;
                    originalTopic.StatusId = dto.Topic.StatusId;

                    if (originalTopic.Forum.Properties != null && originalTopic.Forum.Properties.Count > 0)
                    {
                        StringBuilder tData = new StringBuilder();
                        tData.Append("<topicdata>");
                        tData.Append("<properties>");
                        foreach (var p in originalTopic.Forum.Properties)
                        {
                            tData.Append("<property id=\"" + p.PropertyId.ToString() + "\">");
                            tData.Append("<name><![CDATA[");
                            tData.Append(p.Name);
                            tData.Append("]]></name>");
                            if (!string.IsNullOrEmpty(dto.Topic.TopicProperties?.Where(pl => pl.PropertyId == p.PropertyId).FirstOrDefault().Value))
                            {
                                tData.Append("<value><![CDATA[");
                                tData.Append(Utilities.XSSFilter(dto.Topic.TopicProperties.Where(pl => pl.PropertyId == p.PropertyId).FirstOrDefault().Value));
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
                        originalTopic.TopicData = tData.ToString();
                    }
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(originalTopic);
                    Utilities.UpdateModuleLastContentModifiedOnDate(ForumModuleId);

                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Tag, string.Join(";", UserInfo.Roles)))
                    {
                        if (!string.IsNullOrEmpty(dto.Topic.Tags))
                        {
                            DataProvider.Instance().Tags_DeleteByTopicId(ActiveModule.PortalID, ForumModuleId, topicId);
                            string tagForm = dto.Topic.Tags;
                            string[] tags = tagForm.Split(',');
                            foreach (string tag in tags)
                            {
                                string sTag = Utilities.CleanString(ActiveModule.PortalID, tag.Trim(), false, EditorTypes.TEXTBOX, false, false, ForumModuleId, string.Empty, false);
                                DataProvider.Instance().Tags_Save(ActiveModule.PortalID, ForumModuleId, -1, sTag, 0, 1, 0, topicId, false, -1, -1);
                            }
                        }
                    }
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Categorize, string.Join(";", UserInfo.Roles)))
                    {
                        if (!string.IsNullOrEmpty(dto.Topic.SelectedCategoriesAsString))
                        {
                            string[] cats = dto.Topic.SelectedCategoriesAsString.Split(';');
                            DataProvider.Instance().Tags_DeleteTopicToCategory(ActiveModule.PortalID, ForumModuleId, -1, topicId);
                            foreach (string c in cats)
                            {
                                int cid = -1;
                                if (!(string.IsNullOrEmpty(c)) && SimulateIsNumeric.IsNumeric(c))
                                {
                                    cid = Convert.ToInt32(c);
                                    if (cid > 0)
                                    {
                                        DataProvider.Instance().Tags_AddTopicToCategory(ActiveModule.PortalID, ForumModuleId, cid, topicId);
                                    }
                                }
                            }
                        }
                    }
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo updatedTopic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                    return Request.CreateResponse(HttpStatusCode.OK, updatedTopic);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, dto.Topic);
                }
            }            
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}