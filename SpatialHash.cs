using System;

namespace FluidSimulation
{
    public class SpatialHash
    {
        // [min_x, max_x, min_y, max_y]
        double cell_size;

        public void SpatialHashGrid(double size, Particle[] all_particles)
        {
            // Create the Spatial Hash grid here
            this.cell_size = size;

            for (int i = 0; i < all_particles.Length; i++)
            {
                Particle current_particle = all_particles[i];

            }
        }

        public int[] GetHash_Position(double x, double y)
        {
            int x_hash = (int)Math.Floor(x / this.cell_size);
            int y_hash = (int)Math.Floor(y / this.cell_size);

            // Return the hash position
            return [x_hash, y_hash];
        }

        public Particle[] getHashNeighbours(Particle mainParticle, Particle[] all_particles)
        {
            // This isnt working at the moment so we'll worry abt this a tad bit later
            return all_particles;
        }
    }
}
