namespace SimulacionDeTraficoVehicularAPP.Interfaces
{
    public interface ISimulacion
    {
        void Iniciar(int maxProcesadores);
        void Detener();
        void MostrarMetricas();
    }
}