using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace SimulacionDeTraficoVehicularAPP.Models
{
    public class Semaforo : ISemaforo
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly object _estadoLock = new object();
        private EstadoSemaforo _estado;
        private CancellationTokenSource _cts;

        // Tiempos configurables en milisegundos
        private readonly int _tiempoVerde;
        private readonly int _tiempoAmarillo;
        private readonly int _tiempoRojo;

        public int Id { get; }

        public EstadoSemaforo Estado
        {
            get { lock (_estadoLock) { return _estado; } }
        }

        public Semaforo(int id, int tiempoVerde = 3000, int tiempoAmarillo = 1000, int tiempoRojo = 3000)
        {
            Id = id;
            _tiempoVerde = tiempoVerde;
            _tiempoAmarillo = tiempoAmarillo;
            _tiempoRojo = tiempoRojo;

            // Inicia en verde, permite múltiples hilos pasar simultáneamente
            _estado = EstadoSemaforo.Verde;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _cts = new CancellationTokenSource();

            IniciarCiclo();
        }

        public void CambiarEstado(EstadoSemaforo nuevoEstado)
        {
            lock (_estadoLock)
            {
                _estado = nuevoEstado;
            }
        }

        public bool PuedeAvanzar()
        {
            lock (_estadoLock)
            {
                return _estado == EstadoSemaforo.Verde;
            }
        }

        // Bloquea el hilo si el semáforo está en rojo, y lo libera cuando cambia a verde
        public async Task EsperarSiEsNecesarioAsync(CancellationToken token = default)
        {
            while (true)
            {
                lock (_estadoLock)
                {
                    if (_estado == EstadoSemaforo.Verde)
                        return; // puede avanzar
                }
                // Espera un momento y vuelve a revisar (polling liviano)
                await Task.Delay(100, token);
            }
        }

        public void Detener()
        {
            _cts.Cancel();
        }

        private void IniciarCiclo()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // --- VERDE ---
                    CambiarEstado(EstadoSemaforo.Verde);
                    await Task.Delay(_tiempoVerde, _cts.Token).ContinueWith(_ => { });

                    if (_cts.Token.IsCancellationRequested) break;

                    // --- AMARILLO ---
                    CambiarEstado(EstadoSemaforo.Amarillo);
                    await Task.Delay(_tiempoAmarillo, _cts.Token).ContinueWith(_ => { });

                    if (_cts.Token.IsCancellationRequested) break;

                    // --- ROJO ---
                    CambiarEstado(EstadoSemaforo.Rojo);
                    await Task.Delay(_tiempoRojo, _cts.Token).ContinueWith(_ => { });
                }
            }, _cts.Token);
        }
    }
}
