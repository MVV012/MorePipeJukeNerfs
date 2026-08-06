using Menu.Remix.MixedUI;

namespace MorePipeJukeNerfs.Remix;

public class CheckBoxQueue : OpCheckBox.Queue
{
    private string _text;
    public float LabelIndent { get; init; } = 10;
    public bool LeftFocusTab { get; init; } = true;
    public bool RightFocusThis { get; init; } = true;
    public float OffsetX { get; init; } = 0;

    public CheckBoxQueue(string text, Configurable<bool> config)
        : base(config, null)
    {
        _text = text;
    }

    public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
    {
        List<UIelement> list = base._InitializeThisQueue(holder, posX + OffsetX, ref offsetY);

        OpCheckBox checkBox = (OpCheckBox)list[0];
        checkBox.SetLeftRightFocusables(LeftFocusTab, RightFocusThis);

        OpLabel label = (OpLabel)list[1];
        label.autoWrap = false;
        label.text = _text;
        label.PosX += LabelIndent - 6;
        label.AdjustWidthToText();

        return list;
    }
}
