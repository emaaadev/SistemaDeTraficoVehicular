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

        public int Id { get; }
        public string Tipo { get; }
        public int VelocidadActual { get; set; }
        public (int X, int Y) Posicion { get; set; }
        public string Ruta { get; }
        public string Destino { get; private set; }
        public int MetaDinamica { get; private set; }

        public Vehiculo(int id, string tipo, string ruta, string destino, int meta)
        {
            Id = id;
            Tipo = tipo;
            Ruta = ruta;
            VelocidadActual = random.Value.Next(1, 5);
            Posicion = (0, 0);
            Destino = destino;
            this.MetaDinamica = meta;
        }



        public void Mover()
        {
            Posicion = (Posicion.X + VelocidadActual, Posicion.Y);

            lock (consoleLock)
            {
                Console.WriteLine($"[Vehículo {Id} - {Tipo}] Avanza a ({Posicion.X}, {Posicion.Y})");
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
        public void Mover(string razon, string nombreCalle)
        {
            Posicion = (Posicion.X + VelocidadActual, Posicion.Y);

            lock (consoleLock)
            {
                Console.WriteLine($"[Vehículo {Id} - {Tipo}] [{nombreCalle}] Avanza a ({Posicion.X}, {Posicion.Y}) - {razon}");
            }
        }

        public void Detener(string razon)
        {
            lock (consoleLock)
            {
                Console.WriteLine($"[Vehículo {Id} - {Tipo}] Se detiene en ({Posicion.X}, {Posicion.Y}) - {razon}");
            }
        }

        public void Simular(Semaforo semaforo, DetectorColisiones detector, string nombreCalle, CancellationToken token = default)
        {
            
            bool colisiono = false;
            bool llego = false;
            int meta = this.MetaDinamica; // CAMBIO: meta fija, antes estaba dentro del loop evaluandose cada iteracion

            int velocidadBase = Tipo switch
            {
                "Moto" => random.Value.Next(1, 3),
                "Auto" => 1,
                "Bus" => 1,
                "Camion" => 1,
                _ => 1
            };

            VelocidadActual = velocidadBase;

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

            while (true)
            {
                Thread.Sleep(random.Value.Next(200, 500));

                if (token.IsCancellationRequested) return;
                if (detector.EstaEliminado(Id)) return;


                // Ticket #7 - control por teclado
                
                if (!semaforo.PuedeAvanzar())
                {
                    if (Tipo == "Moto" && random.Value.Next(100) < 30)
                    {
                        Mover("Ignora semaforo (Moto)", nombreCalle);
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

                Mover("Vía libre", nombreCalle);

                // Registrar nueva posicion y verificar colision
                var colisionCon = detector.RegistrarPosicion(this);

                if (colisionCon != null)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"[COLISIÓN] Vehículo {Id} ({Tipo}) chocó con Vehículo {colisionCon.Id} ({colisionCon.Tipo}) en ({Posicion.X}, {Posicion.Y})");
                        Console.WriteLine($"Total colisiones hasta ahora: {DetectorColisiones.TotalColisiones}");
                    }
                    colisiono = true;
                    detector.LiberarPosicion(this);  // limpiar posición aquí mismo
                    detector.EliminarVehiculo(Id);   // eliminar del registro
                    return;                          // salir directo, sin pasar por el código de abajo
                }

                if (Posicion.X >= meta)
                {
                    detector.LiberarPosicion(this);  // liberar ANTES de marcar llegada
                    detector.EliminarVehiculo(Id);   // eliminar del registro para que nadie choque con él
                    llego = true;
                    break;
                }


            }

            detector.LiberarPosicion(this);

            if (!colisiono && llego)
            {
                lock (consoleLock)
                {
                    Console.WriteLine($"[Vehículo {Id} - {Tipo}] Llegó a su destino: {Destino}.\n");
                }
            }

        }
       
    }
}