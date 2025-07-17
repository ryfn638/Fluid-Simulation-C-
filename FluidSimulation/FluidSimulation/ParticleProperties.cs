using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace FluidSimulation
{

    public class Particle : ParticleProperties
    {
        // Particle class that contains the essential componenets for each particles

        // Fluid Variables
        public float density;
        public float pressure;
        public Vector2 pressureVector;
        public float smoothing_radius;
        public int visual_radius;

        // Arbitrary at the moment until i decide to implement more shite
        public float mass = 1;

        // Positional Variables
        // X and Y vectors for the velocity and acceleration vectors
        public Vector2 velocity;
        public Vector2 vectors;
        public Vector2 position;
    }

    public class ParticleProperties : KernelFunctions
    {
        private float sharedPressureValue(Particle particleA, Particle particleB)
        {
            // Calculate the mean pressure between the two in accordance with 3rd law motion (equal and opposite reaction with eachother
            float pressureA = CalculatePressureFromDensity(particleA.density);
            float pressureB = CalculatePressureFromDensity(particleB.density);

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
        public float CalculatePressureFromDensity(float density)
        {
            // Tuneable coefficients
            float rest_density = 1.5f;
            float pressureMultiplier = 1f;
            return pressureMultiplier * (rest_density - density);
        }

        // Calulcate the force as a result of pressure
        public Vector2 CalculatePressureForce(Particle[] all_particles, Particle mainParticle)
        {
            Vector2 vector = new Vector2();

            for (int i = 0; i < all_particles.Length; i++)
            {
                Particle compared_particle = all_particles[i];


                // x - y direction vectors
                Vector2 direction = mainParticle.position - compared_particle.position;

                // Unit vector for direction
                float distance = EuclideanDistance(compared_particle, mainParticle);
                direction = direction / distance;

                // Working out the pressure of the two points, Newtons third law ensure that pressures are equivalent
                mainParticle.pressure = sharedPressureValue(compared_particle, mainParticle);

                float coeff = compared_particle.mass * 2 * mainParticle.pressure / compared_particle.density;
                float kernel_output = GradKernelFunction(distance, mainParticle.smoothing_radius);
                float scaling = coeff * kernel_output;

                direction = direction * scaling;
                // Add on the vector
                vector += vector + direction;
            }
            // Return summation of all forces.
            return vector;
        }

        // Add the laplace in later.

        public float EuclideanDistance(Particle particleA, Particle particleB)
        {
            // long ass line of code but its just euclidean dist between 2 points.
            return (float)Math.Sqrt(Math.Pow(particleA.position[0] - particleB.position[0], 2) + Math.Pow(particleA.position[1] - particleB.position[1], 2));
        }
    }
}