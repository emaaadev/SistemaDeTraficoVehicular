using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace TestsTraficoVehicular
{
    public class InterfacesBaseTests
    {
        [Fact]
        public void EstadoSemaforo_TieneLosTresValoresEsperados()
        {
            var valores = Enum.GetValues<EstadoSemaforo>();
            Assert.Contains(EstadoSemaforo.Verde, valores);
            Assert.Contains(EstadoSemaforo.Amarillo, valores);
            Assert.Contains(EstadoSemaforo.Rojo, valores);
            Assert.Equal(3, valores.Length);
        }

        [Fact]
        public void IVehiculo_ExisteEnElNamespaceCorrecto()
        {
            var tipo = typeof(IVehiculo);
            Assert.Equal("SimulacionDeTraficoVehicularAPP.Interfaces", tipo.Namespace);
        }

        [Fact]
        public void ISemaforo_ExisteEnElNamespaceCorrecto()
        {
            var tipo = typeof(ISemaforo);
            Assert.Equal("SimulacionDeTraficoVehicularAPP.Interfaces", tipo.Namespace);
        }

        [Fact]
        public void IInterseccion_ExisteEnElNamespaceCorrecto()
        {
            var tipo = typeof(IInterseccion);
            Assert.Equal("SimulacionDeTraficoVehicularAPP.Interfaces", tipo.Namespace);
        }

        [Fact]
        public void ISimulacion_ExisteEnElNamespaceCorrecto()
        {
            var tipo = typeof(ISimulacion);
            Assert.Equal("SimulacionDeTraficoVehicularAPP.Interfaces", tipo.Namespace);
        }
    }
}