using System;

namespace OMP.LSWTSS;

partial class Overlay1
{
    // Set if the overlay rendering ever throws, so one failure disables the
    // overlay instead of crashing the game on every subsequent frame.
    static bool _presentHookDisabled;

    readonly static CFuncHook1<DirectX11Bindings.SwapChainPresentMethodNativeDelegate> _directX11SwapChainPresentMethodHook = new(
        DirectX11Bindings.SwapChainPresentMethodNativePtr,
        (
            swapChainNativeHandle,
            syncInterval,
            flags
        ) =>
        {
            if (!_presentHookDisabled)
            {
                try
                {
                    foreach (var instance in _instances!)
                    {
                        if (
                            instance._directX11OverlayQuad == null
                            ||
                            instance._directX11OverlayQuad.SwapChain.NativePointer != swapChainNativeHandle
                        )
                        {
                            instance._directX11OverlayQuad?.Dispose();

                            instance._directX11OverlayQuad = new DirectX11OverlayQuad(
                                new SharpDX.DXGI.SwapChain(swapChainNativeHandle)
                            );

                            CefSharp.WebBrowserExtensions.GetBrowserHost(instance.ChromiumWebBrowser).WindowlessFrameRate = 60;

                            instance.ChromiumWebBrowser.Size = new System.Drawing.Size(
                                instance._directX11OverlayQuad.TextureWidth,
                                instance._directX11OverlayQuad.TextureHeight
                            );
                        }

                        if (instance.IsVisible)
                        {
                            instance._directX11OverlayQuad.Draw();
                        }
                    }
                }
                catch (Exception e)
                {
                    _presentHookDisabled = true;
                    Console.WriteLine("OMP.LSWTSS.Overlay1: overlay rendering disabled after error:");
                    Console.WriteLine(e);
                }
            }

            return _directX11SwapChainPresentMethodHook!.Trampoline!(swapChainNativeHandle, syncInterval, flags);
        }
    );
}
