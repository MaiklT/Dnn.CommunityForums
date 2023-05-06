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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Web;
using DotNetNuke.Common.Controls;
using System.Xml.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public class EmailController
    { 

        public bool UseQueue = false;

        public static void SendEmail(int templateId, int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, string comments, Author author)
        {
            var portalSettings = (DotNetNuke.Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
            var sTemplate = string.Empty;
            var tc = new TemplateController();
            var ti = tc.Template_Get(templateId, portalId, moduleId);
            var subject = TemplateUtils.ParseEmailTemplate(ti.Subject, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            var bodyText = TemplateUtils.ParseEmailTemplate(ti.TemplateText, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            var bodyHTML = TemplateUtils.ParseEmailTemplate(ti.TemplateHTML, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            bodyText = bodyText.Replace("[REASON]", comments);
            bodyHTML = bodyHTML.Replace("[REASON]", comments);
            var fc = new ForumController();
            var fi = fc.Forums_Get(forumId, -1, false, true);
            var sFrom = fi.EmailAddress != string.Empty ? fi.EmailAddress : portalSettings.Email;

            //Send now

            var subs = new List<SubscriptionInfo>();
            var si = new SubscriptionInfo
            {
                DisplayName = author.DisplayName,
                Email = author.Email,
                FirstName = author.FirstName,
                LastName = author.LastName,
                UserId = author.AuthorId,
                Username = author.Username
            };

            subs.Add(si);
            //new Thread(
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.Email()
            {
                BodyHTML = bodyHTML,
                BodyText = bodyText,
                From = sFrom,
                ModuleId = moduleId,
                PortalId = portalId,
                Recipients = subs,
                Subject = subject
            });
            //.Start();
        }
        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments)
        {
            SendEmailToModerators(templateId: templateId, portalId: portalId, moduleID: moduleID, forumId: forumId, topicId: topicId, replyId: replyId, tabID: tabID, comments: comments, user: null);
        }

        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments, UserInfo user)
        {
            var fc = new ForumController();
            var fi = fc.Forums_Get(forumId, -1, false, true);
            if (fi == null)
                return;

            var subs = new List<SubscriptionInfo>();
            var rc = new Security.Roles.RoleController();
            var rp = RoleProvider.Instance();
            var uc = new DotNetNuke.Entities.Users.UserController();
            var modApprove = fi.Security.ModApprove;
            var modRoles = modApprove.Split('|')[0].Split(';');
            foreach (var r in modRoles)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var rid = Convert.ToInt32(r);
                var rName = rc.GetRole(rid, portalId).RoleName;
                foreach (UserRoleInfo usr in rp.GetUserRoles(portalId, null, rName))
                {
                    var ui = uc.GetUser(portalId, usr.UserID);
                    var si = new SubscriptionInfo
                    {
                        UserId = ui.UserID,
                        DisplayName = ui.DisplayName,
                        Email = ui.Email,
                        FirstName = ui.FirstName,
                        LastName = ui.LastName,
                        TimeZoneOffSet = Utilities.GetTimeZoneOffsetForUser(portalId, ui.UserID),
                        UserCulture = Utilities.GetCultureInfoForUser(portalId, ui.UserID),
                        TopicSubscriber = false
                    };
                    if (!(subs.Contains(si)))
                    {
                        subs.Add(si);
                    }
                }
            }
            if (subs.Count > 0)
            {
                SendTemplatedEmail(templateId, portalId, topicId, replyId, moduleID, tabID, comments, user.UserID, fi, subs);
            }
        }
        public static void SendTemplatedEmail(int templateId, int portalId, int topicId, int replyId, int moduleID, int tabID, string comments, int userId, Forum fi, List<SubscriptionInfo> subs)
        {
            PortalSettings portalSettings = (DotNetNuke.Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
            TemplateController tc = new TemplateController();
            TemplateUtils.lstSubscriptionInfo = subs;
            TemplateInfo ti = templateId > 0 ? tc.Template_Get(templateId) : tc.Template_Get("SubscribedEmail", portalId, moduleID);
            IEnumerable<CultureInfo> userCultures = subs.Select(s => s.UserCulture).Distinct();
            foreach (CultureInfo userCulture in userCultures)
            {
                IEnumerable<TimeSpan> timeZoneOffsets = subs.Where(s => s.UserCulture == userCulture).Select(s => s.TimeZoneOffSet).Distinct();
                foreach (TimeSpan timeZoneOffset in timeZoneOffsets)
                {
                    string sTemplate = string.Empty;
                    string sFrom = fi.EmailAddress != string.Empty ? fi.EmailAddress : portalSettings.Email;
                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.Email()
                    {
                        PortalId = portalId,
                        ModuleId = moduleID,
                        BodyHTML = TemplateUtils.ParseEmailTemplate(ti.TemplateHTML, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: comments, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false),
                        BodyText = TemplateUtils.ParseEmailTemplate(ti.TemplateHTML, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: comments, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: true),
                        From = sFrom,
                        Recipients = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture == userCulture && s.TopicSubscriber).ToList(),
                        Subject = TemplateUtils.ParseEmailTemplate(ti.Subject, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: string.Empty, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: true),
                    });
                    /* subject/text/body, etc. can now be different based on topic subscriber vs forum subscriber so process first for topic subscribers and then for forum subscribers */
                    
                    if (subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture == userCulture && !s.TopicSubscriber).ToList().Count > 0)
                    {
                        //new System.Threading.Thread(oEmail.Send).Start();
                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.Email()
                        {
                            BodyHTML = TemplateUtils.ParseEmailTemplate(ti.TemplateHTML, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: comments, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false),
                            BodyText = TemplateUtils.ParseEmailTemplate(ti.TemplateText, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: comments, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false),
                            From = sFrom,
                            ModuleId = moduleID,
                            PortalId = portalId,
                            Recipients = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture == userCulture && !s.TopicSubscriber).ToList(),
                            Subject = TemplateUtils.ParseEmailTemplate(ti.Subject, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: string.Empty, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false),
                        });
                    }

                }
            }
        }
        
        public static void SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            //USE DNN API for this to ensure proper delivery & adherence to portal settings
            //Services.Mail.Mail.SendEmail(fromEmail, fromEmail, toEmail, subject, bodyHTML);

            //Since this code is triggered from the DNN scheduler, the default/simple API (now commented out above) uses Host rather than Portal-specific SMTP configuration
            //updated here to retrieve portal-specific SMTP configuration and use more elaborate DNN API that allows passing of the SMTP information rather than rely on DNN API DotNetNuke.Host.SMTP property accessors to determine portal vs. host SMTP values 
            DotNetNuke.Services.Mail.Mail.SendMail(mailFrom: fromEmail,
                                        mailSender: (SMTPPortalEnabled(portalId) ? PortalController.Instance.GetPortal(portalId).Email : Host.HostEmail),
                                        mailTo: toEmail,
                                        cc: string.Empty,
                                        bcc: string.Empty,
                                        replyTo: string.Empty,
                                        priority: DotNetNuke.Services.Mail.MailPriority.Normal,
                                        subject: subject,
                                        bodyFormat: DotNetNuke.Services.Mail.MailFormat.Html,
                                        bodyEncoding: System.Text.Encoding.Default,
                                        body: bodyHTML,
                                        attachments: new List<System.Net.Mail.Attachment>(),
                                        smtpServer: SMTPServer(portalId),
                                        smtpAuthentication: SMTPAuthentication(portalId),
                                        smtpUsername: SMTPUsername(portalId),
                                        smtpPassword: SMTPPassword(portalId),
                                        smtpEnableSSL: EnableSMTPSSL(portalId));
        }

        public static void Send(DotNetNuke.Modules.ActiveForums.Entities.Email message)
        {
            try
            {
                message.Subject = message.Subject.Replace("&#91;", "[");
                message.Subject = message.Subject.Replace("&#93;", "]");

                foreach (var si in message.Recipients.Where(si => si.Email != string.Empty))
                {
                    if (SettingsBase.GetModuleSettings(message.ModuleId).MailQueue)
                        DotNetNuke.Modules.ActiveForums.Controllers.MailQueueController.Add(portalId: message.PortalId, moduleId: message.ModuleId, message.From, emailTo: si.Email, emailSubject: message.Subject, emailBody: message.BodyHTML, emailBodyPlainText: message.BodyText, emailCC: string.Empty, emailBcc: string.Empty);
                    else
                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendNotification(message.PortalId, message.ModuleId, fromEmail: message.From, toEmail: si.Email, subject: message.Subject, bodyText: message.BodyText, bodyHTML: message.BodyHTML);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        

        #region "code modeled on DotNetNuke.Services.Mail/DotNetNuke.Entities.Host APIs to support portal-specific SMTP configuration"
        internal static string SMTPServer(int portalId)
        {
            return GetSmtpSetting(portalId, "SMTPServer");
        }
        internal static string SMTPAuthentication(int portalId)
        {
            return GetSmtpSetting(portalId, "SMTPAuthentication");
        }
        internal static bool EnableSMTPSSL(int portalId)
        {
            if (SMTPPortalEnabled(portalId))
            {
                return PortalController.GetPortalSettingAsBoolean("SMTPEnableSSL", portalId, false);
            }
            else
            {
                return HostController.Instance.GetBoolean("SMTPEnableSSL", false);
            }
        }
        internal static string SMTPUsername(int portalId)
        {
            return GetSmtpSetting(portalId, "SMTPUsername");
        }
        internal static string SMTPPassword(int portalId)
        {
            if (SMTPPortalEnabled(portalId))
            {
                return PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey());
            }
            else
            {
                string decryptedText;
                try
                {
                    decryptedText = HostController.Instance.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
                }
                catch (Exception)
                {
                    //fixes case where smtppassword failed to encrypt due to failing upgrade
                    var current = HostController.Instance.GetString("SMTPPassword");
                    if (!string.IsNullOrEmpty(current))
                    {
                        HostController.Instance.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                        decryptedText = current;
                    }
                    else
                    {
                        decryptedText = string.Empty;
                    }
                }
                return decryptedText;
            }
        }

        internal static bool SMTPPortalEnabled(int portalId)
        {
            if (portalId != null && portalId != -1)
            {
                return PortalController.GetPortalSetting("SMTPmode", portalId, Null.NullString).Equals("P", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        internal static string GetSmtpSetting(int portalId, string settingName)
        {
            if (SMTPPortalEnabled(portalId))
            {
                return PortalController.GetPortalSetting(settingName, portalId, Null.NullString);
            }
            else
            {
                return HostController.Instance.GetString(settingName);
            }
        }
        #endregion 
        #region Deprecated
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML).")]
        public static void SendNotification(string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            SendNotification(-1, -1, fromEmail, toEmail, subject, bodyText, bodyHTML);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML).")]
        public static void SendNotification(int portalId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            SendNotification(portalId, -1, fromEmail, toEmail, subject, bodyText, bodyHTML);
        }



#endregion
    }
}