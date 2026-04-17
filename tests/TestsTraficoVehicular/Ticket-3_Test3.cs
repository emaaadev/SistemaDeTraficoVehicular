using SimulacionDeTraficoVehicularAPP.Interfaces;
using SimulacionDeTraficoVehicularAPP.Models;
using Xunit;

namespace SimulacionDeTraficoVehicularAPP.Tests
{
    public class SemaforoTests
    {
        // ─────────────────────────────────────────
        // 1. Estado inicial
        // ─────────────────────────────────────────
        [Fact]
        public void Semaforo_DebeIniciarEnVerde()
        {
            var semaforo = new Semaforo(id: 1);

            Assert.Equal(EstadoSemaforo.Verde, semaforo.Estado);
        }

        [Fact]
        public void Semaforo_PuedeAvanzar_DebeSerTrue_CuandoEstaEnVerde()
        {
            var semaforo = new Semaforo(id: 1);

            Assert.True(semaforo.PuedeAvanzar());
        }

        // ─────────────────────────────────────────
        // 2. CambiarEstado
        // ─────────────────────────────────────────
        [Fact]
        public void Semaforo_CambiarEstado_ARojo_DebeBloquearAvance()
        {
            var semaforo = new Semaforo(id: 1);

            semaforo.CambiarEstado(EstadoSemaforo.Rojo);

            Assert.Equal(EstadoSemaforo.Rojo, semaforo.Estado);
            Assert.False(semaforo.PuedeAvanzar());
        }

        [Fact]
        public void Semaforo_CambiarEstado_AAmarillo_DebeBloquearAvance()
        {
            var semaforo = new Semaforo(id: 1);

            semaforo.CambiarEstado(EstadoSemaforo.Amarillo);

            Assert.Equal(EstadoSemaforo.Amarillo, semaforo.Estado);
            Assert.False(semaforo.PuedeAvanzar());
        }

        [Fact]
        public void Semaforo_CambiarEstado_AVerde_DebePermitirAvance()
        {
            var semaforo = new Semaforo(id: 1);
            semaforo.CambiarEstado(EstadoSemaforo.Rojo);

            semaforo.CambiarEstado(EstadoSemaforo.Verde);

            Assert.True(semaforo.PuedeAvanzar());
        }

        // ─────────────────────────────────────────
        // 3. Ciclo automático (sin deadlock)
        // ─────────────────────────────────────────
        [Fact]
        public void Semaforo_CicloAutomatico_DebeCompletarSinDeadlock()
        {
            // Tiempos muy cortos para que el test sea rápido
            var semaforo = new Semaforo(id: 1, tiempoVerde: 200, tiempoAmarillo: 100, tiempoRojo: 200);

            // Espera suficiente para que pase por al menos un ciclo completo
            Thread.Sleep(700);

            // Si llegamos aquí sin colgarse, no hay deadlock
            semaforo.Detener();
            Assert.True(true);
        }

        [Fact]
        public void Semaforo_CicloAutomatico_DebePasarPorRojo()
        {
            var semaforo = new Semaforo(id: 1, tiempoVerde: 100, tiempoAmarillo: 100, tiempoRojo: 500);

            // Espera a que pase verde + amarillo
            Thread.Sleep(300);

            Assert.Equal(EstadoSemaforo.Rojo, semaforo.Estado);
            semaforo.Detener();
        }

        // ─────────────────────────────────────────
        // 4. Acceso concurrente (thread-safety)
        // ─────────────────────────────────────────
        [Fact]
        public void Semaforo_AccesoConcurrente_NoDebeGenerarExcepciones()
        {
            var semaforo = new Semaforo(id: 1, tiempoVerde: 100, tiempoAmarillo: 50, tiempoRojo: 100);
            var excepciones = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            var tareas = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < 50; i++)
                    {
                        bool puede = semaforo.PuedeAvanzar();
                        EstadoSemaforo estado = semaforo.Estado;
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    excepciones.Add(ex);
                }
            })).ToArray();

            Task.WaitAll(tareas);
            semaforo.Detener();

            Assert.Empty(excepciones);
        }

        // ─────────────────────────────────────────
        // 5. Vehículo espera en rojo
        // ─────────────────────────────────────────
        [Fact]
        public void Vehiculo_DebeEsperarCuandoSemaforoEstaEnRojo()
        {
            var semaforo = new Semaforo(id: 1, tiempoVerde: 100, tiempoAmarillo: 50, tiempoRojo: 10000);
            var vehiculo = new Vehiculo(1, "Auto");
            var detector = new DetectorColisiones();
            // Forzar rojo
            Thread.Sleep(250);
            Assert.Equal(EstadoSemaforo.Rojo, semaforo.Estado);

            bool simulacionTermino = false;

            var tarea = Task.Run(() =>
            {
                vehiculo.Simular(semaforo, detector);
                simulacionTermino = true;
            });

            // En 1 segundo no debería terminar porque el rojo dura 10 segundos
            bool terminoRapido = tarea.Wait(1000);

            semaforo.Detener();
            semaforo.CambiarEstado(EstadoSemaforo.Verde); // liberar para que termine

            Assert.False(terminoRapido, "El vehículo no debería haber terminado mientras el semáforo estaba en rojo");
        }

        // ─────────────────────────────────────────
        // 6. Detener el semáforo
        // ─────────────────────────────────────────
        [Fact]
        public void Semaforo_Detener_DebeDetenerElCiclo()
        {
            var semaforo = new Semaforo(id: 1, tiempoVerde: 200, tiempoAmarillo: 100, tiempoRojo: 200);

            semaforo.Detener();
            var estadoTrasDetener = semaforo.Estado;
            Thread.Sleep(500);

            // El estado no debe cambiar después de detener
            Assert.Equal(estadoTrasDetener, semaforo.Estado);
        }
    }
}
