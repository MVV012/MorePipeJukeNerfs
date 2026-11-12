using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace MorePipeJukeNerfs;

public static class ILHookUtils
{
    extension(Instruction instr)
    {
        public bool MatchGetter<T>(string propertyName)
        {
            return instr.MatchCallOrCallvirt<T>("get_" + propertyName);
        }
    }

    extension(Type type)
    {
        public string GenericName => type.ToString().Replace("[", "<").Replace("]", ">").Replace("+", "/");
    }

    extension(ILCursor c)
    {
        public void EmitStaticDelegate(Delegate del)
        {
            if (del.GetInvocationList().Length != 1 || del.Target != null)
                throw new ArgumentException($"Emitted delegate {del.Method.Name} is not static");

            c.Emit(OpCodes.Call, del.Method);
        }
    }

    public static ILContext.Manipulator Wrap(ILContext.Manipulator ilHook)
    {
        return (ILContext il) =>
        {
            try
            {
                ilHook(il);
            }
            catch (Exception e)
            {
                LogError($"Failed to apply IL hook to {il.Method.Name}. Exception: {e}");
            }
        };
    }
}
