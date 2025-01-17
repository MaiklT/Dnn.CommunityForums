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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;


namespace DotNetNuke.Modules.ActiveForums
{
    [ValidateAntiForgeryToken]
    public class ModerationServiceController : DnnApiController
    {
        private int _tabId = -1;
        private int _moduleId = -1;
        private int _forumId = -1;
        private int _topicId = -1;
        private int _replyId = -1;

        // For the Notification API, return an object with "Result" property set to "success" if the operation succeeded.
        // In there is an error, return the error message in the "Message" property.
        // In both cases, it must return an 200 "OK" response.

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage ApprovePost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);

            ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(_forumId, _moduleId);
            if (fi == null)
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });


            if (!(IsMod(_forumId)))
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });


            if (_replyId > 0)
            {
                var reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().ApproveReply(PortalSettings.PortalId, _tabId, _moduleId, _forumId, _topicId, _replyId);
                if (reply == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });
            }
            else
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Approve(_topicId);
                if (topic == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });
            }

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);

            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage RejectPost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);

            ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(_forumId, _moduleId);
            if (fi == null)
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });

            if (!(IsMod(_forumId)))
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });


            var mc = new ModController();
            mc.Mod_Reject(PortalSettings.PortalId, _moduleId, UserInfo.UserID, _forumId, _topicId, _replyId);

            int authorId;

            if (_replyId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetReply(_replyId);

                if (reply == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });

                authorId = reply.Content.AuthorId;
            }
            else
            {
                var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(_topicId);
                if (topic == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });

                authorId = topic.Content.AuthorId;
            }

            if (fi.ModRejectTemplateId > 0 && authorId > 0)
            {
                var uc = new DotNetNuke.Entities.Users.UserController();
                var ui = uc.GetUser(PortalSettings.PortalId, authorId);
                if (ui != null)
                {
                    var au = new Author
                                 {
                                     AuthorId = authorId,
                                     DisplayName = ui.DisplayName,
                                     Email = ui.Email,
                                     FirstName = ui.FirstName,
                                     LastName = ui.LastName,
                                     Username = ui.Username
                                 };
                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.ModRejectTemplateId, PortalSettings.PortalId, _moduleId, _tabId, _forumId, _topicId, _replyId, string.Empty, au);
                }

            }

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage DeletePost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);
            ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(_forumId, _moduleId);
            if (fi == null)
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });

            if (!(IsMod(_forumId)))
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });


            int authorId;
            var ms = SettingsBase.GetModuleSettings(_moduleId);
            if (_replyId > 0 & _replyId != _topicId)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetReply(_replyId);

                if (reply == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });

                authorId = reply.Content.AuthorId;
                var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController();
                rc.Reply_Delete(PortalSettings.PortalId, _forumId, _topicId, _replyId, ms.DeleteBehavior);
            }
            else
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(_topicId);
                if (ti == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });

                authorId = ti.Content.AuthorId;
                new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().DeleteById(_topicId);

                
            }

            if (fi.ModDeleteTemplateId > 0 && authorId > 0)
            {
                var uc = new DotNetNuke.Entities.Users.UserController();
                var ui = uc.GetUser(PortalSettings.PortalId, authorId);
                if (ui != null)
                {
                    var au = new Author
                    {
                        AuthorId = authorId,
                        DisplayName = ui.DisplayName,
                        Email = ui.Email,
                        FirstName = ui.FirstName,
                        LastName = ui.LastName,
                        Username = ui.Username
                    };
                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmailToModerators(fi.ModDeleteTemplateId, PortalSettings.PortalId, _forumId, _topicId, _replyId, _moduleId, _tabId, string.Empty);
                }
            }

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }
        [HttpPost]
        [DnnAuthorize]
        public HttpResponseMessage BanUser(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);
            ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(_forumId, _moduleId);
            if (fi == null)
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });

            if (!(IsMod(_forumId)))
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });


            int authorId;
            if (_replyId > 0 & _replyId != _topicId)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetReply(_replyId);

                if (reply == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });

                authorId = reply.Content.AuthorId;
            }
            else
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(_topicId);
                if (ti == null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });

                authorId = ti.Content.AuthorId;
            }
            string moduleTitle = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(_moduleId, _tabId, true).ModuleTitle;
            DotNetNuke.Modules.ActiveForums.Controllers.UserController.BanUser(PortalId: ActiveModule.PortalID, ModuleId: _moduleId, ModuleTitle: moduleTitle, TabId: _tabId, ForumId: _forumId, TopicId: _topicId, ReplyId: _replyId, BannedBy: UserInfo, AuthorId: authorId);

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage IgnorePost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);
            ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(_forumId, _moduleId);
            if (fi == null)
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });

            if (!(IsMod(_forumId)))
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        #region Private Methods

        private bool IsMod(int forumId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetListOfModerators(ActiveModule.PortalID, _moduleId, forumId).Any(i => i.UserID == UserInfo.UserID);
        }

        private void ParseNotificationContext(string context)
        {
            var keys = context.Split(':');
            _tabId = int.Parse(keys[0]);
            _moduleId = int.Parse(keys[1]);
            _forumId = int.Parse(keys[2]);
            _topicId = int.Parse(keys[3]);
            _replyId = int.Parse(keys[4]);
        }

        #endregion

        public class ModerationDTO
        {
            public int NotificationId { get; set; }
        }
    }
}

