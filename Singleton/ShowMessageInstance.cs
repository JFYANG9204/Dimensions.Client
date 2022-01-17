using System;

namespace Dimensions.Client.Singleton
{
    public sealed class ShowMessageInstance
    {
        private static readonly ShowMessageInstance instance = new ShowMessageInstance();
        private ShowMessageInstance()
        {

        }

        public static ShowMessageInstance GetInstance()
        {
            return instance;
        }

        public Action<string, string> ShowMessageDialog { get; set; }
        public Action<string, string, string> ShowFilePathDialog { get; set; }
    }
}
