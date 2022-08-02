using System;
using Microsoft.Toolkit.Uwp.Notifications;
using System.IO;
using Avalonia;
using Avalonia.Controls.Notifications;
using redmine_notifier.Models;
using System.Diagnostics;

namespace redmine_notifier.Services
{
    public class NotificationService
    {
        private ConfigurationService configurationService;

        private INotificationManager notificationManager;

        private LoggingService loggingService;
        public NotificationService(ConfigurationService configurationService, LoggingService loggingService)
        {
            this.configurationService = configurationService;
            this.loggingService = loggingService;

            ToastNotificationManagerCompat.OnActivated += this.NotificationActivated;
        }
        public void ShowIssuesNotification(Issue issue)
        {

            var redmineLogo = new Uri(Directory.GetCurrentDirectory() + "/Assets/redmine-logo.ico");
            new ToastContentBuilder()
                .AddAppLogoOverride(redmineLogo)
                .AddText("New Issue: " + issue.subject)
                .AddText(issue.author.name)
                .AddArgument(issue.id.ToString())
                .Show();
        }


        public void NotificationActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            try
            {
                string id = e.Argument;

                Configuration configuration = configurationService.Configuration;

                var issueUrl = configuration.RedmineUrl + "/issues/" + id;

                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c start " + issueUrl 
                };
                Process.Start(processInfo);

            }
            catch (Exception exception)
            {

                loggingService.Log(exception.ToString());

            }

        }
    }
}