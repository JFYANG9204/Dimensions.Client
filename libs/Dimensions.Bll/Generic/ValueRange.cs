
namespace Dimensions.Bll.Generic
{
    public class ValueRange
    {
        public ValueRange()
        {
            _min = string.Empty;
            _max = string.Empty;
            _rangeExp = string.Empty;
        }

        public ValueRange(string min, string max, string rangeExp = "")
        {
            _min = min;
            _max = max;
            _rangeExp = rangeExp;
        }

        private string _min;
        private string _max;
        private string _rangeExp;

        public string Min { get { return _min; } }
        public string Max { get { return _max; } }

        public void SetValue(string rangeExp)
        {
            _rangeExp = rangeExp;
        }

        public void SetValue(string min, string max)
        {
            _min = min;
            _max = max;
        }

        public string Get()
        {
            if (!string.IsNullOrEmpty(_rangeExp)) return _rangeExp;
            return $"[{_min}..{_max}]";
        }
    }
}
