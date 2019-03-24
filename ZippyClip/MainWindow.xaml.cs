﻿#nullable enable

namespace ZippyClip
{
    using Items;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using ZippyClip.Hotkeys;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void ClipboardNotification_ClipboardUpdate(object sender, EventArgs e)
        {
            PutClipboardContentsInList();
        }

        private void PutClipboardContentsInList()
        {
            ClipboardItems.PushClipboardContents();
        }

        private ClipboardItemCollection ClipboardItems = new ClipboardItemCollection();

        public ObservableCollection<Item> ClipboardHistory =>
            ClipboardItems.Items;

        public Item? SelectedItem { get; set; }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CopySelectedItemToClipboard();
        }

        private void CopySelectedItemToClipboard()
        {
            if (SelectedItem == null)
                return;

            Console.WriteLine("Copied to clipboard: " + SelectedItem);

            SelectedItem.CopyToClipboard();
        }

        private void HideAndPaste()
        {
            Hide();

            ThreadPool.QueueUserWorkItem(sleepAndPaste);

            void sleepAndPaste(object userState)
            {
                Thread.Sleep(100);
                System.Windows.Forms.SendKeys.SendWait("^v");
            }
        }

        private void CopyItemToClipboard(int index)
        {
            if (index < 0 || index >= ClipboardHistory.Count)
                return;

            var item = ClipboardHistory[index];

            item.CopyToClipboard();
        }

        private void ButtonUri_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ClipboardNotification.ClipboardUpdate += ClipboardNotification_ClipboardUpdate;

            RegisterHotkeys();
            Hide();
        }

        private void RegisterHotkeys()
        {
            var hk = new HotkeyHandler(1, KeyModifiers.Alt | KeyModifiers.Control, System.Windows.Forms.Keys.V);

            hk.Pressed += delegate
            {
                WakeUp();
            };
        }

        private void WakeUp()
        {
            CenterWindow();
            Show();
            FocusOnList();
        }

        private void FocusOnList()
        {
            listClipboardItems.Focus();

            if (listClipboardItems.Items.Count > 0)
            {
                listClipboardItems.SelectedIndex = 0;
            }
        }

        private void CenterWindow()
        {
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;

            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }

        private void ButtonCopyItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.DataContext is Item item)
            {
                item.CopyToClipboard();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ButtonPreviewItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.DataContext is Item item)
            {
                PreviewItem(item);
            }
        }

        private void PreviewItem(Item item)
        {
            if (item == null)
                return;

            Console.WriteLine("Preview " + item);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Return:
                    CopySelectedItemToClipboard();
                    HideAndPaste();
                    break;
                default:
                    break;
            }
        }
    }
}
