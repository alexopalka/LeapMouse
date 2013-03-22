using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Leap;

class LeapListener : Listener
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


    private Object thisLock = new Object();

    private void SafeWriteLine(String line)
    {
        lock (thisLock)
        {
            Console.WriteLine(line);
        }
    }
    public override void OnInit(Controller controller)
    {
        IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
        ShowWindow(h, 0);
        controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        SafeWriteLine("Initialized");
    }

    public override void OnConnect(Controller controller)
    {
        SafeWriteLine("Connected");
    }

    public override void OnDisconnect(Controller controller)
    {
        SafeWriteLine("Disconnected");
    }

    public override void OnExit(Controller controller)
    {
        SafeWriteLine("Exited");
    }

    public Int64 prevTime;
    public Int64 currentTime;
    public Int64 changeTime;
    public Frame currentFrame;
    public Frame prevFrame;
    public override void OnFrame(Controller controller)
    {
        currentFrame = controller.Frame();
        prevFrame = controller.Frame(10);
        currentTime = currentFrame.Timestamp;
        changeTime = currentTime - prevTime;

        if (changeTime > 500)
        {
            Hand hand = currentFrame.Hands[0];
            FingerList fingers = hand.Fingers;
            Hand prevHand = prevFrame.Hands[0];
            FingerList prevFingers = prevHand.Fingers;
            Pointable currentPointable = currentFrame.Pointables[0];
            Pointable prevPointable = prevFrame.Pointables[0];
            // Set Up Screen
            Leap.Screen currentScreen = controller.CalibratedScreens.ClosestScreenHit(currentPointable);
            Leap.Screen prevScreen = controller.CalibratedScreens.ClosestScreenHit(prevPointable);
            // Set up Display             
            float DisplayHeight = currentScreen.HeightPixels;
            float DisplayWidth = currentScreen.WidthPixels;
            double scalingFactor = 1.5;
            // Set up Coordinates
            float CurrentLeapXCoordinate = currentScreen.Intersect(currentPointable, true, 1.0F).x;
            float CurrentLeapYCoordinate = currentScreen.Intersect(currentPointable, true, 1.0F).y;
            int CurrentyPixel = (int)(DisplayHeight - (DisplayHeight * (scalingFactor * CurrentLeapYCoordinate)));
            int CurrentxPixel = (int)(DisplayWidth * (scalingFactor * CurrentLeapXCoordinate));
            float PrevLeapXCoordinate = currentScreen.Intersect(prevPointable, true, 1.0F).x;
            float PrevLeapYCoordinate = currentScreen.Intersect(prevPointable, true, 1.0F).y;
            int PrevyPixel = (int)(DisplayHeight - (DisplayHeight * (scalingFactor * PrevLeapYCoordinate)));
            int PrevxPixel = (int)(DisplayWidth * (scalingFactor * PrevLeapXCoordinate));
            float changeyPixel = (PrevyPixel - CurrentyPixel);
            float changexPixel = (PrevxPixel - CurrentxPixel);
            if (changeyPixel < 0) { changeyPixel = changeyPixel * -1; }
            if (changexPixel < 0) { changexPixel = changexPixel * -1; }
            bool allowfalse = false;
            if (changeyPixel > 10) { allowfalse = true; }
            if (CurrentyPixel < 0) { CurrentyPixel = 0; }
            if (CurrentxPixel < 0) { CurrentxPixel = 0; }
            if (allowfalse)
            {
                if (prevFingers.Count != 2 || prevFingers.Count != 3)
                {

                    if (fingers.Count == 1)
                    {
                        if (changeTime > 500)
                        {
                            Console.Write("TipPosition: " + fingers[0].TipPosition + " Width: " + CurrentxPixel + " height: " + CurrentyPixel + "\n");
                            SetCursorPos(CurrentxPixel, CurrentyPixel);
                        }
                    }
                    if (fingers.Count == 2)
                    {
                        if (changeTime > 1000)
                        {
                            mouse_event(0x0002 | 0x0004, 0, CurrentxPixel, CurrentyPixel, 0);
                            Console.Write("Clicked At " + CurrentxPixel + " " + CurrentyPixel + "\n");
                        }
                    }
                    if (fingers.Count == 4)
                    {
                        if (changeTime > 10000)
                        {

                            float Translation = currentFrame.Translation(prevFrame).y;
                            Console.WriteLine(Translation);
                            if (Translation < -50)
                            {
                                SendKeys.SendWait("{LEFT}");
                                System.Threading.Thread.Sleep(700);
                            }
                            if (Translation > 50)
                            {
                                SendKeys.SendWait("{Right}");
                                System.Threading.Thread.Sleep(700);
                            }
                        }
                    }
                }
            }
        }
        prevTime = currentTime;
    }
}

