namespace FluidSimulation
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new shapeWindow());


        }


    }

    public class shapeWindow : Form
    {
        public shapeWindow()
        {
            this.Text = "Fluid Simulation";
            this.Size = new Size(300, 300);
            this.BackColor = Color.White;

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; // 60fps
            timer.Tick += (s, e) =>
            {
                
                // Update all shape positions here
                Invalidate();
            };
            timer.Start();
            
            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.FillEllipse(Brushes.Red, 50, 50, 100, 100);
        }



    }
}