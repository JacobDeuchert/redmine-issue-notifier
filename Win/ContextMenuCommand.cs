using System;
using System.Windows.Input;

namespace redmine_notifier.Win
{
    public class ContextMenuCommand : ICommand
    {

        public event EventHandler CanExecuteChanged; 

        public event EventHandler Executed;

        public void Execute(object args)
        {
          Executed(this, EventArgs.Empty);
        }

        public bool CanExecute(object args)
        {
          return true;            
        }


    }
}