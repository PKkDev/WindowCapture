using Windows.Devices.Enumeration;

namespace WindowCapture.WinApp.MVVM.Model
{
    public sealed class MicrophoneInfo
    {
        public DeviceInformation Microphone { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }

        public MicrophoneInfo(DeviceInformation microphone, bool isActive)
        {
            Microphone = microphone;
            IsActive = isActive;
            Name = microphone.Name;
        }
    }
}
