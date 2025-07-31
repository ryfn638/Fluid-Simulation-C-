using System;
using System.CodeDom.Compiler;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;

namespace FluidSimulation
{
    public class Program
    {
        Particle[] all_particles;
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

            // Implement the boundary system and the updating fluid screen DONE
            // Steps Once this is implemented
            // 1. Randomy generate a set of points with dimensions W, H. Done
            // 2. Pre Generate all of the density values for these points Done
            // 3. Calculate the neighbors for all points Done
            // 4. Calculate the pressure force Done
            // ONCE ALL OF THIS IS FINISHEd
            // - Debug this
            // - Optimise this (do a bit more research if needed)
            // - Make into 3D sim
        }

        public Particle[] GenerateParticles(int num_particles, int width, int height)
        {
            Particle[] all_particles = new Particle[num_particles];
            Random random = new Random();

            for (int i = 0; i < num_particles; i++)
            {
                // Assign default properties
                // sample smoothing radius
                all_particles[i] = new Particle();
                all_particles[i].smoothing_radius = 30;
                all_particles[i].visual_radius = 5;

                
                int random_x = random.Next(0, width);
                int random_y = random.Next(0, height);

                all_particles[i].position = new Vector2();
                all_particles[i].position.X = random_x;
                all_particles[i].position.Y = random_y;
                all_particles[i].mass = 1;



               
            }

            return all_particles;

        }

        public void MoveTick(Particle movedParticle, float tickRate, int width, int height)
        {
            // Collision dampening coefficient, TUNEABLE
            float collision_dampening = 0.9f;

            // Update velocity vectors of each particle
            movedParticle.velocity += movedParticle.vectors * tickRate;
            movedParticle.velocity *= .99f;
            movedParticle.position += movedParticle.velocity * tickRate;


            movedParticle.position.X = Math.Clamp(movedParticle.position.X, movedParticle.visual_radius, width - movedParticle.visual_radius);
            movedParticle.position.Y = Math.Clamp(movedParticle.position.Y, movedParticle.visual_radius, height - movedParticle.visual_radius);



            // Flipping the speed if hits the border
            if (movedParticle.position.X + movedParticle.visual_radius >= width ||
                movedParticle.position.X - movedParticle.visual_radius <= 0)
            {
                movedParticle.velocity.X *= -1 * collision_dampening;
            }

            if (movedParticle.position.Y + movedParticle.visual_radius >= height ||
                movedParticle.position.Y - movedParticle.visual_radius <= 0)
            {
                movedParticle.velocity.Y *= -1 * collision_dampening;
            }

        }

        public class shapeWindow : Form
        {
            Particle[] all_particles;
            private bool isMouseDown = false;
            private System.Windows.Forms.Timer holdTimer;
            private MouseEventArgs lastMouseEvent;

            private float gravity = 0f;
            private float pressure_multiplier = 200f;
            private float rest_density = 10f;

            private SpatialHash spatialHash;
            public shapeWindow()
            {

                int particle_nums = 1000;
                int width = 600;
                int height = 900;



                this.Text = "Fluid Simulation";
                this.Size = new Size(width+300, height+35);
                this.BackColor = Color.Black;

                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.UpdateStyles();


                this.MouseDown += Form1_MouseDown;
                this.MouseUp += Form1_MouseUp;
                this.MouseMove += Form1_MouseMove;

                holdTimer = new System.Windows.Forms.Timer();
                holdTimer.Interval = 1; // milliseconds
                holdTimer.Tick += HoldTimer_Tick;


                Program newProgram = new Program();
                this.all_particles = newProgram.GenerateParticles(particle_nums, width, height);


                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 16; // 60fps, this is 16ms hence why timer.interval does 16/1000
                timer.Tick += (s, e) =>
                {


                    // Update all particles with forces and whatnot
                    ParticleTick(this.all_particles, height, width);


                    for (int i = 0; i < this.all_particles.Length; i++)
                    {
                        newProgram.MoveTick(this.all_particles[i], 0.064f, width, height);

                    }

                    Vector2 avgVelocity = Vector2.Zero;
                    Vector2 avgPos = Vector2.Zero;

                    // Update all shape positions here
                    Invalidate();
                };
                timer.Start();

                // Slidebar for gravity
                TrackBar slider = new TrackBar();
                slider.Minimum = 0;
                slider.Maximum = 100;
                slider.Value = 0;
                slider.TickFrequency = 10;
                slider.Orientation = Orientation.Horizontal;
                slider.Width = 200;
                slider.Location = new Point(width+30, 10);


                // Pressure
                TrackBar slider2 = new TrackBar();
                slider2.Minimum = 0;
                slider2.Maximum = 1000;
                slider2.Value = 200;
                slider2.TickFrequency = 10;
                slider2.Orientation = Orientation.Horizontal;
                slider2.Width = 200;
                slider2.Location = new Point(width + 30, 60);

                // rest Density
                TrackBar rest_density = new TrackBar();
                rest_density.Minimum = 1;
                rest_density.Maximum = 100;
                rest_density.Value = 10;
                rest_density.TickFrequency = 5;
                rest_density.Orientation = Orientation.Horizontal;
                rest_density.Width = 200;
                rest_density.Location = new Point(width + 30, 110);


                // Add an event handler for when the value changes
                slider.ValueChanged += (s, e) =>
                {
                    int value = slider.Value;
                    this.gravity = (float)value;
                };


                slider2.ValueChanged += (s, e) =>
                {
                    int value = slider2.Value;
                    this.pressure_multiplier = (float)value;
                };

                rest_density.ValueChanged += (s, e) =>
                {
                    int value = rest_density.Value;
                    this.rest_density = (float)value;
                };


                this.Controls.Add(slider);
                this.Controls.Add(slider2);
                this.Controls.Add(rest_density);


            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;

                for (int i = 0; i < this.all_particles.Length; i++ )
                {
                    float x = this.all_particles[i].position.X - this.all_particles[i].visual_radius;
                    float y = this.all_particles[i].position.Y - this.all_particles[i].visual_radius;

                    float size = this.all_particles[i].visual_radius;


                    float currentSpeed = (float)this.all_particles[i].velocity.Length();

                    float clamped_speed = Math.Clamp(currentSpeed, 0, 80);

                    double red_amount = (clamped_speed / 80);

                    double blue_amount = 1 - red_amount;

                    Color speedColour = Color.FromArgb((int)(255*red_amount), 0, ((int)(255 * blue_amount)));

                    using (SolidBrush brush = new SolidBrush(speedColour))
                    {
                        g.FillEllipse(brush, x, y, size, size); // Draw circle
                    }
                }
            }

