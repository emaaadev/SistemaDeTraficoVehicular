using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace SimulacionDeTraficoVehicularAPP.Models
{
    public class Interseccion : IInterseccion
    {
        private readonly object _lock = new object();
        private readonly HashSet<int> _vehiculosEnInterior = new HashSet<int>();

        public int Id { get; }
        public (int X, int Y) Coordenadas { get; }
        public ISemaforo Semaforo { get; }
        public Calle CalleEntrada { get; }
        public Calle CalleSalida { get; }
        public string Nombre { get; }

        public Interseccion(int id, (int X, int Y) coordenadas, ISemaforo semaforo, Calle calleEntrada, Calle calleSalida, string nombre)
        {
            Id = id;
            Nombre = nombre;
            Coordenadas = coordenadas;
            Semaforo = semaforo;
            CalleEntrada = calleEntrada;
            CalleSalida = calleSalida;
        }

        public bool HayColision(IVehiculo vehiculo)
        {
            lock (_lock)
            {
                return _vehiculosEnInterior.Contains(vehiculo.Id);
            }
        }

        public void RegistrarVehiculo(IVehiculo vehiculo)
        {
            lock (_lock)
            {
                if (_vehiculosEnInterior.Contains(vehiculo.Id))
                {
                    Console.WriteLine($"[Intersección {Id}] COLISIÓN detectada — Vehículo {vehiculo.Id} ya estaba registrado.");
                    return;
                }

                _vehiculosEnInterior.Add(vehiculo.Id);
               Console.WriteLine($"[{Nombre}] Vehículo {vehiculo.Id} ({vehiculo.Tipo}) entró a la intersección. Vehículos dentro: {_vehiculosEnInterior.Count}");
            }
        }

        public void LiberarVehiculo(IVehiculo vehiculo)
        {
            lock (_lock)
            {
                _vehiculosEnInterior.Remove(vehiculo.Id);
                Console.WriteLine($"[{Nombre}] Vehículo {vehiculo.Id} ({vehiculo.Tipo}) salió de la intersección. Vehículos dentro: {_vehiculosEnInterior.Count}");
            }
        }

        public int VehiculosDentro()
        {
            lock (_lock) { return _vehiculosEnInterior.Count; }
        }
    }
}