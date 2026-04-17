using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace SimulacionDeTraficoVehicularAPP.Models
{
    public class DetectorColisiones
    {
        private readonly object _posicionesLock = new object();
        private readonly Dictionary<(int X, int Y), IVehiculo> _posiciones = new();
        private static int _contadorColisiones = 0;

        public static int TotalColisiones => _contadorColisiones;

        // Retorna el vehiculo con el que colisiono, o null si no hay colision
        public IVehiculo? RegistrarPosicion(IVehiculo vehiculo)
        {
            lock (_posicionesLock)
            {
                var pos = vehiculo.Posicion;

                if (_posiciones.TryGetValue(pos, out IVehiculo? otro) && otro.Id != vehiculo.Id)
                {
                    // Colision detectada
                    Interlocked.Increment(ref _contadorColisiones);
                    _posiciones.Remove(pos);
                    return otro;
                }

                _posiciones[pos] = vehiculo;
                return null;
            }
        }

        public void LiberarPosicion(IVehiculo vehiculo)
        {
            lock (_posicionesLock)
            {
                var pos = vehiculo.Posicion;
                if (_posiciones.TryGetValue(pos, out IVehiculo? actual) && actual.Id == vehiculo.Id)
                    _posiciones.Remove(pos);
            }
        }

        public static void ResetContador()
        {
            Interlocked.Exchange(ref _contadorColisiones, 0);
        }
    }
}