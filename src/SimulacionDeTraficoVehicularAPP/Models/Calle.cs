using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace SimulacionDeTraficoVehicularAPP.Models
{
    public class Calle
    {
        private readonly object _lock = new object();
        private int _vehiculosActuales = 0;

        public int Id { get; }
        public string Nombre { get; }
        public int CapacidadMaxima { get; }
        public bool HayCongestión => _vehiculosActuales >= CapacidadMaxima;

        public Calle(int id, string nombre, int capacidadMaxima = 5)
        {
            Id = id;
            Nombre = nombre;
            CapacidadMaxima = capacidadMaxima;
        }

        // Retorna true si el vehiculo logró entrar, false si la calle está llena
        public bool Entrar(IVehiculo vehiculo)
        {
            lock (_lock)
            {
                if (_vehiculosActuales >= CapacidadMaxima)
                {
                    Console.WriteLine($"[Calle {Nombre}] CONGESTIÓN — Vehículo {vehiculo.Id} ({vehiculo.Tipo}) no puede entrar. Capacidad: {_vehiculosActuales}/{CapacidadMaxima}");
                    return false;
                }

                _vehiculosActuales++;
                Console.WriteLine($"[Calle {Nombre}] Vehículo {vehiculo.Id} ({vehiculo.Tipo}) entró. Ocupación: {_vehiculosActuales}/{CapacidadMaxima}");
                return true;
            }
        }

        public void Salir(IVehiculo vehiculo)
        {
            lock (_lock)
            {
                if (_vehiculosActuales > 0)
                    _vehiculosActuales--;

                Console.WriteLine($"[Calle {Nombre}] Vehículo {vehiculo.Id} ({vehiculo.Tipo}) salió. Ocupación: {_vehiculosActuales}/{CapacidadMaxima}");
            }
        }

        public int VehiculosActuales()
        {
            lock (_lock) { return _vehiculosActuales; }
        }
    }
}