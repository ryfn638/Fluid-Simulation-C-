using System.Security.Policy;

namespace FluidSimulation
{
    public class KernelFunctions()
    {
        // Primary kernel function W
        public float KernelFunction(double distance, double smoothing_radius)
        {
            // Base kernel function
            double normalise_constant = 12 / (Math.Pow(smoothing_radius, 4) * float.Pi);

            // Get the normalised Distance
            double q = distance / smoothing_radius;

            // Function cuts off at q > 1
            if (q > 1)
            {
                return 0;
            }
            else
            {
                // Output coefficient
                return (float)(normalise_constant * Math.Pow(1 - q, 2));
            }
        }

        // Gradient Kernel function W Delta
        public float GradKernelFunction(float distance, float smoothing_radius)
        {
            double normalise_constant = 12 / (Math.Pow(smoothing_radius, 4) * float.Pi);
            // Normalised Distance
            double q = distance / smoothing_radius;
            

            if (q > 1)
            {
                return 0;
            }
            else
            {
                // Derivative of W
                return (float)(2 * (1 - q) * normalise_constant);
            }

        }
    }
}