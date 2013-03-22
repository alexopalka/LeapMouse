using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
namespace LeapMouse
{
    class LeapMouse
    {
        public static void Main()
        {
            // Create a sample listener and controller
            LeapListener listener = new LeapListener();
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
}
