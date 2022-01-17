using MahApps.Metro.Controls.Dialogs;
using System;

namespace Dimensions.Client.Singleton
{
    public sealed class EditSettingDialogInstance
    {
        private static readonly EditSettingDialogInstance _instance = new EditSettingDialogInstance();
        private EditSettingDialogInstance()
        {
        }

        public static EditSettingDialogInstance GetInstance()
        {
            return _instance;
        }

        public IDialogCoordinator DialogCoordinator { get; set; }

        public Action<string, string> ShowEditKeysDialog { get; set; }

        public Action ShowEditFixedKeysDialog { get; set; }
    }
}
