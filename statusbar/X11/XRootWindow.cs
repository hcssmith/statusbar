
namespace X11;

using static NativeXlib;
using System.Text;

public class XRootWindow : IDisposable {
  private IntPtr _display;
  private Window _rootWindow;
  private Dictionary<string, Atom> _atomCache;

  public XRootWindow() {
    _display = XOpenDisplay(null);
    _rootWindow = XDefaultRootWindow(_display);
    _atomCache = new();
  }

  private Atom GetAtom(string name) =>
    _atomCache.TryGetValue(name, out var atom) 
      ? atom 
      : _atomCache[name] = XInternAtom(_display, name, false);
  
  public void SetWindowName(string name) {
    var utf8bytes = Encoding.UTF8.GetBytes(name);

    XChangeProperty(
        _display,
        _rootWindow,
        GetAtom("_NET_WM_NAME"),
        GetAtom("UTF8_STRING"),
        8,
        0,
        utf8bytes,
        utf8bytes.Length);
    XChangeProperty(
        _display,
        _rootWindow,
        GetAtom("WM_NAME"),
        GetAtom("UTF8_STRING"),
        8,
        0,
        utf8bytes,
        utf8bytes.Length);

    XFlush(_display);

    
  }

  public void Dispose() {
    if (_display != IntPtr.Zero) {
      XCloseDisplay(_display);
    }
  }
}
