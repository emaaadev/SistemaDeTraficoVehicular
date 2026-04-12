namespace SimulacionDeTraficoVehicularAPP.Interfaces
{
    public interface IInterseccion
    {
        int Id { get; }
        (int X, int Y) Coordenadas { get; }
        ISemaforo Semaforo { get; }
        bool HayColision(IVehiculo vehiculo);
        void RegistrarVehiculo(IVehiculo vehiculo);
        void LiberarVehiculo(IVehiculo vehiculo);
    }
}