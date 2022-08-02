using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using redmine_notifier.Models;
using redmine_notifier.Services;
using redmine_notifier.ViewModels;
using redmine_notifier.Views;
using redmine_notifier.Win;

namespace redmine_notifier
{
    public class App : Application
    {

        private MainWindow mainWindow;

        private NotifyIcon notifyIcon;

        private RedmineService redmineService;
        private NotificationService notificationService;

        private ConfigurationService configurationService;

        private LoggingService loggingService;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Console.WriteLine("Init");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            createNotifyIcon();

            loggingService = new LoggingService();

            configurationService = new ConfigurationService();
            notificationService = new NotificationService(configurationService, loggingService);
            redmineService = new RedmineService(notificationService, loggingService);

            if (!String.IsNullOrWhiteSpace(configurationService.Configuration?.RedmineUrl)) 
            {
                startIssueChecker(configurationService.Configuration);
            }

            

            base.OnFrameworkInitializationCompleted();
        }

        public void EventClicked(object? sender, EventArgs e)
        {
            createMainWindow();
        }
        
        private void createMainWindow()
        {
            if (mainWindow != default(MainWindow)) 
            {  
               mainWindow.Show();
               mainWindow.ActivateWorkaround();
               return;          
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                var viewModel = new MainWindowViewModel(configurationService.Configuration);
                
                viewModel.SaveBtnClicked += this.newConfigurationSaved;
                viewModel.CloseBtnClicked += this.hideWindow;

                var configuration = 
                mainWindow = new MainWindow()
                {
                    DataContext = viewModel,
                    ShowInTaskbar = false,
                    ShowActivated = true        
                };

                desktop.MainWindow = mainWindow;
                desktop.MainWindow.Show();

                mainWindow.Deactivated += this.hideWindow;
            }  
        }

        private void createNotifyIcon()
        {
            if (notifyIcon != null)
            {
                return;
            }

            var contextMenu = new ContextMenu();

            var menuItems = new List<MenuItem>();

            var commandHandler = new ContextMenuCommand();

            commandHandler.Executed += this.contextMenuClosed_Click;

            var menuItem = new MenuItem()
            {
                Header = "Close",
                Command = commandHandler
            };

            menuItem.Click += this.contextMenuClosed_Click;

            menuItems.Add(menuItem);

            contextMenu.Items = menuItems;      
            

            notifyIcon = new NotifyIcon()
            {
                IconPath = "./Assets/redmine-logo.ico",
                Visible = true,
                ToolTipText = "Redmine Notifier",
                ContextMenu = contextMenu
            };
            notifyIcon.Click += this.EventClicked;
        } 


        private void newConfigurationSaved(object? sender, EventArgs e)
        {

            if (e is ConfigurationEvent configurationEvent)
            {
                Configuration newConfiguration = configurationEvent.configuration;
                startIssueChecker(newConfiguration);
            }
            
        }


        private async Task startIssueChecker(Configuration configuration)
        {
            var valid = await redmineService.CheckConfiguration(configuration);

            if (valid)
            {
                configurationService.SaveConfiguration(configuration);
                redmineService.StartWatchingIssues(configuration);
            }
            else
            {

            }
        }

        private void hideWindow(object? sender, EventArgs e)
        {
            mainWindow.Hide();

        }


        private void contextMenuClosed_Click(object? sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}