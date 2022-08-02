using System;
using System.IO;
using redmine_notifier.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace redmine_notifier.Services
{
    public class ConfigurationService
    {

        public Configuration Configuration => configuration;

        private Configuration configuration;


        public ConfigurationService()
        {
            _loadConfiguration();
        }

        public void SaveConfiguration(Configuration configuration)
        {
            _storeConfiguration(configuration);
        }


        private void _storeConfiguration(Configuration configuration)
        {
            var json = JsonSerializer.Serialize(configuration);
            File.WriteAllText("./configuration.json", json);      
        }


        private void _loadConfiguration()
        {
            try
            {
                var json = File.ReadAllText("./configuration.json");
                configuration = JsonSerializer.Deserialize<Configuration>(json);              
            } catch (Exception ex)
            {
                configuration = new Configuration();
                
            }
            
        }
    }
}