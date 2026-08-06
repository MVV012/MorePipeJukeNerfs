using MorePipeJukeNerfs.Shortcuts;

namespace MorePipeJukeNerfs;

public class RepeatingShortcutTracker
{
    private IShortcut? _lastShortcut;
    private int _ticksSinceShortcut;
    private int _repeatingShortcutCount;

    public int RepeatingShortcutCount => _repeatingShortcutCount;

    public RepeatingShortcutTracker()
    {
        _lastShortcut = null;
        _ticksSinceShortcut = 0;
        _repeatingShortcutCount = 0;
    }

    public void Update()
    {
        _ticksSinceShortcut++;
    }

    public void ExitedShortcut(IShortcut shortcut)
    {
        // TODO: move to plugin options?
        int fastChangeTicks = 60;
        int resetTicks = 15 * 40;

        if (_ticksSinceShortcut < fastChangeTicks)
        {
            _lastShortcut = shortcut;
            _repeatingShortcutCount++;
        }
        else if (_ticksSinceShortcut < resetTicks && _lastShortcut != null && shortcut.IsSameShortcut(_lastShortcut))
        {
            _repeatingShortcutCount++;
        }
        else
        {
            _lastShortcut = shortcut;
            _repeatingShortcutCount = 1;
        }
        _ticksSinceShortcut = 0;
    }
}
