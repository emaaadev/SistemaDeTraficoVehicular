using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulacionDeTraficoVehicularAPP.Models;

namespace TestsTraficoVehicular
{
    public class VehiculoTests
    {
        [Fact]
        public void Vehiculo_SeMueve_CuandoViaEstaLibre()
        {
            var vehiculo = new Vehiculo(1, "Auto");
            var posicionInicial = vehiculo.Posicion;

            vehiculo.Mover("Via libre");

            Assert.NotEqual(posicionInicial, vehiculo.Posicion);
        }

        [Fact]
        public void Vehiculo_NoSeMueve_CuandoSeDetiene()
        {
            var vehiculo = new Vehiculo(2, "Bus");
            var posicionInicial = vehiculo.Posicion;

            vehiculo.Detener("Semaforo en rojo");

            Assert.Equal(posicionInicial, vehiculo.Posicion);
        }
    }
}