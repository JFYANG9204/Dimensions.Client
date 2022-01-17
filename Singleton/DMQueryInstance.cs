using Dimensions.Bll.Mdd;
using System;
using System.Collections.ObjectModel;

namespace Dimensions.Client.Singleton
{
    public class DMQueryInstance
    {
        private DMQueryInstance()
        {
            MddLoaded = false;
        }

        private static readonly DMQueryInstance _instance = new DMQueryInstance();
        public static DMQueryInstance GetInstance() { return _instance; }

        public Action ShowDMQuerySettingFlyout { get; set; }
        public Action CloseDMQuerySettingFlyout { get; set; }
        public Action<object> SetProperty { get; set; }
        public Action LoadData { get; set; }
        public Action Clear { get; set; }
        public Action SetQuery { get; set; }
        public Action RemoveFilterField { get; set; }
        
        public Func<string> GetFilterString { get; set; }
        

        public IMddDocument Mdd { get; set; }
        public ObservableCollection<string> Fields { get; set; }
        public bool MddLoaded { get; set; }
        public string QueryString { get; set; }
    }
}
