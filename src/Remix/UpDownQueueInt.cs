using Menu.Remix.MixedUI;
using UnityEngine;

namespace MorePipeJukeNerfs.Remix;

public class UpDownQueueInt : OpUpdown.QueueInt
{
    private string _text;
    public float UpdownWidth { get; init; } = 56;
    public float ElementSpacing { get; init; } = 6;
    public bool LeftFocusTab { get; init; } = true;
    public bool RightFocusThis { get; init; } = true;

    public UpDownQueueInt(string text, Configurable<int> config) : base(config, null)
    {
        _text = text;
    }

    public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
    {
        List<UIelement> list = base._InitializeThisQueue(holder, posX, ref offsetY);

        OpLabel label = (OpLabel)list[1];
        label.text = _text;
        label.PosX = posX;
        label.AdjustWidthToText();

        OpUpdown updown = (OpUpdown)list[0];
        updown.PosX = label.EndX + ElementSpacing;
        updown.size = new Vector2(UpdownWidth, updown.size.y);
        updown.SetLeftRightFocusables(LeftFocusTab, RightFocusThis);

        return list;
    }
}
