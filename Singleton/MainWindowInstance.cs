using System;

namespace Dimensions.Client.Singleton
{
    public class MainWindowInstance
    {
        private MainWindowInstance() { }
        
        private readonly static MainWindowInstance _instance = new MainWindowInstance();
        public static MainWindowInstance GetInstance() { return _instance; }

        public Action OnTextEditorPageClosing { get; set; }

    }
}
