using System.Reflection.Metadata.Ecma335;

namespace FluidSimulation
{

    public class Main(string[] args)
    {
        public const float gravity = 9.8f;
        // We can add on the viscosity force later, but at the moment viscosity wont be included.
        // Next to do
        // Add a Visual interface
        // Add boundaries
        // Hash keys
        //
        
    }

    public class Particle : KernelFunctions
    {
        // Particle class that contains the essential componenets for each particles

        // Fluid Variables
        public float density;
        public float pressure;
        public float smoothing_radius;

        // Arbitrary at the moment until i decide to implement more shite
        public float mass = 1;

        // Positional Variables
        // X and Y vectors for the velocity and acceleration vectors
        public float[] velocity;
        public float[] vectors;
        public float[] position;
    }

    public class ParticleProperties : Particle
    {
        private float sharedPressureValue(Particle particleA, Particle particleB)
        {
            // Calculate the mean pressure between the two in accordance with 3rd law motion (equal and opposite reaction with eachother
            float pressureA = CalculatePressureFromDensity(particleA.density);
            float pressureB = CalculatePressureFromDensity(particleB.density);

            // Average out the two pressures
            return (pressureA + pressureB) / 2;
        }


        private float CalculateDensity(Particle[] all_particles, Particle mainParticle)
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
        private float CalculatePressureFromDensity(float density)
        {
            // Tuneable coefficients
            float rest_density = 1.5f;
            float pressureMultiplier = 1f;
            return pressureMultiplier * (rest_density - density);
        }

        // Calulcate the force as a result of pressure
        private float[] CalculatePressureForce(Particle[] all_particles, Particle mainParticle)
        {
            float[] vector = new float[2];

            for (int i = 0; i < all_particles.Length; i++)
            {
                Particle compared_particle = all_particles[i];


                // x - y direction vectors
                float[] direction = mainParticle.position.Zip(compared_particle.position, (x, y) => x - y).ToArray();

                // Unit vector for direction
                float distance = EuclideanDistance(compared_particle, mainParticle);
                direction = direction.Select(x => x/distance).ToArray();

                // Working out the pressure of the two points, Newtons third law ensure that pressures are equivalent
                mainParticle.pressure = sharedPressureValue(compared_particle, mainParticle);

                float coeff = compared_particle.mass * 2 * mainParticle.pressure / compared_particle.density;
                float kernel_output = GradKernelFunction(distance, mainParticle.smoothing_radius);
                float scaling = coeff * kernel_output;

                direction = direction.Select(x => x * scaling).ToArray();

                // Add on the vector
                vector = vector.Zip(direction, (x, y) => x + y).ToArray();
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