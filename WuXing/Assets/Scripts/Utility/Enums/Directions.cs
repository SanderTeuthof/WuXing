[System.Flags]
public enum Directions
{
    None = 0,
    Front = 1 << 0,         
    Back = 1 << 1,          
    Left = 1 << 2,          
    Right = 1 << 3,        
    Up = 1 << 4,            
    Down = 1 << 5,         
}