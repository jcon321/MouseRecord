namespace jMouse
{

    public partial class Form1 : Form
    {
        //This statement below adds the external function mouse_event to my project, which provides methods  
        //for left click, right click, middle click, wheel, etc...
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        //These constant variables are used by mouse_event to know which method you want, for a full list see
        //http://pinvoke.net/default.aspx/user32/mouse_event.html
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;

        //These variables are the arrays which will hold the X and Y coordinate for each mouse click
        //and mouse time, which records the time of day mouse was clicked in ms, and a change in time from 
        //previous click array. Each array needs a special pointer, although they are parallel some arrays will
        //need to start at different positions because there will be no previous click time when first starting the 
        //recording method.
        int arraySize = 500;
        int[] recordMouseX = new int[arraySize];
        int[] recordMouseY = new int[arraySize];
        long[] recordMouseTime = new long[arraySize];
        int[] recordMouseDelta = new int[arraySize];
        int xpntr = 0;
        int ypntr = 0;
        int tpntr = 0;
        int dpntr = 1;

        //These arrays are printed to a log file, just in case one might want to copy/paste to another recording
        //software
        StreamWriter log = new StreamWriter("jlogfile.txt");

        public Form1()
        {
            InitializeComponent();
            //This line adds the MouseMove Hook function into our program 
            HookManager.MouseMove += HookManager_MouseMove;
        }

        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            //The line that is ran every time the mouse moves.
            lblMouse.Text = e.X + "," + e.Y;
        }

        private void HookManager_MouseDown(object sender, MouseEventArgs e)
        {
            //This function is ran every time the mouse is clicked after the record button has been pushed.
            //The position of the mouse is recorded into an array and the pointers are updated accordingly.
            recordMouseX[xpntr] = e.X;
            recordMouseY[ypntr] = e.Y;
            xpntr++;
            ypntr++;

            //This array holds the time of day in milliseconds, used to build the delta(change in) time array
            recordMouseTime[tpntr] = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            tpntr++;
            //The reason for the if statement is so that the change in time is not recorded for the very first click.
            if (xpntr > 1)
            {
                recordMouseDelta[dpntr] = (int)(recordMouseTime[tpntr - 1] - recordMouseTime[tpntr - 2]);
                dpntr++;
            }
            //The reason for the pointer - 1 is because of the final click of the recording (the stop button)
            //we do not want to keep that value.
            log.WriteLine(recordMouseX[xpntr - 1] + " , " + recordMouseY[ypntr - 1] + " | " + recordMouseDelta[dpntr - 2]);

        }

        public void MouseClick(int x, int y, int waitMS)
        {
            //This function hosts the ability to provide a mouse click given an X, Y position and the delay in ms
            //used after the click. Why is the waitMS divided into a 10% variable variable? In some
            //games lag is inevitable so a delay is issued right after moving the cursor (to allow it to get there and 
            //recognize) before clicking at the desired location.
            double firstwaitMS = (waitMS * 0.1);
            Cursor.Position = new Point(x, y);
            Thread.Sleep((int)firstwaitMS);
            mouse_event(MOUSEEVENTF_LEFTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
            Thread.Sleep(waitMS);
        }

        private void btn_Record_Click(object sender, EventArgs e)
        {
            //When the record button is clicked, the MouseDown function is hooked.
            HookManager.MouseDown += HookManager_MouseDown;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            log.Close();
            HookManager.MouseDown -= HookManager_MouseDown;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            //This while loop plays back the recorded mouse click pattern.
            while (true)
            {
                int x = 0;
                while (x < xpntr - 1)
                {
                    MouseClick(recordMouseX[x], recordMouseY[x], recordMouseDelta[x + 1]);
                    x++;
                }
            }
        }
    }
}