namespace RobotSimulation
{
    public interface IFloor
    {
        /// <summary>
        /// compute the normalized lightness value
        /// </summary>
        float ComputeLightness(Rectangle rc);
    }
}
