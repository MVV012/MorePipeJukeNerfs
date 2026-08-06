using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MorePipeJukeNerfs.Remix;

public static class RemixUtils
{
    public static void ApplyHooks()
    {
        On.Menu.Remix.ConfigContainer.GetFocusables += ConfigContainer_GetFocusables;
    }

    private static List<UIfocusable> ConfigContainer_GetFocusables(On.Menu.Remix.ConfigContainer.orig_GetFocusables orig, ConfigContainer self)
    {
        List<UIfocusable> list = orig(self);
        if (ConfigContainer.activeTab?.owner?.mod.id == Plugin.GUID)
        {
            // Make greyed out UIfocusables of my mod remix menu unfocusable
            return list.Where(element => !(element.tab?.owner?.mod.id == Plugin.GUID && element.greyedOut)).ToList();
        }
        return list;
    }

    extension(UIelement element)
    {
        public float EndX => element.PosX + element.size.x;
        public float EndY => element.PosY + element.size.y;

        public void InstantSetPos(Vector2 pos)
        {
            element.SetPos(pos);
            element.lastScreenPos = element.ScreenPos;
        }
        public void InstantSetX(float x)
        {
            element.InstantSetPos(new Vector2(x, element.PosY));
        }
        public void InstantSetY(float y)
        {
            element.InstantSetPos(new Vector2(element.PosX, y));
        }
    }

    extension(UIfocusable element)
    {
        public void SetLeftRightFocusables(bool leftFocusTab = true, bool rigtFocusThis = true)
        {
            if (leftFocusTab)
            {
                element.SetNextFocusable(UIfocusable.NextDirection.Left,
                    FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton)
                );
            }
            if (rigtFocusThis)
            {
                element.SetNextFocusable(UIfocusable.NextDirection.Right, element);
            }
        }
    }

    extension(OpLabel label)
    {
        public void AdjustWidthToText()
        {
            label.size = new Vector2(label.label.textRect.width, label.size.y);
        }
    }

    extension(IEnumerable<UIelement> elements)
    {
        public UIconfig WithConfig(ConfigurableBase config)
        {
            return elements.OfType<UIconfig>().Where(element => element.cfgEntry == config).First();
        }
    }
}
