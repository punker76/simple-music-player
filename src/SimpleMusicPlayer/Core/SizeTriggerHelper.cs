using ReactiveUI;

namespace SimpleMusicPlayer.Core
{
    public class SizeTriggerHelper : ReactiveObject
    {
        private static SizeTriggerHelper _instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static SizeTriggerHelper()
        {
        }

        private SizeTriggerHelper()
        {
        }

        public static SizeTriggerHelper Instance
        {
            get { return _instance ??= new SizeTriggerHelper(); }
        }

        private string size;

        public string Size
        {
            get => size;
            set => this.RaiseAndSetIfChanged(ref size, value);
        }

        public void CalcSize(double actualWidth, double actualHeight)
        {
            if (actualWidth > 850 && actualHeight > 690) Size = "Large";
            else if (actualWidth > 560) Size = "Medium";
            else Size = "Small";
        }
    }
}