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
                    Interlocked.Increment(ref _contadorColisiones);
                    _posiciones.Remove(pos);
                    EliminarVehiculo(vehiculo.Id);  // <-- eliminar ambos
                    EliminarVehiculo(otro.Id);
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

        private readonly HashSet<int> _vehiculosEliminados = new();

        public bool EstaEliminado(int vehiculoId)
        {
            lock (_posicionesLock)
            {
                return _vehiculosEliminados.Contains(vehiculoId);
            }
        }

        public void EliminarVehiculo(int vehiculoId)
        {
            lock (_posicionesLock)
            {
                _vehiculosEliminados.Add(vehiculoId);
            }
        }
    }
}