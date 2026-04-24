using SimulacionDeTraficoVehicularAPP.Models;

namespace SimulacionDeTraficoVehicularAPP.Controllers
{
    public class ControladorTeclado
    {
        private readonly CancellationTokenSource _cts;
        private readonly List<Vehiculo> _listaVehiculos;

        public ControladorTeclado(CancellationTokenSource cts, List<Vehiculo> listaVehiculos, int nextId, string destino, int meta)
        {
            _cts = cts;
            _listaVehiculos = listaVehiculos;
        }

        public Task IniciarEscuchaAsync()
        {
            return Task.Run(() =>
            {
                MostrarMenu();

                while (!_cts.Token.IsCancellationRequested)
                {
                    if (!Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var keyInfo = Console.ReadKey(intercept: true);

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        Terminar();
                        return;
                    }
                }
            });
        }

        private void Terminar()
        {
            Console.WriteLine("\n[Teclado] Cancelando simulación...");
            _cts.Cancel();
        }

        private void MostrarMenu()
        {
            Console.WriteLine("\n--- Control de Simulación ---");
            Console.WriteLine("  Q = Terminar simulación");
            Console.WriteLine("-----------------------------\n");
        }
    }
}