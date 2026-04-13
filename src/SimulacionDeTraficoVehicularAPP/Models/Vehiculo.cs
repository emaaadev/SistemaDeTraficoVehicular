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

        public void Simular()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(random.Value.Next(200, 500));

                bool semaforoEnRojo = random.Value.Next(0, 10) < 3;
                bool decideDetenerse = random.Value.Next(0, 10) < 2;

                if (semaforoEnRojo)
                {
                    Detener("Semaforo en rojo");
                }
                else if (decideDetenerse)
                {
                    Detener("Trafico o decision del conductor");
                }
                else
                {
                    Mover("Via libre");
                }
            }

            lock (consoleLock)
            {
                Console.WriteLine($"[Vehiculo {Id} - {Tipo}] Llego a su destino.\n");
            }
        }
    }
}