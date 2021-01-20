public class ServerTick
{
    public static int GetTick(float networkTime)
    {
        if (networkTime < 10)
        {
            return (int) (networkTime * 1000) / 50;
        }
        else
        {
            return (int) ((networkTime % 10) * 1000) / 50;
        }
    }
}
