using SimulacionDeTraficoVehicularAPP.Interfaces;
using SimulacionDeTraficoVehicularAPP.Models;
using Xunit;

namespace SimulacionDeTraficoVehicularAPP.Tests
{
    public class DetectorColisionesTests
    {
        // ─────────────────────────────────────────
        // 1. Sin colisión cuando posiciones son distintas
        // ─────────────────────────────────────────
        [Fact]
        public void RegistrarPosicion_NoDebeDetectarColision_CuandoPosicionesSonDistintas()
        {
            DetectorColisiones.ResetContador();
            var detector = new DetectorColisiones();
            var vehiculo1 = new Vehiculo(1, "Auto");
            var vehiculo2 = new Vehiculo(2, "Bus");

            vehiculo1.Posicion = (0, 0);
            vehiculo2.Posicion = (5, 5);

            var resultado1 = detector.RegistrarPosicion(vehiculo1);
            var resultado2 = detector.RegistrarPosicion(vehiculo2);

            Assert.Null(resultado1);
            Assert.Null(resultado2);
            Assert.Equal(0, DetectorColisiones.TotalColisiones);
        }

        // ─────────────────────────────────────────
        // 2. Colisión cuando dos vehículos coinciden en posición
        // ─────────────────────────────────────────
        [Fact]
        public void RegistrarPosicion_DebeDetectarColision_CuandoDosVehiculosCoinciden()
        {
            DetectorColisiones.ResetContador();
            var detector = new DetectorColisiones();
            var vehiculo1 = new Vehiculo(1, "Auto");
            var vehiculo2 = new Vehiculo(2, "Moto");

            vehiculo1.Posicion = (3, 3);
            vehiculo2.Posicion = (3, 3);

            detector.RegistrarPosicion(vehiculo1);
            var colision = detector.RegistrarPosicion(vehiculo2);

            Assert.NotNull(colision);
            Assert.Equal(vehiculo1.Id, colision.Id);
            Assert.Equal(1, DetectorColisiones.TotalColisiones);
        }

        // ─────────────────────────────────────────
        // 3. Contador es correcto bajo carga paralela
        // ─────────────────────────────────────────
        [Fact]
        public void ContadorColisiones_DebeSerExacto_BajoCargaParalela()
        {
            DetectorColisiones.ResetContador();
            var detector = new DetectorColisiones();

            // 50 pares de vehículos que van a la misma posición = 50 colisiones
            var tareas = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
            {
                var v1 = new Vehiculo(i * 2, "Auto");
                var v2 = new Vehiculo(i * 2 + 1, "Bus");

                v1.Posicion = (i, 0);
                v2.Posicion = (i, 0);

                detector.RegistrarPosicion(v1);
                detector.RegistrarPosicion(v2);
            })).ToArray();

            Task.WaitAll(tareas);

            Assert.Equal(50, DetectorColisiones.TotalColisiones);
        }

        // ─────────────────────────────────────────
        // 4. Liberar posición permite que otro vehículo ocupe ese lugar sin colisión
        // ─────────────────────────────────────────
        [Fact]
        public void LiberarPosicion_DebePermitirQueOtroVehiculoOcupeElLugar()
        {
            DetectorColisiones.ResetContador();
            var detector = new DetectorColisiones();
            var vehiculo1 = new Vehiculo(1, "Auto");
            var vehiculo2 = new Vehiculo(2, "Bus");

            vehiculo1.Posicion = (4, 4);
            vehiculo2.Posicion = (4, 4);

            detector.RegistrarPosicion(vehiculo1);
            detector.LiberarPosicion(vehiculo1); // libera antes de que llegue el segundo

            var colision = detector.RegistrarPosicion(vehiculo2);

            Assert.Null(colision);
            Assert.Equal(0, DetectorColisiones.TotalColisiones);
        }

        // ─────────────────────────────────────────
        // 5. Acceso concurrente no genera excepciones
        // ─────────────────────────────────────────
        [Fact]
        public void DetectorColisiones_AccesoConcurrente_NoDebeGenerarExcepciones()
        {
            DetectorColisiones.ResetContador();
            var detector = new DetectorColisiones();
            var excepciones = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            var tareas = Enumerable.Range(0, 30).Select(i => Task.Run(() =>
            {
                try
                {
                    var vehiculo = new Vehiculo(i, "Auto");
                    vehiculo.Posicion = (i % 5, 0); // posiciones intencionalmente repetidas
                    detector.RegistrarPosicion(vehiculo);
                    detector.LiberarPosicion(vehiculo);
                }
                catch (Exception ex)
                {
                    excepciones.Add(ex);
                }
            })).ToArray();

            Task.WaitAll(tareas);

            Assert.Empty(excepciones);
        }
    }
}