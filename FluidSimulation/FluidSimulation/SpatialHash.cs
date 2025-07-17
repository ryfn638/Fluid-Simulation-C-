using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FluidSimulation
{
    public class SpatialHash
    {
        // [min_x, max_x, min_y, max_y]
        double cell_size;
        int num_y_cells;

        int grid_Height;
        int grid_Width;

        List<Particle>[] spatial_hash;
        public void SpatialHashGrid(double size, Particle[] all_particles, int height, int width)
        {

            // Create the Spatial Hash grid here, size is the size of each cell
            this.cell_size = size;

            this.grid_Width = width;
            this.grid_Height = height;

            int num_x_cells = (int)(width / this.cell_size);
            this.num_y_cells = (int)(height / this.cell_size);

            // In the flattened array the length will be num_x_cells * num_y_cells
            int total_cells = num_y_cells * this.num_y_cells;
            // Clear out the spatial hash every instance
            List<Particle>[] spatial_hash = new List<Particle>[total_cells];
          

            for (int i = 0; i< total_cells; i++)
            {
                // initialise list
                spatial_hash[i] = new List<Particle>();
            }



            for (int i = 0; i < all_particles.Length; i++)
            {
                // add all particles to the spatial hash list
                Particle current_particle = all_particles[i];

                int getCellIndex = GetHash_Position(current_particle.position);
                spatial_hash[getCellIndex].Add(current_particle);
            }

            this.spatial_hash = spatial_hash;

        }

        public int GetHash_Position(Vector2 position)
        {
            int x_hash = (int)Math.Floor(position.X / this.cell_size);
            int y_hash = (int)Math.Floor(position.Y / this.cell_size);

            int index = y_hash * this.num_y_cells + x_hash;

            // In 3D this becomes
            // index = (x_hash * num_y_cells + y_hash) * num_z_cells + z_hash

            // Return the hash position
            return index;
        }

        public Particle[] getHashNeighbours(Particle mainParticle)
        {

            // getting hash coordinates of functions
            int hash_x = (int)Math.Floor(mainParticle.position.X / this.cell_size);
            int hash_y = (int)Math.Floor(mainParticle.position.Y / this.cell_size);


            List<Particle> neighbors = new List<Particle> ();

            // Looping over 3x3 grid
            for (int y_neighbor = -1; y_neighbor <= 1 ; y_neighbor++)
            {
                for (int x_neighbor = -1; x_neighbor <= 1  ; x_neighbor++)
                {
                    if (x_neighbor < 0 || y_neighbor < 0 || x_neighbor >= this.grid_Width|| y_neighbor >= this.grid_Height)
                        continue;

                    int neighbor_x_idx = (int)Math.Floor(hash_x + x_neighbor / this.cell_size);
                    int neighbor_y_idx = (int)Math.Floor(hash_y + y_neighbor / this.cell_size);

                    int neighborIndex = neighbor_y_idx * this.grid_Width + neighbor_x_idx;

                    neighbors.AddRange(this.spatial_hash[neighborIndex]);
                }
            }

            return neighbors.ToArray();
        }
    }
}
