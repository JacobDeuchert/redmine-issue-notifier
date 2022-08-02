using System;
using System.Text;
using redmine_notifier.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace redmine_notifier.Services
{
    public class RedmineService
    {   

        private CancellationTokenSource currentCancellationToken;

        private DateTime lastChecked;

        private NotificationService notificationService;

        private LoggingService loggingService;

        public RedmineService(NotificationService notificationService, LoggingService loggingService)
        {
            lastChecked = DateTime.UtcNow;
            
            this.notificationService = notificationService;
            this.loggingService = loggingService;

        }

        public async Task<bool> CheckConfiguration(Configuration configuration)
        {
            try
            {
                var user = await getOwnUser(configuration);
                if (user != null)
                {
                    return true;
                }
            } catch (Exception e)
            {
                loggingService.Log(e.Message);
            }
            return false;
        }

        public void StartWatchingIssues(Configuration configuration)
        {
            if (currentCancellationToken != null)
            {
                currentCancellationToken.Cancel();
            }

            var cancellationToken = new CancellationTokenSource();


            Task.Run(() => watchIssues(cancellationToken.Token, configuration));

        }

        private async Task watchIssues(CancellationToken cancellationToken, Configuration configuration)
        {
            
            var ownUser = await getOwnUser(configuration);

            while(true)
            {
                try
                {
                    await checkForNewIssues(configuration, ownUser);
                } catch (Exception e)
                {
                    loggingService.Log(e.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(10));


            }
        }

        private async Task checkForNewIssues(Configuration configuration, User currentUser)
        {
            var issues = await getIssues(configuration, lastChecked);

            lastChecked = DateTime.UtcNow;

            if (issues != null && issues.Any(x => x.author.id != currentUser.id))
            {
                var issue = issues.FirstOrDefault(x => x.author.id != currentUser.id);
                notificationService.ShowIssuesNotification(issue);
            }            
        }



        private async Task<List<Issue>> getIssues(Configuration configuration, DateTime? lastRequested = null)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", configuration.ApiKey);

                    var issuesUrl = configuration.RedmineUrl + "/issues.json";

                    if (lastRequested.HasValue)
                    {
                        var formattedDate = lastRequested.Value.ToString(@"yyyy-MM-dd\THH:mm:ss\Z");
                        issuesUrl += "?created_on=>=" + formattedDate;
                    }

                    var response = await httpClient.GetAsync(issuesUrl);

                    string issueResponseJson = await response.Content.ReadAsStringAsync();

                    var issueResponse = JsonSerializer.Deserialize<IssueResponse>(issueResponseJson);

                    return issueResponse.issues;
                }
            } catch (Exception e)
            {
                throw new Exception("Failed to request issues: " + e);
            }
        }


        private async Task<User> getOwnUser(Configuration configuration)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", configuration.ApiKey);
                    var userUrl = configuration.RedmineUrl + "/users/current.json";
                    var response = await httpClient.GetAsync(userUrl);
                    
                    string userResponseJSON = await response.Content.ReadAsStringAsync();

                    UserResponse userResponse = JsonSerializer.Deserialize<UserResponse>(userResponseJSON);

                    return userResponse.user;
                } 
            } catch (Exception e)
            {
                throw new Exception("Failed to get own User: " + e);
            }
            
        }





    }
}