namespace Bw.Entities.Simulation
{
    public interface ISteppable
    {
        public void Step();
    }

    public interface ISteppable<in TInput> where TInput : struct
    {
        public void Step(TInput input);
    }
}
