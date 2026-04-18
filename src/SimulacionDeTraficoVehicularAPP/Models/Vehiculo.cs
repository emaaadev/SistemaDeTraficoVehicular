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

            for (int i = 0; i < 10; i++)
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
                    Detener("Semáforo en rojo/amarillo - esperando...");
                    while (!semaforo.PuedeAvanzar())
                    {
                        if (_accidenteForzado) return;
                        if (token.IsCancellationRequested) return;
                        Thread.Sleep(100);
                    }
                }

                // Liberar posicion anterior antes de moverse
                detector.LiberarPosicion(this);

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