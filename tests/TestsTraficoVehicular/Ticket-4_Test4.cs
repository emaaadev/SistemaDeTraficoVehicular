using SimulacionDeTraficoVehicularAPP.Models;
using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace TestsTraficoVehicular
{
    public class CalleInterseccionTests
    {
        [Fact]
        public void Calle_PermiteEntrarHastaCapacidadMaxima()
        {
            var calle = new Calle(1, "Test", capacidadMaxima: 2);
            var v1 = new Vehiculo(1, "Auto");
            var v2 = new Vehiculo(2, "Bus");

            Assert.True(calle.Entrar(v1));
            Assert.True(calle.Entrar(v2));
        }

        [Fact]
        public void Calle_RechazaVehiculoCuandoEstaLlena()
        {
            var calle = new Calle(1, "Test", capacidadMaxima: 1);
            var v1 = new Vehiculo(1, "Auto");
            var v2 = new Vehiculo(2, "Moto");

            calle.Entrar(v1);
            Assert.False(calle.Entrar(v2));
        }

        [Fact]
        public void Calle_ReduceConteoAlSalir()
        {
            var calle = new Calle(1, "Test", capacidadMaxima: 2);
            var v1 = new Vehiculo(1, "Auto");

            calle.Entrar(v1);
            calle.Salir(v1);

            Assert.Equal(0, calle.VehiculosActuales());
        }

        [Fact]
        public void Interseccion_RegistraVehiculoCorrectamente()
        {
            var semaforo = new Semaforo(1);
            var c1 = new Calle(1, "Entrada", 5);
            var c2 = new Calle(2, "Salida", 5);
            var interseccion = new Interseccion(1, (0, 0), semaforo, c1, c2);
            var vehiculo = new Vehiculo(1, "Auto");

            interseccion.RegistrarVehiculo(vehiculo);

            Assert.Equal(1, interseccion.VehiculosDentro());
            semaforo.Detener();
        }

        [Fact]
        public void Interseccion_NoDuplicaVehiculoYaRegistrado()
        {
            var semaforo = new Semaforo(1);
            var c1 = new Calle(1, "Entrada", 5);
            var c2 = new Calle(2, "Salida", 5);
            var interseccion = new Interseccion(1, (0, 0), semaforo, c1, c2);
            var vehiculo = new Vehiculo(1, "Auto");

            interseccion.RegistrarVehiculo(vehiculo);
            interseccion.RegistrarVehiculo(vehiculo); // intento duplicado

            Assert.Equal(1, interseccion.VehiculosDentro());
            semaforo.Detener();
        }

        [Fact]
        public void Interseccion_LiberaVehiculoCorrectamente()
        {
            var semaforo = new Semaforo(1);
            var c1 = new Calle(1, "Entrada", 5);
            var c2 = new Calle(2, "Salida", 5);
            var interseccion = new Interseccion(1, (0, 0), semaforo, c1, c2);
            var vehiculo = new Vehiculo(1, "Auto");

            interseccion.RegistrarVehiculo(vehiculo);
            interseccion.LiberarVehiculo(vehiculo);

            Assert.Equal(0, interseccion.VehiculosDentro());
            semaforo.Detener();
        }
    }
}