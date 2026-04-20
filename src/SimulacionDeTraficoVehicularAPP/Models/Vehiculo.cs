using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimulacionDeTraficoVehicularAPP.Interfaces;

namespace SimulacionDeTraficoVehicularAPP.Models
{
    public class Vehiculo : IVehiculo
    {
        private static readonly object consoleLock = new object();

        private static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random());

        private volatile bool _accidenteForzado = false;

        public int Id { get; }
        public string Tipo { get; }
        public int VelocidadActual { get; set; }
        public (int X, int Y) Posicion { get; set; }

        public Vehiculo(int id, string tipo)
        {
            Id = id;
            Tipo = tipo;
            VelocidadActual = random.Value.Next(1, 5);
            Posicion = (0, 0);
        }

        public void Mover()
        {
            Posicion = (Posicion.X + VelocidadActual, Posicion.Y);

            lock (consoleLock)
            {
                Console.WriteLine($"[Vehiculo {Id} - {Tipo}] Avanza a ({Posicion.X}, {Posicion.Y}) - Sin razon especificada.");
            }
        }

        public void Detener()
        {
            lock (consoleLock)
            {
                Console.WriteLine($"[Vehiculo {Id} - {Tipo}] Se detiene en ({Posicion.X}, {Posicion.Y}) - Sin razon especificada.");
            }
        }



        public void Mover(string razon)
        {
            Posicion = (Posicion.X + VelocidadActual, Posicion.Y);

            lock (consoleLock)
            {
                Console.WriteLine($"[Vehículo {Id} - {Tipo}] Avanza a ({Posicion.X}, {Posicion.Y}) - {razon}");
            }
        }

        public void Detener(string razon)
        {
            lock (consoleLock)
            {
                Console.WriteLine($"[Vehículo {Id} - {Tipo}] Se detiene en ({Posicion.X}, {Posicion.Y}) - {razon}");
            }
        }

        public void Simular(Semaforo semaforo, DetectorColisiones detector, CancellationToken token = default)
        {
            bool colisiono = false;

            int velocidadBase = Tipo switch
            {
                "Moto" => random.Value.Next(3, 7),
                "Auto" => random.Value.Next(2, 5),
                "Bus" => random.Value.Next(1, 3),
                _ => 2
            };

            VelocidadActual = velocidadBase;

            while (true)
            {
                Thread.Sleep(random.Value.Next(200, 500));

                if (token.IsCancellationRequested) return;
                if (detector.EstaEliminado(Id)) return;

                if (_accidenteForzado)

                // Ticket #7 - control por teclado
                if (_accidenteForzado)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Vehículo {Id} - {Tipo}] Accidente forzado. Saliendo de la simulación.");
                    }
                    break;
                }

                if (!semaforo.PuedeAvanzar())
                {
                    if (Tipo == "Moto" && random.Value.Next(100) < 30)
                    {
                        Mover("Ignora semaforo (Moto)");
                    }
                    else
                    {
                        Detener("Semaforo en rojo/amarillo - esperando...");
                        while (!semaforo.PuedeAvanzar())
                        {
                            if (token.IsCancellationRequested) return;
                            Thread.Sleep(100);
                        }
                    }
                }

                // Liberar posicion anterior antes de moverse
                detector.LiberarPosicion(this);

                int probabilidadColision = Tipo switch
                {
                    "Moto" => 20, // mas riesgo
                    "Auto" => 10,
                    "Bus" => 5, // mas seguro
                    _ => 10
                };

                if (random.Value.Next(100) < probabilidadColision)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Vehículo {Id} - {Tipo}] Colisión por comportamiento propio.");
                    }
                    break;
                }

                Mover("Vía libre");

                // Registrar nueva posicion y verificar colision
                var colisionCon = detector.RegistrarPosicion(this);

                if (colisionCon != null)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[COLISIÓN] Vehículo {Id} ({Tipo}) chocó con Vehículo {colisionCon.Id} ({colisionCon.Tipo}) en ({Posicion.X}, {Posicion.Y})");
                        Console.WriteLine($"Total colisiones hasta ahora: {DetectorColisiones.TotalColisiones}");
                    }
                    colisiono = true; // <-- marcar colison
                    break;
                }

                // Condicion de llegada (meta aleatoria)
                if (Posicion.X >= random.Value.Next(20, 50))
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Vehículo {Id} - {Tipo}] Llegó a su destino.\n");
                    }
                    break;
                }
            }

            if (!colisiono && !_accidenteForzado)
            {
                lock (consoleLock)
                {
                    Console.WriteLine($"[Vehículo {Id} - {Tipo}] Llegó a su destino.\n");
                }
            }
        }
        public void ForzarAccidente()
        {
            _accidenteForzado = true;
        }

        public bool TieneAccidente() => _accidenteForzado;
    }
}