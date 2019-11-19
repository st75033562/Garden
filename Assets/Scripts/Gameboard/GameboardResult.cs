namespace Gameboard
{
    public class GameboardResult
    {
        public int[] robotScores;
        public int sceneScore;
        public GameboardResult(int numRobots)
        {
            robotScores = new int[numRobots];
        }
    }
}