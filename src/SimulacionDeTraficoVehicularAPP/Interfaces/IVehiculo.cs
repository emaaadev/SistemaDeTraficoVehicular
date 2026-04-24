using SimulacionDeTraficoVehicularAPP.Models;

namespace SimulacionDeTraficoVehicularAPP.Interfaces
{
    public interface IVehiculo
    {
        int Id { get; }
        string Tipo { get; }          // "Auto", "Bus", "Moto"
        int VelocidadActual { get; set; }
        (int X, int Y) Posicion { get; set; }
        string Ruta { get; }          // "Norte-Sur", "Este-Oeste", etc.
        void Mover();
        void Mover(string razon);
        void Mover(string razon, string nombreCalle);
        void Detener();
        void Simular(Semaforo semaforo, DetectorColisiones detector, string nombreCalle, CancellationToken token);
    }
}