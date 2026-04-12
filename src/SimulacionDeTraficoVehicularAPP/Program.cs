using SimulacionDeTraficoVehicularAPP.Interfaces;

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