using System;

namespace redmine_notifier.Models
{
    public class ConfigurationEvent : EventArgs
    {
        public Configuration configuration;

        public ConfigurationEvent(Configuration configuration)
        {
            this.configuration = configuration;

        }
    }
}