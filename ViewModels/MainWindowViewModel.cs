using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Data;
using redmine_notifier.Models;

namespace redmine_notifier.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel(Configuration configuration)
        {
            redmineUrl = configuration.RedmineUrl;
            apiKey = configuration.ApiKey;
        }

        public event EventHandler SaveBtnClicked;

        public event EventHandler CloseBtnClicked;
        
        private string redmineUrl;

        private string apiKey;

        public string RedmineUrl
        {
            get { return redmineUrl; }
            set
            {
                if (value.Length == 0)
                {
                    //  throw new DataValidationException("Redmine Url is required");
                }

                redmineUrl = value;
            }
        }

        public string ApiKey
        {
            get { return apiKey; }
            set
            {
                if (value.Length == 0)
                {
                    //  throw new DataValidationException("Redmine Url is required");
                }

                apiKey = value;
            }
        }


        public void onCloseBtnClicked()
        {
            CloseBtnClicked(this, EventArgs.Empty);
        }

        public void onSaveBtnClicked()
        {
            if (!String.IsNullOrEmpty(redmineUrl) && !String.IsNullOrWhiteSpace(ApiKey))
            {
                var newConfiguration = new Configuration()
                {
                    RedmineUrl = redmineUrl,
                    ApiKey = apiKey
                };

                SaveBtnClicked(this, new ConfigurationEvent(newConfiguration));
            }
        }

    }
}
