using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Dimensions.Client.ViewModels
{
    public class SettingDialogContent : ViewModelBase
    {
        public SettingDialogContent(Action confirmCommand, Action cancelCommand)
        {
            ConfirmCommand = new RelayCommand(confirmCommand);
            CancelCommand = new RelayCommand(cancelCommand);
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { Set(ref _content, value); }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
    }
}
