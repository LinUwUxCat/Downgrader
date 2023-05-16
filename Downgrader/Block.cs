using GBX.NET;
namespace Downgrader;
public class Block{
    public string ReplaceWith { get; set; }
    public Vec3 Offset { private get; set; }
    public Int3 IntOffset {
        get => new(((int)Offset.X), ((int)Offset.Y), ((int)Offset.Z));
    }
    public Block(){
        this.ReplaceWith = "";
        this.Offset = new(0,-8,0);
    }
}