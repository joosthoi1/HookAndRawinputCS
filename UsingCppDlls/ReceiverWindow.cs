using System;
using System.Diagnostics;
using Linearstar.Windows.RawInput;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UsingCppDlls
{
    class ReceiverWindow : NativeWindow
    {
        private struct DecisionRecord
        {
            public int virtualKeyCode;
            public bool decision;

            public DecisionRecord(int virtualKeyCode, bool decision)
            {
                this.virtualKeyCode = virtualKeyCode;
                this.decision = decision;
            }
        };
        private const int WM_HOOK = 32769;
        private const int WM_INPUT = 0x00FF;
        private const int PM_REMOVE = 0x0001;
        public const int maxWaitingTime = 100;
        private const string secKeyboard = @"\\?\HID#VID_04CA&PID_002F&MI_00#7&12194c2&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}";
        private const string mechKeyboard = @"\\?\HID#VID_046D&PID_C33C&MI_00#7&26c97ae&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}";
        private List<DecisionRecord> decisionBuffer = new List<DecisionRecord>();

        public ReceiverWindow()
        {
            CreateHandle(new CreateParams
            {
                X = 0,
                Y = 0,
                Width = 0,
                Height = 0,
                Style = 0x800000,
            });
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            //System.Diagnostics.Debug.WriteLine(m.LP aram);

            //Debug.WriteLine(x);
            //Debug.WriteLine(Marshal.PtrToStringAnsi(x));
            string output = "";
            int virtualKeyCode;
            bool keyDown;
            bool blockThisHook = false;

            switch (m.Msg)
            {
                case WM_INPUT:
                    RawInputKeyboardData data = (RawInputKeyboardData)RawInputData.FromHandle(m.LParam);

                    virtualKeyCode = data.Keyboard.VirutalKey;
                    keyDown = ((int)data.Keyboard.Flags & 1) == 0;
                    
                    string deviceName = data.Device?.DevicePath;
                    if (String.IsNullOrEmpty(deviceName))
                    {
                        break;
                    }
                    output += $"{virtualKeyCode} - {deviceName} - {deviceName == secKeyboard}\n";
                    if (virtualKeyCode == 0x20 && deviceName == secKeyboard && keyDown)
                    {
                        decisionBuffer.Insert(0, new DecisionRecord(virtualKeyCode, true));
                        PressKey(0x54);
                        PressKey(0x45);
                        PressKey(0x53);
                        PressKey(0x54);

                    }
                    else
                    {
                        decisionBuffer.Insert(0, new DecisionRecord(virtualKeyCode, false));
                    }
                    //m.Result = (IntPtr)0;
                    break;
                     
                case WM_HOOK:
                    //var ver = Marshal.PtrToStructure(m.LParam, typeof(HookStruct));
                    IntPtr x = m.LParam;
                    long lParam = x.ToInt64();
                    keyDown = (lParam & 0x80000000) == 0;
                    virtualKeyCode = (int)m.WParam;
                    if (!keyDown)
                    {
                        return;
                    }
                    Debug.WriteLine(virtualKeyCode);
                    bool recordFound = false;
                    int index = 1;
                    if (decisionBuffer.Count > 0)
                    {

                        foreach (DecisionRecord decision in decisionBuffer)
                        {

                            output += $"{decision.virtualKeyCode}, {decision.decision}";
                            if (decision.virtualKeyCode == virtualKeyCode)
                            {
                                blockThisHook = decision.decision;
                                recordFound = true;

                                for (int i = 0; i < index; ++i)
                                {
                                    decisionBuffer.RemoveAt(decisionBuffer.Count-1);
                                }
                                break;
                            }
                            ++index; 
                        }
                    }
                    
                    Debug.WriteLine(recordFound);
                    uint currentTime, startTime;
                    startTime = (uint)Environment.TickCount;
                    //output += startTime;
                    
                    while (!recordFound)
                    {
                        Message rawMessage = new Message();

                        while (!PeekMessage(out rawMessage, Handle, WM_INPUT, WM_INPUT, PM_REMOVE))
                        {
                            currentTime = (uint)Environment.TickCount;
                            if (startTime - currentTime > maxWaitingTime)
                            {
                                Debug.WriteLine("Hook timed out");
                                return;
                            }
                        };
                        Debug.WriteLine("HOOK DID NOT TIME OUT");
                        recordFound = true;
                    }
                    break;
            }

            if (blockThisHook)
            {

                m.Result = (IntPtr)1;
            }

            if (output.Length > 0) Debug.WriteLine(output);

            /// Block input
            /// m.Result = (IntPtr)1;
        }

        void PressKey(byte keyCode)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event(keyCode, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(keyCode, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PeekMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin,
   uint wMsgFilterMax, uint wRemoveMsg);


        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    }
}
