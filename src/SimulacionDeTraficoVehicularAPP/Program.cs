using SimulacionDeTraficoVehicularAPP.Interfaces;
using SimulacionDeTraficoVehicularAPP.Models;

namespace SimulacionDeTraficoVehicularAPP
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(" Simulador de Tráfico Vehicular \n");

            int maxProcesadores = SolicitarProcesadores();

            var opciones = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxProcesadores
            };

            Console.WriteLine($"\nConfiguración lista: {maxProcesadores} procesadores asignados.");
            Console.WriteLine("Interfaces definidas: IVehiculo, ISemaforo, IInterseccion, ISimulacion.");
            Console.WriteLine("Proyecto listo para la siguiente tarea.");

            // Semáforo compartido para todos los vehículos
            var semaforo = new Semaforo(id: 1, tiempoVerde: 3000, tiempoAmarillo: 1000, tiempoRojo: 3000);

            var listaVehiculos = new List<Vehiculo>
            {
                new Vehiculo(1, "Auto"),
                new Vehiculo(2, "Bus"),
                new Vehiculo(3, "Moto"),
                new Vehiculo(4, "Auto"),
                new Vehiculo(5, "Bus")
            };

            Console.WriteLine("\nIniciando simulación...\n");

            Parallel.ForEach(listaVehiculos, opciones, vehiculo =>
            {
                vehiculo.Simular(semaforo); // Le pasamos el semaforo compartido
            });

            semaforo.Detener(); // detiene el ciclo del semaforo al finalizar

            Console.WriteLine("\nSimulación finalizada."); // finaliza ejecucion
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