            private void ParticleTick(Particle[] all_particle, int height, int width)
            {
                // Add all points to the hash map
                spatialHash = new SpatialHash();
                float smoothing_radius = all_particle[0].smoothing_radius;
                spatialHash.SpatialHashGrid((int)smoothing_radius, all_particle, height, width);

                // Calculate the neighbours first
                List<Particle[]> neighboring_particles = new List<Particle[]>();


                // Initial density and neighbours generation
                for (int i = 0; i < all_particle.Length; i++)
                {
                    neighboring_particles.Add(spatialHash.getHashNeighbours(all_particle[i]));
                    // Pregenerate density
                    all_particle[i].pressure_multiplier = pressure_multiplier;
                    all_particle[i].rest_density = rest_density;

                    System.Diagnostics.Debug.WriteLine(pressure_multiplier);
                    all_particle[i].density = all_particle[i].CalculateDensity(neighboring_particles[i], all_particle[i]);
                }
                Vector2 gravityVector = new Vector2(0, this.gravity);
                for (int i = 0; i < all_particle.Length; i++)
                {
                    // TO DO: Add viscosity force when pressure works fine
                    all_particle[i].pressureVector = all_particle[i].CalculatePressureForce(neighboring_particles[i], all_particle[i]);

                    Vector2 viscosity_vector = all_particle[i].CalculateViscosityForce(neighboring_particles[i], all_particle[i]);


                    // For now we just use pressure as an indicator. We add in viscosity and gravity later
                    all_particle[i].vectors = all_particle[i].pressureVector + viscosity_vector + gravityVector;

                }
            }


            // Mouse stuff with the mouse click
            private void Form1_MouseDown(object sender, MouseEventArgs e)
            {
                isMouseDown = true;
                lastMouseEvent = e;
                holdTimer.Start();
            }


            private void Form1_MouseMove(object sender, MouseEventArgs e)
            {
                if (isMouseDown)
                {
                    lastMouseEvent = e;
                }
            }

            private void Form1_MouseUp(object sender, MouseEventArgs e)
            {
                isMouseDown = false;
                holdTimer.Stop();
            }

            private void HoldTimer_Tick(object sender, EventArgs e)
            {
                
                if (isMouseDown && lastMouseEvent != null)
                {
                    bool left;
                    float moveForce;
                    if (lastMouseEvent.Button == MouseButtons.Left)
                    {
                        left = true;
                        moveForce = 5f;
                    } else
                    {
                        left = false;
                        moveForce = 5f;
                    }

                    System.Diagnostics.Debug.WriteLine(lastMouseEvent.Button);
                    float pushRadius = 300f;
                    Vector2 mouseLocation = new Vector2(lastMouseEvent.Location.X, lastMouseEvent.Location.Y);

                    Particle newParticle = new Particle();
                    newParticle.smoothing_radius = 100;
                    newParticle.visual_radius = 5;

                    newParticle.position = new Vector2();
                    newParticle.position = mouseLocation;
                    newParticle.mass = 1;

                    Particle[] close_particles = spatialHash.getHashNeighbours(newParticle);

                    moveParticles(close_particles, newParticle, moveForce, pushRadius, left);
                }
            }

            public void moveParticles(Particle[] neighbouringParticles, Particle mainParticle, float moveForce, float radius, bool leftclick)
            {
                foreach(Particle particle in neighbouringParticles)
                {

                    Vector2 direction = mainParticle.position - particle.position;
                    double distance = (double)particle.EuclideanDistance(particle, mainParticle);
                    float coeff = customKernelFunction((float)distance, radius);

                    if (leftclick == true)
                    {
                        particle.velocity -= direction * coeff * moveForce;
                    } else
                    {
                        particle.velocity += direction * coeff * moveForce;
                    }
                        //System.Diagnostics.Debug.WriteLine(direction * coeff * moveForce);
                        //particle.velocity -= direction * coeff * moveForce;
                }
            }

            private float customKernelFunction(float distance, float smoothing_radius)
            {



                double normalise_constant = 12 / (Math.Pow(smoothing_radius, 4) * float.Pi);

                // Get the normalised Distance
                double q = distance / smoothing_radius;

                // Function cuts off at q > 1
                if (q <= 1)
                {
                    return (float)(3 * q - (9 / 4) * Math.Pow(q, 2));
                }
                else if (q <= 2)
                {
                    return (float)((3 / 4) * Math.Pow((2-q), 2));
                } else
                {
                    // Output coefficient
                    return 0;
                }
            }


    }
    }
}