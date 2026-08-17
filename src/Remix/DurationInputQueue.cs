using Menu.Remix.MixedUI;
using UnityEngine;

namespace MorePipeJukeNerfs.Remix;

public class DurationInputQueue : UpDownQueueInt
{
    public DurationInputQueue(string text, Configurable<int> config) : base(text, config) {}

    public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
    {
        List<UIelement> list = base._InitializeThisQueue(holder, posX, ref offsetY);

        OpUpdown updown = (OpUpdown)list[0];

        OpLabel ticksSecondsLabel = new OpLabel(
            new Vector2(updown.EndX + ElementSpacing, GetPosY(holder, offsetY)),
            new Vector2(120, 30),
            "ticks (0.00 seconds)",
            FLabelAlignment.Left
        )
        {
            bumpBehav = updown.bumpBehav,
            description = updown.description
        };
        list.Add(ticksSecondsLabel);

        BindTicksSecondsLabel(updown, ticksSecondsLabel);

        return list;
    }

    public static void BindTicksSecondsLabel(OpTextBox input, OpLabel label)
    {
        UpdateLabel();
        input.OnValueUpdate += (_, _, _) => UpdateLabel();

        void UpdateLabel()
        {
            label.text = GetLabelText(input.valueInt);
            label.AdjustWidthToText();
        }

        static string GetLabelText(int ticks)
        {
            float seconds = (float)ticks / 40;
            return $"tick{(ticks != 1 ? "s" : "")} ({seconds:0.0#} second{(seconds != 1f ? "s" : "")})";
        }
    }
}
