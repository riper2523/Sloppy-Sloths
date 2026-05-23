public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Backward
}
public static class DirectionExtensions
{
    public static Direction Opposite(this Direction dir)
    {
        return dir switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Forward => Direction.Backward,
            Direction.Backward => Direction.Forward,
            _ => dir
        };
    }
}