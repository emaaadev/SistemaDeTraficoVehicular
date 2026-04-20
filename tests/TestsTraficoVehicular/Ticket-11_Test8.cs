using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulacionDeTraficoVehicularAPP.Models;
using Xunit;

namespace TestsTraficoVehicular
{
    public class SimulacionVehiculoTests
    {
        [Fact]
        public void Vehiculo_TieneVelocidadSegunTipo()
        {
            var moto = new Vehiculo(1, "Moto");
            var auto = new Vehiculo(2, "Auto");
            var bus = new Vehiculo(3, "Bus");

            Assert.True(moto.VelocidadActual >= 1);
            Assert.True(auto.VelocidadActual >= 1);
            Assert.True(bus.VelocidadActual >= 1);
        }

        [Fact]
        public void Vehiculo_CambiaPosicion_CuandoSeMueve()
        {
            var vehiculo = new Vehiculo(4, "Auto");
            var posicionInicial = vehiculo.Posicion;

            vehiculo.Mover("Prueba");

            Assert.NotEqual(posicionInicial, vehiculo.Posicion);
        }

        [Fact]
        public void Vehiculo_NoCambiaPosicion_CuandoSeDetiene()
        {
            var vehiculo = new Vehiculo(5, "Bus");
            var posicionInicial = vehiculo.Posicion;

            vehiculo.Detener("Prueba");

            Assert.Equal(posicionInicial, vehiculo.Posicion);
        }
    }
}