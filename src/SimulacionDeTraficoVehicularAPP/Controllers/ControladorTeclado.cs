using SimulacionDeTraficoVehicularAPP.Models;

namespace SimulacionDeTraficoVehicularAPP.Controllers
{
    public class ControladorTeclado
    {
        private readonly CancellationTokenSource _cts;
        private readonly List<Vehiculo> _listaVehiculos;
        private int _nextId;
        private string _idBuffer = "";

        private readonly string _destinoActual;
        private readonly int _metaActual;

        public ControladorTeclado(CancellationTokenSource cts, List<Vehiculo> listaVehiculos, int nextId,string destino, int meta)
        {
            _cts = cts;
            _listaVehiculos = listaVehiculos;
            _nextId = nextId;
            _destinoActual = destino;
            _metaActual = meta;
        }
        public Task IniciarEscuchaAsync()
        {
            return Task.Run(() =>
            {
                MostrarMenu();
                bool esperandoId = false;

                while (!_cts.Token.IsCancellationRequested)
                {
                    if (!Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var keyInfo = Console.ReadKey(intercept: true);

                    if (esperandoId)
                    {
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine();
                            if (int.TryParse(_idBuffer, out int id))
                                EjecutarAccidente(id);
                            _idBuffer = "";
                            esperandoId = false;
                        }
                        else if (char.IsDigit(keyInfo.KeyChar))
                        {
                            _idBuffer += keyInfo.KeyChar;
                            Console.Write(keyInfo.KeyChar);
                        }
                        continue;
                    }

                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.A:
                            AgregarVehiculo();
                            break;
                        case ConsoleKey.F:
                            esperandoId = true;
                            _idBuffer = "";
                            Console.Write("\n[Teclado] ID del vehículo (Enter para confirmar): ");
                            break;
                        case ConsoleKey.Q:
                            Terminar();
                            return;
                        case ConsoleKey.B:
                            AgregarVehiculoEnRuta("Sur");
                            break;
                        case ConsoleKey.P:
                            AgregarVehiculoEnRuta("Norte");
                            break;
                        case ConsoleKey.V:
                            AgregarVehiculoEnRuta("Centro");
                            break;
                    }
                }
            });
        }

        private void AgregarVehiculo()
        {
            var tipos = new[] { "Auto", "Bus", "Moto" };
            int id = Interlocked.Increment(ref _nextId);
            string tipo = tipos[id % 3];
            var nuevo = new Vehiculo(id, tipo, "Norte", _destinoActual, _metaActual);
            lock (_listaVehiculos)
            {
                _listaVehiculos.Add(nuevo);
            }
            Console.WriteLine($"\n[Teclado] Vehículo {id} ({tipo}) agregado a la ruta norte.");
        }

        private void EjecutarAccidente(int id)
        {
            Vehiculo? vehiculo;
            lock (_listaVehiculos)
            {
                vehiculo = _listaVehiculos.FirstOrDefault(v => v.Id == id);
            }

            if (vehiculo != null)
            {
                vehiculo.ForzarAccidente();
                Console.WriteLine($"[Teclado] Accidente forzado en Vehículo {id}.");
            }
            else
            {
                Console.WriteLine($"[Teclado] Vehículo {id} no encontrado.");
            }
        }

        private void Terminar()
        {
            Console.WriteLine("\n[Teclado] Cancelando simulación...");
            _cts.Cancel();
        }

        private void AgregarVehiculoEnRuta(string ruta)
        {
            var tipos = new[] { "Auto", "Bus", "Moto", "Camion" };
            int id = Interlocked.Increment(ref _nextId);
            string tipo = tipos[new Random().Next(tipos.Length)];
            var nuevo = new Vehiculo(id, tipo, ruta, _destinoActual, _metaActual);
            lock (_listaVehiculos) { _listaVehiculos.Add(nuevo); }
            Console.WriteLine($"\n[Teclado] Vehículo {id} ({tipo}) agregado en Ruta {ruta}.");
        }

        private void MostrarMenu()
        {
            Console.WriteLine("\n--- Control de Simulación ---");
            Console.WriteLine("  B = Agregar vehículo en Ruta Sur");
            Console.WriteLine("  P = Agregar vehículo en Ruta Norte");
            Console.WriteLine("  V = Agregar vehículo en Ruta Centro");
            Console.WriteLine("  F = Forzar accidente");
            Console.WriteLine("  Q = Terminar simulación");
            Console.WriteLine("-----------------------------\n");
        }
    }
}
