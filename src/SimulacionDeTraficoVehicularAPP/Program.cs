using SimulacionDeTraficoVehicularAPP.Controllers;
using SimulacionDeTraficoVehicularAPP.Interfaces;
using SimulacionDeTraficoVehicularAPP.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace SimulacionDeTraficoVehicularAPP
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Simulador de Tráfico Vehicular \n");

            int maxProcesadores = SolicitarProcesadores();

            var opciones = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxProcesadores
            };

            
            Console.WriteLine($"\nConfiguración lista: {maxProcesadores} procesadores asignados.");
            Console.WriteLine("Proyecto listo para la siguiente tarea.");

            // Generacion aleatoria de vehiculos por zona usando Task
            var tipos = new[] { "Auto", "Bus", "Moto", "Camion" };
            var listaVehiculos = new List<Vehiculo>();
            int idCounter = 0;

            var tareasGeneracion = new List<Task>
            {
                Task.Run(() =>
                {
                    int cant = new Random().Next(3, 10);
                    for (int i = 0; i < cant; i++)
                    {
                        int id = Interlocked.Increment(ref idCounter);
                        string tipo = tipos[new Random().Next(tipos.Length)];
                        var v = new Vehiculo(id, tipo, "Norte");
                        lock (listaVehiculos) { listaVehiculos.Add(v); }
                        Console.WriteLine($"[Norte] Vehículo {v.Id} ({v.Tipo}) generado — Task ID: {Task.CurrentId} — Hilo: {Thread.CurrentThread.ManagedThreadId}");
                    }
                }),
                Task.Run(() =>
                {
                    int cant = new Random().Next(3, 10);
                    for (int i = 0; i < cant; i++)
                    {
                        int id = Interlocked.Increment(ref idCounter);
                        string tipo = tipos[new Random().Next(tipos.Length)];
                        var v = new Vehiculo(id, tipo, "Sur");
                        lock (listaVehiculos) { listaVehiculos.Add(v); }
                        Console.WriteLine($"[Sur] Vehículo {v.Id} ({v.Tipo}) generado — Task ID: {Task.CurrentId} — Hilo: {Thread.CurrentThread.ManagedThreadId}");
                    }
                }),
                Task.Run(() =>
                {
                    int cant = new Random().Next(3, 10);
                    for (int i = 0; i < cant; i++)
                    {
                        int id = Interlocked.Increment(ref idCounter);
                        string tipo = tipos[new Random().Next(tipos.Length)];
                        var v = new Vehiculo(id, tipo, "Centro");
                        lock (listaVehiculos) { listaVehiculos.Add(v); }
                        Console.WriteLine($"[Centro] Vehículo {v.Id} ({v.Tipo}) generado — Task ID: {Task.CurrentId} — Hilo: {Thread.CurrentThread.ManagedThreadId}");
                    }
                })
            };

            await Task.WhenAll(tareasGeneracion);
            Console.WriteLine($"\nTotal vehículos generados: {listaVehiculos.Count}\n");

            // ─── VERSION SECUENCIAL (baseline) ───

            Console.WriteLine("\n[ Ejecutando version secuencial como baseline... ]\n");
            var semaforoSec = new Semaforo(id: 4, tiempoVerde: 3000, tiempoAmarillo: 1000, tiempoRojo: 3000);
            var detectorSec = new DetectorColisiones();


            var vehiculosSec = listaVehiculos
                .Select(v => new Vehiculo(v.Id, v.Tipo, v.Zona))
                .ToList();

            // agregar control por teclado al baseline secuencial
            using var ctsSecuencial = new CancellationTokenSource();
            var controladorSec = new ControladorTeclado(ctsSecuencial, vehiculosSec, vehiculosSec.Count);
            var tareaEscuchaSec = controladorSec.IniciarEscuchaAsync();

            var vehiculosSecProcesados = new HashSet<int>();
            var swSec = Stopwatch.StartNew();


            await Task.Run(() =>
            {
                while (!ctsSecuencial.Token.IsCancellationRequested)
                {
                    List<Vehiculo> pendientesSec;
                    lock (vehiculosSec)
                    {
                        pendientesSec = vehiculosSec
                            .Where(v => !vehiculosSecProcesados.Contains(v.Id))
                            .ToList();
                    }

                    if (pendientesSec.Count > 0)
                    {
                        foreach (var v in pendientesSec)
                        {
                            if (ctsSecuencial.Token.IsCancellationRequested) break;
                            vehiculosSecProcesados.Add(v.Id);
                            v.Simular(semaforoSec, detectorSec, ctsSecuencial.Token);
                        }
                    }
                    else
                    {
                        Thread.Sleep(300);
                        lock (vehiculosSec)
                        {
                            if (vehiculosSec.All(v => vehiculosSecProcesados.Contains(v.Id)))
                                break;
                        }
                    }
                }
            });
            swSec.Stop();
            semaforoSec.Detener();
            double tiempoSecuencial = swSec.Elapsed.TotalSeconds;
            Console.WriteLine($"\n Tiempo secuencial (version secuencial): {tiempoSecuencial:F2} ms\n");

            Console.WriteLine("\n[Version secuencial terminada.]\n");

            // ─── VERSION PARALELA (baseline) ───
            // Semaforo compartido para todos los vehiculos
            var detector = new DetectorColisiones();

            // 3 zonas con su propio semaforo, calle e intersección
            var semaforoNorte = new Semaforo(id: 1, tiempoVerde: 3000, tiempoAmarillo: 1000, tiempoRojo: 3000);
            var semaforoSur = new Semaforo(id: 2, tiempoVerde: 2000, tiempoAmarillo: 1000, tiempoRojo: 4000);
            var semaforoCentro = new Semaforo(id: 3, tiempoVerde: 4000, tiempoAmarillo: 1000, tiempoRojo: 2000);

            var calleNorte = new Calle(1, "Zona Norte", capacidadMaxima: 3);
            var calleSur = new Calle(2, "Zona Sur", capacidadMaxima: 3);
            var calleCentro = new Calle(3, "Zona Centro", capacidadMaxima: 3);

            var interseccionNorte = new Interseccion(1, (5, 5), semaforoNorte, calleNorte, calleNorte);
            var interseccionSur = new Interseccion(2, (10, 10), semaforoSur, calleSur, calleSur);
            var interseccionCentro = new Interseccion(3, (15, 15), semaforoCentro, calleCentro, calleCentro);

            var zonaInfraestructura = new Dictionary<string, (Semaforo semaforo, Calle calle, Interseccion interseccion)>
        {
            { "Norte",  (semaforoNorte,  calleNorte,  interseccionNorte)  },
            { "Sur",    (semaforoSur,    calleSur,    interseccionSur)    },
            { "Centro", (semaforoCentro, calleCentro, interseccionCentro) }
        };

            // Contador de vehiculos completados
            int vehiculosCompletados = 0;

            // Medicion de CPU
            var proceso = Process.GetCurrentProcess();
            var cpuInicio = proceso.TotalProcessorTime;

            // Control por teclado
            using var cts = new CancellationTokenSource();
            opciones.CancellationToken = cts.Token;
            var controlador = new ControladorTeclado(cts, listaVehiculos, listaVehiculos.Count);
            var tareaEscucha = controlador.IniciarEscuchaAsync();

            Console.WriteLine("\n[ Ejecutando version paralela como baseline... ]\n");
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

                            var (semaforo, calle, interseccion) = zonaInfraestructura[vehiculo.Zona];

                            bool entro = calle.Entrar(vehiculo);
                            if (!entro) return;

                            interseccion.RegistrarVehiculo(vehiculo);
                            vehiculo.Simular(semaforo, detector, cts.Token);
                            interseccion.LiberarVehiculo(vehiculo);
                            calle.Salir(vehiculo);

                            Interlocked.Increment(ref vehiculosCompletados);
                        });
                    }
                    else
                    {
                        // No hay pendientes — espera un momento por si el usuario agrega uno
                        Thread.Sleep(300);

                        // Si despues  de esperar sigue sin pendientes, termina
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

            // Calculo CPU
            var cpuFin = proceso.TotalProcessorTime;
            var cpuUsado = cpuFin - cpuInicio;

            cts.Cancel(); // cancela el controlador cuando la simulación termina sola
            try { await tareaEscucha; } catch (OperationCanceledException) { }

            semaforoNorte.Detener();
            semaforoSur.Detener();
            semaforoCentro.Detener();

            // Calculo de speedup y eficiencia
            double tiempoParalelo = stopwatch.Elapsed.TotalSeconds;
            double speedup = tiempoSecuencial / tiempoParalelo;
            double eficiencia = speedup / maxProcesadores;

            Console.WriteLine("\n╔══════════════════════════════════════════╗");
            Console.WriteLine("║       REPORTE FINAL DE SIMULACIÓN        ║");
            Console.WriteLine("╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Vehículos completados : {vehiculosCompletados.ToString(),-16}║");
            Console.WriteLine($"║  Colisiones detectadas : {DetectorColisiones.TotalColisiones.ToString(),-16}║");
            Console.WriteLine($"║  Procesadores usados   : {maxProcesadores.ToString(),-16}║");
            Console.WriteLine("╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Tiempo secuencial     : {(tiempoSecuencial.ToString("F2") + " s"),-16}║");
            Console.WriteLine($"║  Tiempo paralelo       : {(tiempoParalelo.ToString("F2") + " s"),-16}║");
            Console.WriteLine($"║  CPU usada             : {(cpuUsado.TotalMilliseconds.ToString("F0") + " ms"),-16}║");
            Console.WriteLine("╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Speedup               : {(speedup.ToString("F2") + "x"),-16}║");
            Console.WriteLine($"║  Eficiencia            : {eficiencia.ToString("P2"),-16}║");
            Console.WriteLine("╚══════════════════════════════════════════╝");
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