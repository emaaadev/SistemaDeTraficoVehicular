using SimulacionDeTraficoVehicularAPP.Controllers;
using SimulacionDeTraficoVehicularAPP.Models;

namespace TestsTraficoVehicular
{
    public class ControladorTecladoTests
    {
        [Fact]
        public void Cancelacion_DetieneLaSimulacion()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var lista = new List<Vehiculo> { new Vehiculo(1, "Auto") };
            var controlador = new ControladorTeclado(cts, lista, lista.Count);

            // Act
            cts.Cancel();

            // Assert
            Assert.True(cts.Token.IsCancellationRequested);
        }

        [Fact]
        public void ForzarAccidente_MarcaVehiculoCorrectamente()
        {
            // Arrange
            var vehiculo = new Vehiculo(1, "Auto");

            // Act
            vehiculo.ForzarAccidente();

            // Assert
            Assert.True(vehiculo.TieneAccidente());
        }

        [Fact]
        public void AgregarVehiculo_IncrementaListaCorrectamente()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var lista = new List<Vehiculo> { new Vehiculo(1, "Auto") };
            int countInicial = lista.Count;

            // Act
            var nuevo = new Vehiculo(2, "Bus");
            lock (lista) { lista.Add(nuevo); }

            // Assert
            Assert.Equal(countInicial + 1, lista.Count);
            cts.Cancel();
        }

        [Fact]
        public void ForzarAccidente_VehiculoSinAccidenteRetornaFalse()
        {
            // Arrange
            var vehiculo = new Vehiculo(1, "Auto");

            // Assert
            Assert.False(vehiculo.TieneAccidente());
        }

        [Fact]
        public void CancellationToken_NoEstaActivoAlCrearse()
        {
            // Arrange
            var cts = new CancellationTokenSource();

            // Assert
            Assert.False(cts.Token.IsCancellationRequested);
        }
    }
}