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
        public string Zona { get; }

        public Vehiculo(int id, string tipo, string zona = "Norte")
        {
            Id = id;
            Tipo = tipo;
            Zona= zona;
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
            // TODO: AQUI AGREGAR CAMION PON LA TASA DE ALEATORIEDAD QUE PREFIERAS
            bool colisiono = false;

            int meta = random.Value.Next(20, 50); // CAMBIO: meta fija, antes estaba dentro del loop evaluandose cada iteracion

            int velocidadBase = Tipo switch
            {
                "Moto" => random.Value.Next(3, 7),
                "Auto" => random.Value.Next(2, 5),
                "Bus" => random.Value.Next(1, 3),
                "Camion" => random.Value.Next(1, 2),
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

                // probabilidades reducidas para que la colision sea ocasional (1-2% por iteración)
                // Ahora: Moto=2, Auto=1, Bus=1, Camion=1
                int probabilidadColision = Tipo switch
                {
                    "Moto" => 2, 
                    "Auto" => 1,
                    "Bus" => 1, 
                    "Camion" => 1,
                    _ => 1
                };

                if (random.Value.Next(100) < probabilidadColision)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[Vehículo {Id} - {Tipo}] Colisión por comportamiento propio.");
                    }
                    colisiono = true; 
                    detector.EliminarVehiculo(Id);
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

          if (Posicion.X >= meta) // CAMBIO: usa meta fija en lugar de random.Value.Next(20, 50) cada iteración
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