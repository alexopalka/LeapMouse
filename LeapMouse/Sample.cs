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

    public Int64 prevTime;
    public Int64 currentTime;
    public Int64 changeTime;

    public Frame currentFrame;
    public Frame previousFrame;
    public HandList currentHands;
    public HandList previousHands;

    public override void OnInit(Controller controller)
    {


        SafeWriteLine("Initialized");
        //IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
        //ShowWindow(h, 0);
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
           frame currentframe = controller.frame();
           currenttime = currentframe.timestamp;
           changetime = currenttime - prevtime;
           if (changetime > 10000)
           {
               hand hand = currentframe.hands[0];
               fingerlist fingers = hand.fingers;
               pointable pointable = currentframe.pointables[0];
               leap.screen screen = controller.calibratedscreens.closestscreenhit(pointable);
               frame prevframe = controller.frame(10);
               hand prevhand = prevframe.hands[0];
               fingerlist prevfingers = prevhand.fingers;
               pointable prevpointable = prevframe.pointables[0];
               screenlist screenlist = controller.calibratedscreens;
               leap.screen prevscreen = screenlist.closestscreenhit(prevpointable);

               if (!fingers.empty)
               {
                   float prevwidth = prevscreen.intersect(prevpointable, true, 1.0f).x * prevscreen.widthpixels;
                   float prevheight = prevscreen.intersect(prevpointable, true, 1.0f).y * prevscreen.heightpixels;
                   float width = screen.intersect(pointable, true, 1.0f).x * screen.widthpixels;
                   float height = screen.intersect(pointable, true, 1.0f).y * screen.heightpixels;
                   float tranx = currentframe.translation(prevframe).x;
                   float trany = currentframe.translation(prevframe).y;
                   int fwidth = (int)((width * 0.2) + (prevwidth * (1.0 - 0.2)));
                   int fheight = (int)((height * 0.2) + (prevheight * (1.0 - 0.2)));
                   fheight = screen.heightpixels - fheight;
                   if (fingers.count == 2 || fingers.count == 3)
                   {
                       if (changetime > 8000)
                       {
                           if (fingers.count == 2)
                           {
                               mouse_event(0x0002 | 0x0004, 0, fwidth, fheight, 0);
                           }
                           else
                           {
                               mouse_event(0x0008 | 0x0010, 0, fwidth, fheight, 0);
                           }
                       }
                   }
                   else
                   {
                       console.write(fingers[0].tipposition + " " + width + " " + height + " " + tranx + " " + trany + "\n");
                       setcursorpos(fwidth, fheight);
                   }
               }
               prevtime = currenttime;
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

        listener.prevTime = controller.Frame().Timestamp;

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