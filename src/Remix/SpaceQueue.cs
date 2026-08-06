using Menu.Remix.MixedUI;

namespace MorePipeJukeNerfs.Remix;

public class SpaceQueue : UIQueue
{
    private float _height;
    public override float sizeY => _height - 20; // Reduced by 20, because 10 pixels of space are added on both sides automatically

    public SpaceQueue(float height)
    {
        if (height <= 20)
        {
            throw new ArgumentException("SpaceQueue height must be greater than 20");
        }
        _height = height;
    }

    public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
    {
        offsetY += sizeY;
        return [];
    }
}
