/******************************************************************************\
* Copyright (C) 2012-2013 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using System;
using System.Threading;
using System.Diagnostics;
using Leap;
using System.Runtime.InteropServices;

class SampleListener : Listener
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
        SafeWriteLine("Initialized");

        IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
        ShowWindow(h, 0);
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

    public override void OnFrame(Controller controller)
    {
        Frame currentFrame = controller.Frame();
        Hand hand = currentFrame.Hands[0];
        FingerList fingers = hand.Fingers;
        Pointable pointable = currentFrame.Pointables[0];
        Leap.Screen screen = controller.CalibratedScreens.ClosestScreenHit(pointable);

        Frame prevFrame = controller.Frame(10);
        Hand prevhand = prevFrame.Hands[0];
        FingerList prevfingers = prevhand.Fingers;
        Pointable prevpointable = prevFrame.Pointables[0];
        ScreenList screenList = controller.CalibratedScreens;
        Leap.Screen prevscreen = screenList.ClosestScreenHit(prevpointable);

        if (!fingers.Empty)
        {
            float prevwidth = prevscreen.Intersect(prevpointable, true, 1.0F).x * prevscreen.WidthPixels;
            float prevheight = prevscreen.Intersect(prevpointable, true, 1.0F).y * prevscreen.HeightPixels;
            float width = screen.Intersect(pointable, true, 1.0F).x * screen.WidthPixels;
            float height = screen.Intersect(pointable, true, 1.0F).y * screen.HeightPixels;

           float tranX =  currentFrame.Translation(prevFrame).x;
           float tranY = currentFrame.Translation(prevFrame).y;    
       
            int fwidth = (int)((width * 0.2) + (prevwidth * (1.0 - 0.2)));
            int fheight = (int)((height * 0.2) + (prevheight * (1.0 - 0.2)));

            fheight = screen.HeightPixels - fheight;
            if (fingers.Count == 2 || fingers.Count == 3)
            {
                if (fingers.Count == 2)
                {
                    mouse_event(0x0002 | 0x0004, 0, fwidth, fheight, 0);
                }
                else
                {
                    mouse_event(0x0008 | 0x0010, 0, fwidth, fheight, 0);
                }

            }
            else
            {
                Console.Write(fingers[0].TipPosition + " " + width + " " + height + " " + tranX + " " + tranY + "\n");
                SetCursorPos(fwidth, fheight);
            }
        }
    }
}

class Sample
{
    public static void Main()
    {


        // Create a sample listener and controller
        SampleListener listener = new SampleListener();
        Controller controller = new Controller();


        // Have the sample listener receive events from the controller
        controller.AddListener(listener);

        // Keep this process running until Enter is pressed
        Console.WriteLine("Press Enter to quit...");
        Console.ReadLine();

        // Remove the sample listener when done
        controller.RemoveListener(listener);
        controller.Dispose();
    }
}
