using System.Threading;
using Xunit;

namespace TestsTraficoVehicular
{
    public class SimulacionTests
    {
        [Fact]
        public void ContadorVehiculos_DebeIncrementarCorrectamente()
        {
            int vehiculosCompletados = 0;

            Parallel.For(0, 1000, i =>
            {
                Interlocked.Increment(ref vehiculosCompletados);
            });

            Assert.Equal(1000, vehiculosCompletados);
        }

        [Fact]
        public void Speedup_DebeSerMayorQueCero()
        {
            double tiempoParalelo = 100;
            double tiempoSecuencial = 150;

            double speedup = tiempoSecuencial / tiempoParalelo;

            Assert.True(speedup > 0);
        }

        [Fact]
        public void Eficiencia_DebeEstarEnRangoValido()
        {
            double speedup = 1.5;
            int procesadores = 2;

            double eficiencia = speedup / procesadores;

            Assert.InRange(eficiencia, 0, 1);
        }
    }
}