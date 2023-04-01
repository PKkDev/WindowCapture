using Microsoft.UI.Xaml;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WindowCapture.WinApp.Dilogs.CaptureItemSelect.Tabs;

namespace WindowCapture.WinApp.Dilogs.CaptureItemSelect
{
    public sealed partial class CapureItemSelectorWindow : Window
    {

        public string Test { get; set; }

        public CapureItemSelectorWindow()
        {
            InitializeComponent();
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var navItemTag = args.InvokedItemContainer.Tag.ToString();

            Type _page = navItemTag switch
            {
                "MonitorContent" => typeof(MonitorCaptureItemPage),
                "WindowContent" => typeof(WindowCaptureItemPage),
                "MouseHookContent" => typeof(WindowCaptureItemPage),
                _ => throw new NotImplementedException(),
            };

            ContentFrame.Navigate(_page, null, new DrillInNavigationTransitionInfo());
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
