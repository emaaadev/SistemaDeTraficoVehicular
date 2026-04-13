using SimulacionDeTraficoVehicularAPP.Models;

namespace SimulacionDeTraficoVehicularAPP.Interfaces
{
    public interface IVehiculo
    {
        int Id { get; }
        string Tipo { get; }          // "Auto", "Bus", "Moto"
        int VelocidadActual { get; set; }
        (int X, int Y) Posicion { get; set; }
        void Mover();
        void Detener();
        void Simular(Semaforo semaforo);
    }
}