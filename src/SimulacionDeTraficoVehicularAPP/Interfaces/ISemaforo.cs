namespace SimulacionDeTraficoVehicularAPP.Interfaces
{
    public enum EstadoSemaforo { Verde, Amarillo, Rojo }

    public interface ISemaforo
    {
        int Id { get; }
        EstadoSemaforo Estado { get; }
        void CambiarEstado(EstadoSemaforo nuevoEstado);
        bool PuedeAvanzar();
    }
}