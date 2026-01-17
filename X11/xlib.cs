using System.Runtime.InteropServices;

namespace X11;

public struct Display { }
public struct Window { public ulong Value; }
public struct Atom { public ulong Value; }

public static class NativeXlib
{
    private const string LibX11 = "libX11.so.6";

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr XOpenDisplay([MarshalAs(UnmanagedType.LPStr)] string? displayName);

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern int XCloseDisplay(IntPtr display);

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern Window XDefaultRootWindow(IntPtr display);

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern Atom XInternAtom(IntPtr display,
        [MarshalAs(UnmanagedType.LPStr)] string atomName,
        [MarshalAs(UnmanagedType.I1)] bool onlyIfExists);

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern int XChangeProperty(IntPtr display,
        Window window,
        Atom property,
        Atom type,
        int format,
        int mode,
        [MarshalAs(UnmanagedType.LPArray)] byte[] data,
        int nelements);

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern int XFlush(IntPtr display);

    [DllImport(LibX11, CallingConvention = CallingConvention.Cdecl)]
    public static extern int XDeleteProperty(IntPtr display,
        Window window,
        Atom property);
}

