namespace X11;

using System.Runtime.InteropServices;
using TerraFX.Interop.Xlib;
using static TerraFX.Interop.Xlib.Xlib;
using System.Text;

public unsafe class XWindow {
  public Window window;
  public Display* dpy;

  private Dictionary<string, Atom> atoms;

  public XWindow()
  {
    atoms = new();
    dpy = XOpenDisplay(null);
  }

  public void ConnectRootWindow()
  {
    window = XDefaultRootWindow(dpy);
  }

  public static Display* GetDisplay() => XOpenDisplay(null);

  public Atom GetAtom(string atomName)
  {
    if (atoms.ContainsKey(atomName))
    {
      return atoms[atomName];
    }
    fixed (byte* atom = Encoding.ASCII.GetBytes($"{atomName}\0"))
    {
      Atom interned_atom = XInternAtom(dpy, (sbyte*)atom, False);
      atoms.Add(atomName, interned_atom);
      return atoms[atomName];
    }
  }

  public void SetWMName(string text)
  {
    fixed (byte* utf8 = Encoding.UTF8.GetBytes(text))
    {
      int length = Encoding.UTF8.GetByteCount(text);

      Atom NET_WM_NAME = GetAtom("_NET_WM_NAME");
      Atom UTF8_STRING = GetAtom("UTF8_STRING");
      Atom WM_NAME = GetAtom("WM_NAME");

      XChangeProperty(
          dpy,
          window,
          NET_WM_NAME,
          UTF8_STRING,
          8,
          PropModeReplace,
          utf8,
          length
        );

      XChangeProperty(
          dpy,
          window,
          WM_NAME,
          UTF8_STRING,
          8,
          PropModeReplace,
          utf8,
          length
        );
    }
      XFlush(dpy);
  }




}

