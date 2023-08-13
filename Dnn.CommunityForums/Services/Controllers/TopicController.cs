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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.ActiveForums.API;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Web.Api;
using static DotNetNuke.Modules.ActiveForums.Handlers.HandlerBase;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class TopicController : ControllerBase<TopicController>
    {
        public struct TopicDto
        {
            public int ForumId { get; set; }
            public int TopicId { get; set; }
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
        public HttpResponseMessage Subscribe(TopicDto dto)
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

        /// <summary>
        /// Gets Subscriber count for a Topic
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/SubscriberCount?ForumId=xxx&TopicId=xxx</remarks>
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

        /// <summary>
        /// Gets Subscriber count string for a Topic
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx&TopicId=xxx</remarks>
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
        public HttpResponseMessage Pin(TopicDto dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(PortalSettings.PortalId, ForumModuleId, topicId);
                if (t != null)
                {
                    t.IsPinned = !t.IsPinned;
                    tc.TopicSave(PortalSettings.PortalId, ForumModuleId, t);
                    return Request.CreateResponse(HttpStatusCode.OK, value: t.IsPinned);
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
        public HttpResponseMessage Lock(TopicDto dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(PortalSettings.PortalId, ForumModuleId, topicId);
                if (t != null)
                {
                    t.IsLocked = !t.IsLocked;
                    tc.TopicSave(PortalSettings.PortalId, ForumModuleId, t);
                    return Request.CreateResponse(HttpStatusCode.OK, t.IsLocked);
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
        public HttpResponseMessage Move(TopicDto dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(ActiveModule.PortalID, ForumModuleId, topicId);
                if (t != null)
                {
                    tc.Topics_Move(ActiveModule.PortalID, ForumModuleId, forumId, topicId);
                    DataCache.CacheClearPrefix(ForumModuleId, string.Format(CacheKeys.ForumViewPrefix, ForumModuleId));
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        /// <summary>
        /// Loads a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Load</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
        [ForumsAuthorize(SecureActions.Edit)]
        [ForumsAuthorize(SecureActions.ModEdit)]
        [ForumsAuthorize(SecureActions.ModMove)]
        public HttpResponseMessage Load(TopicDto dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                DotNetNuke.Modules.ActiveForums.User user = new DotNetNuke.Modules.ActiveForums.UserController().LoadUser(UserInfo);
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(ActiveModule.PortalID, ForumModuleId, topicId);
                if (t != null)
                {
                    Forum f = new DotNetNuke.Modules.ActiveForums.ForumController().Forums_Get(ActiveModule.PortalID, ForumModuleId, forumId, user.UserId, true, false, -1);
                    if (f != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new object[1] { (t, f) });
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        } 
        /// <summary>
        /// Deletes a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Delete</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Delete)]
        [ForumsAuthorize(SecureActions.ModDelete)]
        public HttpResponseMessage Delete(TopicDto dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(ActiveModule.PortalID, ForumModuleId, topicId);
                if (t != null)
                {
                    tc.Topics_Delete(ActiveModule.PortalID, ForumModuleId, forumId, topicId, SettingsBase.GetModuleSettings(ForumModuleId).DeleteBehavior);
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);                   
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}