using SimulacionDeTraficoVehicularAPP.Controllers;
using SimulacionDeTraficoVehicularAPP.Interfaces;
using SimulacionDeTraficoVehicularAPP.Models;
using System.Diagnostics;

namespace SimulacionDeTraficoVehicularAPP
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine(" Simulador de Tráfico Vehicular \n");

            int maxProcesadores = SolicitarProcesadores();

            var opciones = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxProcesadores
            };

            Console.WriteLine($"\nConfiguración lista: {maxProcesadores} procesadores asignados.");
            Console.WriteLine("Proyecto listo para la siguiente tarea.");

            // Semaforo compartido para todos los vehiculos
            var semaforo = new Semaforo(id: 1, tiempoVerde: 3000, tiempoAmarillo: 1000, tiempoRojo: 3000);
            var detector = new DetectorColisiones(); // <-- nuevo



            // Calles e intersección compartidas
            var calleEntrada = new Calle(1, "Av. Principal", capacidadMaxima: 3);
            var calleSalida = new Calle(2, "Calle 27", capacidadMaxima: 3);
            var interseccion = new Interseccion(1, (5, 5), semaforo, calleEntrada, calleSalida);

            var listaVehiculos = new List<Vehiculo>
            {
                new Vehiculo(1, "Auto"),
                new Vehiculo(2, "Bus"),
                new Vehiculo(3, "Moto"),
                new Vehiculo(4, "Auto"),
                new Vehiculo(5, "Bus")
            };

            // Control por teclado
            using var cts = new CancellationTokenSource();
            opciones.CancellationToken = cts.Token;
            var controlador = new ControladorTeclado(cts, listaVehiculos, listaVehiculos.Count);
            var tareaEscucha = controlador.IniciarEscuchaAsync();

            Console.WriteLine("\nIniciando simulación...\n");

            var stopwatch = Stopwatch.StartNew();

            var vehiculosProcesados = new HashSet<int>();
            var lockProcesados = new object();

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    List<Vehiculo> pendientes;
                    lock (listaVehiculos)
                    {
                        pendientes = listaVehiculos
                            .Where(v => { lock (lockProcesados) { return !vehiculosProcesados.Contains(v.Id); } })
                            .ToList();
                    }

                    if (pendientes.Count > 0)
                    {
                        Parallel.ForEach(pendientes, opciones, vehiculo =>
                        {
                            lock (lockProcesados) { vehiculosProcesados.Add(vehiculo.Id); }

                            bool entro = calleEntrada.Entrar(vehiculo);
                            if (!entro) return;

                            interseccion.RegistrarVehiculo(vehiculo);
                            vehiculo.Simular(semaforo, detector, cts.Token);
                            interseccion.LiberarVehiculo(vehiculo);
                            calleEntrada.Salir(vehiculo);
                        });
                    }
                    else
                    {
                        // No hay pendientes — espera un momento por si el usuario agrega uno
                        Thread.Sleep(300);

                        // Si después de esperar sigue sin pendientes, termina
                        lock (listaVehiculos)
                        {
                            if (listaVehiculos.All(v => { lock (lockProcesados) { return vehiculosProcesados.Contains(v.Id); } }))
                                break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n[Sistema] Simulación cancelada por el usuario.");
            }

            stopwatch.Stop();

            cts.Cancel(); // cancela el controlador cuando la simulación termina sola
            try { await tareaEscucha; } catch (OperationCanceledException) { }

            semaforo.Detener();
            Console.WriteLine("\nSimulación finalizada.");
            Console.WriteLine($"\n Total de colisiones registradas: {DetectorColisiones.TotalColisiones}");
            Console.WriteLine($"\nTiempo total de ejecucion: {stopwatch.ElapsedMilliseconds} ms");
        }

        private static int SolicitarProcesadores()
        {
            int limite = Environment.ProcessorCount;

            while (true)
            {
                Console.Write($"Ingrese número de procesadores a usar (1 - {limite}): ");
                string? entrada = Console.ReadLine();

                if (int.TryParse(entrada, out int valor) && valor >= 1 && valor <= limite)
                    return valor;

                Console.WriteLine($"  Valor inválido. Ingrese un número entre 1 y {limite}.");
            }
        }
    }
}