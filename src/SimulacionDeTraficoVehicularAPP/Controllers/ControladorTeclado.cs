using SimulacionDeTraficoVehicularAPP.Models;

namespace SimulacionDeTraficoVehicularAPP.Controllers
{
    public class ControladorTeclado
    {
        private readonly CancellationTokenSource _cts;
        private readonly List<Vehiculo> _listaVehiculos;
        private int _nextId;
        private string _idBuffer = "";

        public ControladorTeclado(CancellationTokenSource cts, List<Vehiculo> listaVehiculos, int nextId)
        {
            _cts = cts;
            _listaVehiculos = listaVehiculos;
            _nextId = nextId;
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
                            AgregarVehiculoEnZona("Sur");
                            break;
                        case ConsoleKey.P:
                            AgregarVehiculoEnZona("Norte");
                            break;
                        case ConsoleKey.V:
                            AgregarVehiculoEnZona("Centro");
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
            var nuevo = new Vehiculo(id, tipo);
            lock (_listaVehiculos)
            {
                _listaVehiculos.Add(nuevo);
            }
            Console.WriteLine($"\n[Teclado] Vehículo {id} ({tipo}) agregado a la simulación.");
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

        private void AgregarVehiculoEnZona(string zona)
        {
            var tipos = new[] { "Auto", "Bus", "Moto", "Camion" };
            int id = Interlocked.Increment(ref _nextId);
            string tipo = tipos[new Random().Next(tipos.Length)];
            var nuevo = new Vehiculo(id, tipo, zona);
            lock (_listaVehiculos) { _listaVehiculos.Add(nuevo); }
            Console.WriteLine($"\n[Teclado] Vehículo {id} ({tipo}) agregado en Zona {zona}.");
        }

        private void MostrarMenu()
        {
            Console.WriteLine("\n--- Control de Simulación ---");
            Console.WriteLine("  A = Agregar vehículo (zona aleatoria)");
            Console.WriteLine("  B = Agregar vehículo en Zona Sur");
            Console.WriteLine("  P = Agregar vehículo en Zona Norte");
            Console.WriteLine("  V = Agregar vehículo en Zona Centro");
            Console.WriteLine("  F = Forzar accidente");
            Console.WriteLine("  Q = Terminar simulación");
            Console.WriteLine("-----------------------------\n");
        }
    }
}
