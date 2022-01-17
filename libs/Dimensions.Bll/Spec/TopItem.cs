
namespace Dimensions.Bll.Spec
{
    public class TopItem : ITopItem
    {
        public TopItem()
        {
            Code = string.Empty;
            Label = string.Empty;
            Definition = string.Empty;
        }

        public string Code { get; private set; }

        public string Label { get; private set; }

        public string Definition { get; private set; }

        public void SetProperty(string code, string label)
        {
            Code = code;
            Label = label;
        }

        public void SetProperty(string definition)
        {
            Definition = definition;
        }
    }
}
