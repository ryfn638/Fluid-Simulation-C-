using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics;
using System.Security.Policy;

namespace FluidSimulation
{

    public class Particle : ParticleProperties
    {
        // Particle class that contains the essential componenets for each particles

        // Fluid Variables
        public float density;
        public float pressure;
        public Vector2 pressureVector = new Vector2(0,0);
        public float smoothing_radius;
        public int visual_radius;
        public float viscosity = 1e-6f;


        public float pressure_multiplier = 200f;

        // Arbitrary at the moment until i decide to implement more shite
        public float mass;

        // Positional Variables
        // X and Y vectors for the velocity and acceleration vectors
        public Vector2 velocity;
        public Vector2 vectors;
        public Vector2 position;
    }

    public class ParticleProperties : KernelFunctions
    {
        public float rest_density = 10f;

        private float sharedPressureValue(Particle particleA, Particle particleB)
        {
            // Calculate the mean pressure between the two in accordance with 3rd law motion (equal and opposite reaction with eachother
            float pressureA = CalculatePressureFromDensity(particleA.density, particleA);
            float pressureB = CalculatePressureFromDensity(particleB.density, particleB);

            // Average out the two pressures
            return (pressureA + pressureB) / 2;
        }


        public float CalculateDensity(Particle[] all_particles, Particle mainParticle)
        {
            float total_density = 0;
            for (int i = 0; i< all_particles.Length; i++)
            {

                float distance = EuclideanDistance(all_particles[i], mainParticle);

                float influence = KernelFunction(distance, mainParticle.smoothing_radius);

                total_density += influence;
            }

            return total_density;
        }


        // Getting the pressure
        public float CalculatePressureFromDensity(float density, Particle particle)
        {
            // Tuneable coefficients
            float pressureMultiplier = particle.pressure_multiplier;
            int abiatic_const = 7;

            return (float)(pressureMultiplier * (Math.Pow(density / rest_density, abiatic_const) - 1));
        }

        // Calulcate the force as a result of pressure
        public Vector2 CalculatePressureForce(Particle[] all_particles, Particle mainParticle)
        {
            Vector2 vector = new Vector2();
            for (int i = 0; i < all_particles.Length; i++)
            {
                Particle compared_particle = all_particles[i];
                if (compared_particle == mainParticle) continue;

                // x - y direction vectors
                Vector2 direction = mainParticle.position - compared_particle.position;

                // Unit vector for direction, maybe can precalculate this as well
                float distance = EuclideanDistance(compared_particle, mainParticle);

                if (distance == 0)
                {
                    continue;
                }
                direction = direction / distance;

                // Working out the pressure of the two points, Newtons third law ensure that pressures are equivalent
                mainParticle.pressure = sharedPressureValue(compared_particle, mainParticle);

                float coeff = compared_particle.mass * 2 * mainParticle.pressure / compared_particle.density;
                float kernel_output = GradKernelFunction(distance, mainParticle.smoothing_radius);
                float scaling = coeff * kernel_output;

                direction = direction * scaling;
                // Add on the vector
                vector -= direction;



            }
            // System.Diagnostics.Debug.WriteLine(vector);
            // Return summation of all forces.
            return vector;
        }

        // Add the laplace in later.
        public Vector2 CalculateViscosityForce(Particle[] all_particles, Particle mainParticle)
        {
            Vector2 total_vector = Vector2.Zero;

            foreach (Particle particle in all_particles)
            {
                if (particle == mainParticle) continue;

                float r = EuclideanDistance(particle, mainParticle);
                if (r < 1e-5f || r > particle.smoothing_radius) continue;

                float laplace = LaplaceKernelFunction(r, particle.smoothing_radius);
                float scalar = (particle.mass / particle.density) * laplace;

                Vector2 coeff = (particle.velocity - mainParticle.velocity) * scalar;

                total_vector += coeff;
            }

            return total_vector * mainParticle.viscosity;
        }

        public float EuclideanDistance(Particle particleA, Particle particleB)
        {
            // long ass line of code but its just euclidean dist between 2 points.
            return (float)Vector2.Distance(particleA.position, particleB.position);
        }
    }
}