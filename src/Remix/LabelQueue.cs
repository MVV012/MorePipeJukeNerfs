using Menu.Remix.MixedUI;

namespace MorePipeJukeNerfs.Remix;

public class LabelQueue : OpLabel.Queue
{
    public float? Height { get; init; } = null;

    public override float sizeY => Height.HasValue ? Height.Value : base.sizeY;

    public LabelQueue(string text, FLabelAlignment alignment = FLabelAlignment.Left, bool bigText = false) : base(text, alignment, bigText) {}
}
