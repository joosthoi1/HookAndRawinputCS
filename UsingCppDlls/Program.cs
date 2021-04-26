using Linearstar.Windows.RawInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UsingCppDlls
{
    /// TODO:
    /// 1. Send rawinput messages to window.handle
    /// 2. Send message to block input


    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool createMacros = false;

            //System.Diagnostics.Debug.WriteLine($"{xxx++} - {virtualKeyCode}");
            // Form1 f = new Form1();
            if (createMacros)
            {
                try
                {
                    Form1 form1 = new Form1();
                    RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, form1.Handle);

                    Application.Run();
                }
                finally
                {
                    UninstallHook();
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
                }
            }
            else
            {
                try
                {
                    ReceiverWindow window = new ReceiverWindow();
                    RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, window.Handle);

                    InstallHook(window.Handle);

                    Application.Run();
                }
                finally
                {
                    UninstallHook();
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
                }
            }
            


        }

        [DllImport(@"C:\Users\Joost\source\cpp\HookingInputDLL\x64\Debug\HookingRawInputDemoDLL.dll")]
        static extern bool InstallHook(IntPtr hwdnParent);
        [DllImport(@"C:\Users\Joost\source\cpp\HookingInputDLL\x64\Debug\HookingRawInputDemoDLL.dll")]
        static extern bool UninstallHook();
    }
}
