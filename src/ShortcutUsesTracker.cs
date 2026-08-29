using MorePipeJukeNerfs.Shortcuts;

namespace MorePipeJukeNerfs;

public class ShortcutUsesTracker
{
    private Dictionary<IShortcut, ShortcutUsesInfo> _shortcutUses = [with(ShortcutComparer.SameShortcut)];

    public class ShortcutUsesInfo {
        public int Uses { get; set; } = 0;
        public int TicksSinceUse { get; set; } = 0;
    }

    public int ExitedShortcut(IShortcut shortcut)
    {
        ShortcutUsesInfo info = GetInfo(shortcut);

        info.Uses++;
        info.TicksSinceUse = 0;

        return info.Uses;
    }

    public void Update()
    {
        List<IShortcut> toRemove = [];
        foreach (var (shortcut, info) in _shortcutUses)
        {
            info.TicksSinceUse++;

            if (info.TicksSinceUse >= Options.ShortcutUsesResetTime.Value)
            {
                toRemove.Add(shortcut);
            }
        }
        foreach (IShortcut shortcut in toRemove)
        {
            _shortcutUses.Remove(shortcut);
        }
    }

    public ShortcutUsesInfo GetInfo(IShortcut shortcut)
    {
        return _shortcutUses.GetOrCreateValue(shortcut);
    }

    public void ClearInfo()
    {
        _shortcutUses = [with(ShortcutComparer.SameShortcut)];
    }
}

public static class DictionaryExtensions
{
    extension<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
    {
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }

    extension<TKey, TValue>(Dictionary<TKey, TValue> dict) where TValue: new()
    {
        public TValue GetOrCreateValue(TKey key)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = new TValue();
                dict.Add(key, value);
            }
            return value;
        }
    }
}