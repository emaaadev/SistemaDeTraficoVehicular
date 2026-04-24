using SimulacionDeTraficoVehicularAPP.Controllers;
using SimulacionDeTraficoVehicularAPP.Interfaces;
using SimulacionDeTraficoVehicularAPP.Models;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace SimulacionDeTraficoVehicularAPP
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Simulador de Tráfico Vehicular \n");

            Console.WriteLine("\nSeleccione el destino de la exploracion:");
            Console.WriteLine("1. Puerto Central (Distancia: 25km)");
            Console.WriteLine("2. Aeropuerto     (Distancia: 15km)");
            Console.WriteLine("3. Aduana         (Distancia: 5km)");
            Console.Write("Selección (1-3): ");
            string opcion = Console.ReadLine() ?? "1";

            string destinoUsuario;
            int distanciaMeta;

            switch (opcion)
            {
                case "2":
                    destinoUsuario = "Aeropuerto";
                    distanciaMeta = 15;
                    break;
                case "3":
                    destinoUsuario = "Aduana";
                    distanciaMeta = 5;
                    break;
                default:
                    destinoUsuario = "Puerto Central";
                    distanciaMeta = 25;
                    break;
            }

            Console.WriteLine($"\nIniciando explorativa competitiva hacia: {destinoUsuario} ({distanciaMeta}km)");

            int maxProcesadores = SolicitarProcesadores();

            var opciones = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxProcesadores
            };

            
            Console.WriteLine($"\nConfiguración lista: {maxProcesadores} procesadores asignados.");
            Console.WriteLine("Proyecto listo para la siguiente tarea.");

            // Generacion aleatoria de vehiculos por ruta usando Task
            var tipos = new[] { "Auto", "Bus", "Moto", "Camion" };
            var listaVehiculos = new List<Vehiculo>();
            int idCounter = 0;

            int cantidadPorRuta = 3; // Para la muestra y analisis de metricas y cuellos de botella

            var tareasGeneracion = new List<Task>
            {
                Task.Run(() =>
                {
                    int cant = cantidadPorRuta;
                    for (int i = 0; i < cant; i++)
                    {
                        int id = Interlocked.Increment(ref idCounter);
                        string tipo = tipos[new Random().Next(tipos.Length)];
                        var v = new Vehiculo(id, tipo, "Norte",destinoUsuario, distanciaMeta);
                        lock (listaVehiculos) { listaVehiculos.Add(v); }
                        Console.WriteLine($"[Norte] Vehículo {v.Id} ({v.Tipo}) - Destino: {v.Destino} — Task ID: {Task.CurrentId} — Hilo: {Thread.CurrentThread.ManagedThreadId}");
                    }
                }),
                Task.Run(() =>
                {
                    int cant = cantidadPorRuta;
                    for (int i = 0; i < cant; i++)
                    {
                        int id = Interlocked.Increment(ref idCounter);
                        string tipo = tipos[new Random().Next(tipos.Length)];
                        var v = new Vehiculo(id, tipo, "Sur" ,destinoUsuario, distanciaMeta);
                        lock (listaVehiculos) { listaVehiculos.Add(v); }
                        Console.WriteLine($"[Sur] Vehículo {v.Id} ({v.Tipo}) - Destino: {v.Destino} — Task ID: {Task.CurrentId} — Hilo: {Thread.CurrentThread.ManagedThreadId}");
                    }
                }),
                Task.Run(() =>
                {
                    int cant = cantidadPorRuta;
                    for (int i = 0; i < cant; i++)
                    {
                        int id = Interlocked.Increment(ref idCounter);
                        string tipo = tipos[new Random().Next(tipos.Length)];
                        var v = new Vehiculo(id, tipo, "Centro",destinoUsuario, distanciaMeta);
                        lock (listaVehiculos) { listaVehiculos.Add(v); }
                        Console.WriteLine($"[Centro] Vehículo {v.Id} ({v.Tipo}) - Destino: {v.Destino} — Task ID: {Task.CurrentId} — Hilo: {Thread.CurrentThread.ManagedThreadId}");
                    }
                })
            };

            await Task.WhenAll(tareasGeneracion);
            Console.WriteLine($"\nTotal vehiculos generados en simulacion secuencial: {listaVehiculos.Count}\n");

            // ─── VERSION SECUENCIAL (baseline) ───

            Console.WriteLine("\n[ Ejecutando version secuencial como baseline... ]\n");
            var detectorSec = new DetectorColisiones();

            // Infraestructura por ruta para el secuencial (igual que el paralelo)
            var semaforoNorteSec = new Semaforo(id: 10, tiempoVerde: 3000, tiempoAmarillo: 1000, tiempoRojo: 3000);
            var semaforoSurSec = new Semaforo(id: 20, tiempoVerde: 2000, tiempoAmarillo: 1000, tiempoRojo: 4000);
            var semaforoCentroSec = new Semaforo(id: 30, tiempoVerde: 4000, tiempoAmarillo: 1000, tiempoRojo: 2000);

            var calleNorteSec = new Calle(10, "Norte", capacidadMaxima: 3);
            var calleSurSec = new Calle(20, "Sur", capacidadMaxima: 3);
            var calleCentroSec = new Calle(30, "Centro", capacidadMaxima: 3);

            var interseccionNorteSec = new Interseccion(10, (5, 5), semaforoNorteSec, calleNorteSec, calleNorteSec, "interseccion Norte");
            var interseccionSurSec = new Interseccion(20, (10, 10), semaforoSurSec, calleSurSec, calleSurSec, "interseccion Sur");
            var interseccionCentroSec = new Interseccion(30, (15, 15), semaforoCentroSec, calleCentroSec, calleCentroSec, "interseccion Centro");

            var rutaInfraestructuraSec = new Dictionary<string, (Semaforo semaforo, Calle calle, Interseccion interseccion)>
            {
                { "Norte",  (semaforoNorteSec,  calleNorteSec,  interseccionNorteSec)  },
                { "Sur",    (semaforoSurSec,    calleSurSec,    interseccionSurSec)    },
                { "Centro", (semaforoCentroSec, calleCentroSec, interseccionCentroSec) }
            };

            var vehiculosSec = listaVehiculos
            .Select(v => new Vehiculo(v.Id, v.Tipo, v.Ruta, v.Destino, v.MetaDinamica))
            .OrderBy(v => v.Id)
            .ToList();

            var ctsSecuencial = new CancellationTokenSource();
            var controladorSec = new ControladorTeclado(ctsSecuencial, vehiculosSec, vehiculosSec.Count, destinoUsuario, distanciaMeta);
            var tareaEscuchaSec = controladorSec.IniciarEscuchaAsync();

            
            var vehiculosSecProcesados = new HashSet<int>();
            var swSec = Stopwatch.StartNew();


            await Task.Run(() =>
            {
                int metaSecuencial = 1;

                var completadosSecRuta = new Dictionary<string, int> { { "Norte", 0 }, { "Sur", 0 }, { "Centro", 0 } };

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
                            Console.WriteLine($"\n[ZONA {v.Ruta}] Iniciando vehículo {v.Id} ({v.Tipo}) — Destino: {v.Destino}");
                            if (ctsSecuencial.Token.IsCancellationRequested) break;
                            vehiculosSecProcesados.Add(v.Id);

                            var (semaforo, calle, interseccion) = rutaInfraestructuraSec[v.Ruta];

                            bool entro = calle.Entrar(v);
                            if (!entro) continue;

                            interseccion.RegistrarVehiculo(v);
                            v.Simular(semaforo, detectorSec, calle.Nombre, ctsSecuencial.Token);
                            interseccion.LiberarVehiculo(v);
                            calle.Salir(v);

                            completadosSecRuta[v.Ruta]++;
                            if (completadosSecRuta[v.Ruta] >= metaSecuencial)
                            {
                                ctsSecuencial.Cancel(); // Detenemos el baseline secuencial al llegar a 4
                                break;
                            }
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
            semaforoNorteSec.Detener();
            semaforoSurSec.Detener();
            semaforoCentroSec.Detener();
            double tiempoSecuencial = swSec.Elapsed.TotalSeconds;
            Console.WriteLine($"\n Tiempo secuencial (version secuencial): {tiempoSecuencial:F2} ms\n");

            Console.WriteLine("\n[Version secuencial terminada.]\n");

            // ─── VERSION PARALELA (baseline) ───
            // Semaforo compartido para todos los vehiculos

            var listaParalela = listaVehiculos
            .Select(v => new Vehiculo(v.Id, v.Tipo, v.Ruta, v.Destino, v.MetaDinamica))
            .ToList();

            Console.WriteLine($"\nTotal vehiculos en simulacion paralela: {listaParalela.Count}\n");

            var detectorNorte = new DetectorColisiones();
            var detectorSur = new DetectorColisiones();
            var detectorCentro = new DetectorColisiones();

            var rutaDetector = new Dictionary<string, DetectorColisiones>
            {
                { "Norte",  detectorNorte  },
                { "Sur",    detectorSur    },
                { "Centro", detectorCentro }
            };

            // 3 zonas con su propio semaforo, calle e intersección
            var semaforoNorte = new Semaforo(id: 1, tiempoVerde: 3000, tiempoAmarillo: 1000, tiempoRojo: 3000);
            var semaforoSur = new Semaforo(id: 2, tiempoVerde: 2000, tiempoAmarillo: 1000, tiempoRojo: 4000);
            var semaforoCentro = new Semaforo(id: 3, tiempoVerde: 4000, tiempoAmarillo: 1000, tiempoRojo: 2000);

            var calleNorte = new Calle(1, "Norte", capacidadMaxima: 3);
            var calleSur = new Calle(2, "Sur", capacidadMaxima: 3);
            var calleCentro = new Calle(3, "Centro", capacidadMaxima: 3);

            var interseccionNorte = new Interseccion(1, (5, 5), semaforoNorte, calleNorte, calleNorte, "interseccion Norte");
            var interseccionSur = new Interseccion(2, (10, 10), semaforoSur, calleSur, calleSur, "interseccion Sur");
            var interseccionCentro = new Interseccion(3, (15, 15), semaforoCentro, calleCentro, calleCentro, "interseccion Centro");

            var rutaInfraestructura = new Dictionary<string, (Semaforo semaforo, Calle calle, Interseccion interseccion)>
        {
            { "Norte",  (semaforoNorte,  calleNorte,  interseccionNorte)  },
            { "Sur",    (semaforoSur,    calleSur,    interseccionSur)    },
            { "Centro", (semaforoCentro, calleCentro, interseccionCentro) }
        };

            // Contador de vehiculos completados
            int vehiculosCompletados = 0;

            var completadosRuta = new ConcurrentDictionary<string, int>();
            var colisionesRuta = new ConcurrentDictionary<string, int>();

            foreach (var via in new[] { "Norte", "Sur", "Centro" })
            {
                completadosRuta[via] = 0;
                colisionesRuta[via] = 0;
            }




            // Medicion de CPU
            var proceso = Process.GetCurrentProcess();
            var cpuInicio = proceso.TotalProcessorTime;

            // Control por teclado
            using var cts = new CancellationTokenSource();
            opciones.CancellationToken = cts.Token;
            var controlador = new ControladorTeclado(cts, listaVehiculos, listaVehiculos.Count, destinoUsuario,distanciaMeta);
            var tareaEscucha = controlador.IniciarEscuchaAsync();

            Console.WriteLine("\n[ Ejecutando version paralela como baseline... ]\n");
            Console.WriteLine("\nIniciando simulación...\n");

            var stopwatch = Stopwatch.StartNew();

            var vehiculosProcesados = new HashSet<int>();
            var lockProcesados = new object();

            int metaVehiculos = 1; //
            bool metaAlcanzada = false;

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    List<Vehiculo> pendientes;
                    lock (listaParalela)
                    {
                        pendientes = listaParalela
                            .Where(v => { lock (lockProcesados) { return !vehiculosProcesados.Contains(v.Id); } })
                            .ToList();
                    }
                                    
                    if (pendientes.Count > 0)
                    {
                        Parallel.ForEach(pendientes, opciones, (vehiculo,state) =>
                        {
                            Console.WriteLine($"\n[ZONA {vehiculo.Ruta}] Iniciando vehículo {vehiculo.Id} ({vehiculo.Tipo}) — Destino: {vehiculo.Destino}");
                            // Si otra zona ya gano la exploracion, abortamos inmediatamente
                            if (metaAlcanzada) { state.Stop(); return; }

                            lock (lockProcesados) { vehiculosProcesados.Add(vehiculo.Id); }

                            var (semaforo, calle, interseccion) = rutaInfraestructura[vehiculo.Ruta];

                            bool entro = calle.Entrar(vehiculo);
                            if (!entro) return;

                            interseccion.RegistrarVehiculo(vehiculo);
                            var detectorRuta = rutaDetector[vehiculo.Ruta];
                            vehiculo.Simular(semaforo, detectorRuta, calle.Nombre, cts.Token);
                            interseccion.LiberarVehiculo(vehiculo);
                            calle.Salir(vehiculo);

                            Interlocked.Increment(ref vehiculosCompletados);

                            int totalRuta = completadosRuta.AddOrUpdate(vehiculo.Ruta, 1, (k, old) => old + 1);

                          
                            if (totalRuta >= metaVehiculos)
                            {
                                metaAlcanzada = true;
                                Console.WriteLine($"\n[EXPLORACION] ¡Ruta {vehiculo.Ruta} completo la meta! Deteniendo busqueda paralela...");
                                state.Stop();  // Detiene el bucle Parallel
                                cts.Cancel();  // Cancela hilos externos y simulación
                            }

                            // contar colisiones por zona
                            if (detectorRuta.EstaEliminado(vehiculo.Id))
                            {
                                colisionesRuta.AddOrUpdate(vehiculo.Ruta, 1, (k, old) => old + 1);
                            }
                        });
                    }
                    else
                    {
                        // No hay pendientes — espera un momento por si el usuario agrega uno
                        Thread.Sleep(300);

                        // Si despues  de esperar sigue sin pendientes, termina
                        lock (listaParalela)
                        {
                            if (listaParalela.All(v => { lock (lockProcesados) { return vehiculosProcesados.Contains(v.Id); } }))
                                break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (!metaAlcanzada)
                {
                    Console.WriteLine("\n[Sistema] Simulación cancelada manualmente por el usuario.");
                }
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
            
            // ─── COMPARACION DE Rutas (descomposicion exploratoria) ───
            var scoreRuta = new Dictionary<string, double>();

            foreach (var ruta in completadosRuta.Keys)
            {
                int completados = completadosRuta[ruta];
                int colisiones = colisionesRuta[ruta];

                // NUEVA FÓRMULA DE SCORE: 
                // El número entero (completados) da el peso de la VELOCIDAD.
                // El decimal (1.0 / colisiones+1) da el peso de la SEGURIDAD (desempate).
                double score = completados + (1.0 / (colisiones + 1.0));

                scoreRuta[ruta] = score;
            }

            // Esto garantiza que el ganador sea el más rápido, y si empatan en velocidad, gana el mas seguro.
            string mejorRuta = completadosRuta.Keys
                .OrderByDescending(r => completadosRuta[r]) // 1er Filtro: Mas completados (Velocidad)
                .ThenBy(r => colisionesRuta[r])             // 2do Filtro: Menos colisiones (Desempate)
                .First();


            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ RUTA    │ COMPLETADOS │ COLISIONES │ SCORE   │ MEJOR       ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            foreach (var ruta in scoreRuta)
            {
                string nombreRuta = ruta.Key;
                int completados = completadosRuta[nombreRuta];
                int colisiones = DetectorColisiones.ColisionesEnRuta(nombreRuta);
                double score = ruta.Value;

                string indicador = nombreRuta == mejorRuta ? "SI" : "  ";

                Console.WriteLine($"║ {nombreRuta,-7} │ {completados,11} │ {colisiones,10} │ {score,7:F2} │ {indicador,-11}║");
            }
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Mejor ruta para llegar: {mejorRuta,-34}║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
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