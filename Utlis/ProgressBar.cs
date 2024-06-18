using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBLKoWizard.Utlis
{
    public class ProgressBar
    {
        private int _minValue;
        private int _maxValue;
        private int _progressWidth;
        private string _additionalInfo;

        public ProgressBar(int minValue, int maxValue, int progressWidth = 90, string additionalInfo = "")
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _progressWidth = progressWidth;
            _additionalInfo = additionalInfo;
        }

        public void Update(int value)
        {
            float normalizedValue = ((float)value - _minValue) / (_maxValue - _minValue) * 100;

            float progress = normalizedValue / 100;

            int progressChars = (int)(progress * _progressWidth);
            int remainingChars = _progressWidth - progressChars;

            Console.ForegroundColor = ConsoleColor.Yellow;

            string progressBar = "[" + new string('#', progressChars) + new string(' ', remainingChars) + "]";

            string info = $"{_additionalInfo} {normalizedValue:0}%";

            string fullProgressBar = progressBar + " " + info;

            Console.WriteLine(fullProgressBar);
            Console.ResetColor();
        }
    }
}
