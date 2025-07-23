using System.CodeDom.Compiler;
using System.Numerics;
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
                all_particles[i].smoothing_radius = 50;
                all_particles[i].visual_radius = 5;

                
                int random_x = random.Next(0, width);
                int random_y = random.Next(0, height);

                all_particles[i].position = new Vector2();
                all_particles[i].position.X = random_x;
                all_particles[i].position.Y = random_y;



               
            }

            return all_particles;

        }

        public void MoveTick(Particle movedParticle, float tickRate, int width, int height)
        {
            // Collision dampening coefficient, TUNEABLE
            float collision_dampening = 0.9f;

            // Update velocity vectors of each particle
            movedParticle.velocity += movedParticle.vectors * tickRate;
            movedParticle.velocity *= 0.98f;
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

            public shapeWindow()
            {
                int particle_nums = 1000;
                int width = 500;
                int height = 500;

                this.Text = "Fluid Simulation";
                this.Size = new Size(width, height);
                this.BackColor = Color.Black;

                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.UpdateStyles();

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
                    g.FillEllipse(Brushes.Red, x, y, size, size);
                }
            }

            private void ParticleTick(Particle[] all_particle, int height, int width)
            {
                // Add all points to the hash map
                SpatialHash spatialHash = new SpatialHash();
                float smoothing_radius = all_particle[0].smoothing_radius;
                spatialHash.SpatialHashGrid((int)smoothing_radius, all_particle, height, width);

                // Calculate the neighbours first
                List<Particle[]> neighboring_particles = new List<Particle[]>();


                // Initial density and neighbours generation
                for (int i = 0; i < all_particle.Length; i++)
                {
                    all_particle[i].vectors = Vector2.Zero;
                    neighboring_particles.Add(spatialHash.getHashNeighbours(all_particle[i]));
                    // Pregenerate density
                    all_particle[i].density = all_particle[i].CalculateDensity(neighboring_particles[i], all_particle[i]);
                }
                Vector2 gravityVector = new Vector2(0, 0);
                for (int i = 0; i < all_particle.Length; i++)
                {
                    // TO DO: Add viscosity force when pressure works fine
                    all_particle[i].pressureVector = all_particle[i].CalculatePressureForce(neighboring_particles[i], all_particle[i]);

                    
                    // For now we just use pressure as an indicator. We add in viscosity and gravity later
                    all_particle[i].vectors = all_particle[i].pressureVector + gravityVector;

                }
            }

        }
    }
}