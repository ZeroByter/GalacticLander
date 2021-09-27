#if !DISABLESTEAMWORKS

// Unity 32bit Mono on Windows crashes with ThisCall/Cdecl for some reason, StdCall without the 'this' ptr is the only thing that works..? 
#if (UNITY_EDITOR_WIN && !UNITY_EDITOR_64) || (!UNITY_EDITOR && UNITY_STANDALONE_WIN && !UNITY_64)
#define STDCALL
#elif STEAMWORKS_WIN
#define THISCALL
#endif

namespace Steamworks {
    using Steamworks;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MonoPInvokeCallbackAttribute : Attribute {
        public MonoPInvokeCallbackAttribute() { }
    }

    public static class SteamCallbackBase {
        internal static IntPtr BuildVTable(CCallbackBaseVTable.RunCBDel runCallback, CCallbackBaseVTable.RunCRDel runCallResult, CCallbackBaseVTable.GetCallbackSizeBytesDel getCallbackSizeBytes) {
            var vtable = new CCallbackBaseVTable() {
                m_RunCallResult = runCallResult,
                m_RunCallback = runCallback,
                m_GetCallbackSizeBytes = getCallbackSizeBytes
            };

            var vtablePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CCallbackBaseVTable)));
            Marshal.StructureToPtr(vtable, vtablePtr, false);

            return vtablePtr;
        }

        internal static void BuildCCallbackBase(IntPtr vtablePtr, int callbackIdentity, out CCallbackBase callbackBase, out GCHandle callbackBasePtr) {
            callbackBase = new CCallbackBase() {
                m_vfptr = vtablePtr,
                m_nCallbackFlags = 0,
                m_iCallback = callbackIdentity
            };
            callbackBasePtr = GCHandle.Alloc(callbackBase, GCHandleType.Pinned);
        }
    }

    public static class SteamCallbacks {
        public static class SteamAppInstalled_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamAppInstalled_t));
            private static System.Action<Steamworks.SteamAppInstalled_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamAppInstalled_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamAppInstalled_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamAppInstalled_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamAppInstalled_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamAppInstalled_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamAppInstalled_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamAppInstalled_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamAppInstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamAppInstalled_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamAppInstalled_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamAppInstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamAppInstalled_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamAppUninstalled_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamAppUninstalled_t));
            private static System.Action<Steamworks.SteamAppUninstalled_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamAppUninstalled_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamAppUninstalled_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamAppUninstalled_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamAppUninstalled_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamAppUninstalled_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamAppUninstalled_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamAppUninstalled_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamAppUninstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamAppUninstalled_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamAppUninstalled_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamAppUninstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamAppUninstalled_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class DlcInstalled_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.DlcInstalled_t));
            private static System.Action<Steamworks.DlcInstalled_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.DlcInstalled_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.DlcInstalled_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.DlcInstalled_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.DlcInstalled_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.DlcInstalled_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.DlcInstalled_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.DlcInstalled_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.DlcInstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.DlcInstalled_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.DlcInstalled_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.DlcInstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.DlcInstalled_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RegisterActivationCodeResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RegisterActivationCodeResponse_t));
            private static System.Action<Steamworks.RegisterActivationCodeResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RegisterActivationCodeResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RegisterActivationCodeResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RegisterActivationCodeResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RegisterActivationCodeResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RegisterActivationCodeResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RegisterActivationCodeResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RegisterActivationCodeResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RegisterActivationCodeResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RegisterActivationCodeResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RegisterActivationCodeResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RegisterActivationCodeResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RegisterActivationCodeResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class NewLaunchQueryParameters_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.NewLaunchQueryParameters_t));
            private static System.Action<Steamworks.NewLaunchQueryParameters_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.NewLaunchQueryParameters_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.NewLaunchQueryParameters_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.NewLaunchQueryParameters_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.NewLaunchQueryParameters_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.NewLaunchQueryParameters_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.NewLaunchQueryParameters_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.NewLaunchQueryParameters_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.NewLaunchQueryParameters_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.NewLaunchQueryParameters_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.NewLaunchQueryParameters_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.NewLaunchQueryParameters_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.NewLaunchQueryParameters_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class AppProofOfPurchaseKeyResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.AppProofOfPurchaseKeyResponse_t));
            private static System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.AppProofOfPurchaseKeyResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.AppProofOfPurchaseKeyResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.AppProofOfPurchaseKeyResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.AppProofOfPurchaseKeyResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.AppProofOfPurchaseKeyResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.AppProofOfPurchaseKeyResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.AppProofOfPurchaseKeyResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FileDetailsResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FileDetailsResult_t));
            private static System.Action<Steamworks.FileDetailsResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FileDetailsResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FileDetailsResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FileDetailsResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FileDetailsResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FileDetailsResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FileDetailsResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FileDetailsResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FileDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FileDetailsResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FileDetailsResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FileDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FileDetailsResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class PersonaStateChange_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.PersonaStateChange_t));
            private static System.Action<Steamworks.PersonaStateChange_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.PersonaStateChange_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.PersonaStateChange_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.PersonaStateChange_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.PersonaStateChange_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.PersonaStateChange_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.PersonaStateChange_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.PersonaStateChange_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.PersonaStateChange_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.PersonaStateChange_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.PersonaStateChange_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.PersonaStateChange_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.PersonaStateChange_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameOverlayActivated_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameOverlayActivated_t));
            private static System.Action<Steamworks.GameOverlayActivated_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameOverlayActivated_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameOverlayActivated_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameOverlayActivated_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameOverlayActivated_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameOverlayActivated_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameOverlayActivated_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameOverlayActivated_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameOverlayActivated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameOverlayActivated_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameOverlayActivated_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameOverlayActivated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameOverlayActivated_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameServerChangeRequested_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameServerChangeRequested_t));
            private static System.Action<Steamworks.GameServerChangeRequested_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameServerChangeRequested_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameServerChangeRequested_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameServerChangeRequested_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameServerChangeRequested_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameServerChangeRequested_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameServerChangeRequested_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameServerChangeRequested_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameServerChangeRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameServerChangeRequested_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameServerChangeRequested_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameServerChangeRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameServerChangeRequested_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameLobbyJoinRequested_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameLobbyJoinRequested_t));
            private static System.Action<Steamworks.GameLobbyJoinRequested_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameLobbyJoinRequested_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameLobbyJoinRequested_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameLobbyJoinRequested_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameLobbyJoinRequested_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameLobbyJoinRequested_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameLobbyJoinRequested_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameLobbyJoinRequested_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameLobbyJoinRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameLobbyJoinRequested_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameLobbyJoinRequested_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameLobbyJoinRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameLobbyJoinRequested_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class AvatarImageLoaded_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.AvatarImageLoaded_t));
            private static System.Action<Steamworks.AvatarImageLoaded_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.AvatarImageLoaded_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.AvatarImageLoaded_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.AvatarImageLoaded_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.AvatarImageLoaded_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.AvatarImageLoaded_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.AvatarImageLoaded_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.AvatarImageLoaded_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.AvatarImageLoaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.AvatarImageLoaded_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.AvatarImageLoaded_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.AvatarImageLoaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.AvatarImageLoaded_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ClanOfficerListResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ClanOfficerListResponse_t));
            private static System.Action<Steamworks.ClanOfficerListResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ClanOfficerListResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ClanOfficerListResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ClanOfficerListResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ClanOfficerListResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ClanOfficerListResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ClanOfficerListResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ClanOfficerListResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ClanOfficerListResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ClanOfficerListResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ClanOfficerListResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ClanOfficerListResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ClanOfficerListResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FriendRichPresenceUpdate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FriendRichPresenceUpdate_t));
            private static System.Action<Steamworks.FriendRichPresenceUpdate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FriendRichPresenceUpdate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FriendRichPresenceUpdate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FriendRichPresenceUpdate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FriendRichPresenceUpdate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FriendRichPresenceUpdate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FriendRichPresenceUpdate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FriendRichPresenceUpdate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FriendRichPresenceUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendRichPresenceUpdate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FriendRichPresenceUpdate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FriendRichPresenceUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendRichPresenceUpdate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameRichPresenceJoinRequested_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameRichPresenceJoinRequested_t));
            private static System.Action<Steamworks.GameRichPresenceJoinRequested_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameRichPresenceJoinRequested_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameRichPresenceJoinRequested_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameRichPresenceJoinRequested_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameRichPresenceJoinRequested_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameRichPresenceJoinRequested_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameRichPresenceJoinRequested_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameRichPresenceJoinRequested_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameRichPresenceJoinRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameRichPresenceJoinRequested_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameRichPresenceJoinRequested_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameRichPresenceJoinRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameRichPresenceJoinRequested_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameConnectedClanChatMsg_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameConnectedClanChatMsg_t));
            private static System.Action<Steamworks.GameConnectedClanChatMsg_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameConnectedClanChatMsg_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameConnectedClanChatMsg_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameConnectedClanChatMsg_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameConnectedClanChatMsg_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameConnectedClanChatMsg_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameConnectedClanChatMsg_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameConnectedClanChatMsg_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameConnectedClanChatMsg_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedClanChatMsg_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameConnectedClanChatMsg_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameConnectedClanChatMsg_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedClanChatMsg_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameConnectedChatJoin_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameConnectedChatJoin_t));
            private static System.Action<Steamworks.GameConnectedChatJoin_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameConnectedChatJoin_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameConnectedChatJoin_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameConnectedChatJoin_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameConnectedChatJoin_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameConnectedChatJoin_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameConnectedChatJoin_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameConnectedChatJoin_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameConnectedChatJoin_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedChatJoin_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameConnectedChatJoin_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameConnectedChatJoin_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedChatJoin_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameConnectedChatLeave_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameConnectedChatLeave_t));
            private static System.Action<Steamworks.GameConnectedChatLeave_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameConnectedChatLeave_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameConnectedChatLeave_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameConnectedChatLeave_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameConnectedChatLeave_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameConnectedChatLeave_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameConnectedChatLeave_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameConnectedChatLeave_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameConnectedChatLeave_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedChatLeave_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameConnectedChatLeave_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameConnectedChatLeave_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedChatLeave_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class DownloadClanActivityCountsResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.DownloadClanActivityCountsResult_t));
            private static System.Action<Steamworks.DownloadClanActivityCountsResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.DownloadClanActivityCountsResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.DownloadClanActivityCountsResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.DownloadClanActivityCountsResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.DownloadClanActivityCountsResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.DownloadClanActivityCountsResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.DownloadClanActivityCountsResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.DownloadClanActivityCountsResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.DownloadClanActivityCountsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.DownloadClanActivityCountsResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.DownloadClanActivityCountsResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.DownloadClanActivityCountsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.DownloadClanActivityCountsResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class JoinClanChatRoomCompletionResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.JoinClanChatRoomCompletionResult_t));
            private static System.Action<Steamworks.JoinClanChatRoomCompletionResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.JoinClanChatRoomCompletionResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.JoinClanChatRoomCompletionResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.JoinClanChatRoomCompletionResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.JoinClanChatRoomCompletionResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.JoinClanChatRoomCompletionResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.JoinClanChatRoomCompletionResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.JoinClanChatRoomCompletionResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.JoinClanChatRoomCompletionResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.JoinClanChatRoomCompletionResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.JoinClanChatRoomCompletionResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.JoinClanChatRoomCompletionResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.JoinClanChatRoomCompletionResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameConnectedFriendChatMsg_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameConnectedFriendChatMsg_t));
            private static System.Action<Steamworks.GameConnectedFriendChatMsg_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameConnectedFriendChatMsg_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameConnectedFriendChatMsg_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameConnectedFriendChatMsg_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameConnectedFriendChatMsg_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameConnectedFriendChatMsg_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameConnectedFriendChatMsg_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameConnectedFriendChatMsg_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameConnectedFriendChatMsg_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedFriendChatMsg_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameConnectedFriendChatMsg_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameConnectedFriendChatMsg_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameConnectedFriendChatMsg_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FriendsGetFollowerCount_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FriendsGetFollowerCount_t));
            private static System.Action<Steamworks.FriendsGetFollowerCount_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FriendsGetFollowerCount_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FriendsGetFollowerCount_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FriendsGetFollowerCount_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FriendsGetFollowerCount_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FriendsGetFollowerCount_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FriendsGetFollowerCount_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FriendsGetFollowerCount_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FriendsGetFollowerCount_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendsGetFollowerCount_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FriendsGetFollowerCount_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FriendsGetFollowerCount_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendsGetFollowerCount_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FriendsIsFollowing_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FriendsIsFollowing_t));
            private static System.Action<Steamworks.FriendsIsFollowing_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FriendsIsFollowing_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FriendsIsFollowing_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FriendsIsFollowing_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FriendsIsFollowing_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FriendsIsFollowing_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FriendsIsFollowing_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FriendsIsFollowing_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FriendsIsFollowing_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendsIsFollowing_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FriendsIsFollowing_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FriendsIsFollowing_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendsIsFollowing_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FriendsEnumerateFollowingList_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FriendsEnumerateFollowingList_t));
            private static System.Action<Steamworks.FriendsEnumerateFollowingList_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FriendsEnumerateFollowingList_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FriendsEnumerateFollowingList_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FriendsEnumerateFollowingList_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FriendsEnumerateFollowingList_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FriendsEnumerateFollowingList_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FriendsEnumerateFollowingList_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FriendsEnumerateFollowingList_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FriendsEnumerateFollowingList_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendsEnumerateFollowingList_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FriendsEnumerateFollowingList_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FriendsEnumerateFollowingList_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FriendsEnumerateFollowingList_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SetPersonaNameResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SetPersonaNameResponse_t));
            private static System.Action<Steamworks.SetPersonaNameResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SetPersonaNameResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SetPersonaNameResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SetPersonaNameResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SetPersonaNameResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SetPersonaNameResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SetPersonaNameResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SetPersonaNameResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SetPersonaNameResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SetPersonaNameResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SetPersonaNameResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SetPersonaNameResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SetPersonaNameResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GCMessageAvailable_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GCMessageAvailable_t));
            private static System.Action<Steamworks.GCMessageAvailable_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GCMessageAvailable_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GCMessageAvailable_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GCMessageAvailable_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GCMessageAvailable_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GCMessageAvailable_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GCMessageAvailable_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GCMessageAvailable_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GCMessageAvailable_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GCMessageAvailable_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GCMessageAvailable_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GCMessageAvailable_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GCMessageAvailable_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GCMessageFailed_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GCMessageFailed_t));
            private static System.Action<Steamworks.GCMessageFailed_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GCMessageFailed_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GCMessageFailed_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GCMessageFailed_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GCMessageFailed_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GCMessageFailed_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GCMessageFailed_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GCMessageFailed_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GCMessageFailed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GCMessageFailed_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GCMessageFailed_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GCMessageFailed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GCMessageFailed_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSClientApprove_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSClientApprove_t));
            private static System.Action<Steamworks.GSClientApprove_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSClientApprove_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSClientApprove_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSClientApprove_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSClientApprove_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSClientApprove_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSClientApprove_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSClientApprove_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSClientApprove_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientApprove_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSClientApprove_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSClientApprove_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientApprove_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSClientDeny_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSClientDeny_t));
            private static System.Action<Steamworks.GSClientDeny_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSClientDeny_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSClientDeny_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSClientDeny_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSClientDeny_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSClientDeny_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSClientDeny_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSClientDeny_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSClientDeny_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientDeny_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSClientDeny_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSClientDeny_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientDeny_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSClientKick_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSClientKick_t));
            private static System.Action<Steamworks.GSClientKick_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSClientKick_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSClientKick_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSClientKick_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSClientKick_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSClientKick_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSClientKick_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSClientKick_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSClientKick_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientKick_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSClientKick_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSClientKick_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientKick_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSClientAchievementStatus_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSClientAchievementStatus_t));
            private static System.Action<Steamworks.GSClientAchievementStatus_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSClientAchievementStatus_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSClientAchievementStatus_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSClientAchievementStatus_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSClientAchievementStatus_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSClientAchievementStatus_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSClientAchievementStatus_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSClientAchievementStatus_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSClientAchievementStatus_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientAchievementStatus_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSClientAchievementStatus_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSClientAchievementStatus_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientAchievementStatus_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSPolicyResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSPolicyResponse_t));
            private static System.Action<Steamworks.GSPolicyResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSPolicyResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSPolicyResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSPolicyResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSPolicyResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSPolicyResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSPolicyResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSPolicyResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSPolicyResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSPolicyResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSPolicyResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSPolicyResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSPolicyResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSGameplayStats_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSGameplayStats_t));
            private static System.Action<Steamworks.GSGameplayStats_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSGameplayStats_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSGameplayStats_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSGameplayStats_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSGameplayStats_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSGameplayStats_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSGameplayStats_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSGameplayStats_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSGameplayStats_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSGameplayStats_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSGameplayStats_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSGameplayStats_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSGameplayStats_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSClientGroupStatus_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSClientGroupStatus_t));
            private static System.Action<Steamworks.GSClientGroupStatus_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSClientGroupStatus_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSClientGroupStatus_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSClientGroupStatus_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSClientGroupStatus_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSClientGroupStatus_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSClientGroupStatus_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSClientGroupStatus_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSClientGroupStatus_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientGroupStatus_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSClientGroupStatus_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSClientGroupStatus_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSClientGroupStatus_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSReputation_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSReputation_t));
            private static System.Action<Steamworks.GSReputation_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSReputation_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSReputation_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSReputation_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSReputation_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSReputation_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSReputation_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSReputation_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSReputation_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSReputation_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSReputation_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSReputation_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSReputation_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class AssociateWithClanResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.AssociateWithClanResult_t));
            private static System.Action<Steamworks.AssociateWithClanResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.AssociateWithClanResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.AssociateWithClanResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.AssociateWithClanResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.AssociateWithClanResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.AssociateWithClanResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.AssociateWithClanResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.AssociateWithClanResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.AssociateWithClanResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.AssociateWithClanResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.AssociateWithClanResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.AssociateWithClanResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.AssociateWithClanResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ComputeNewPlayerCompatibilityResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ComputeNewPlayerCompatibilityResult_t));
            private static System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ComputeNewPlayerCompatibilityResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ComputeNewPlayerCompatibilityResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ComputeNewPlayerCompatibilityResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ComputeNewPlayerCompatibilityResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ComputeNewPlayerCompatibilityResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ComputeNewPlayerCompatibilityResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ComputeNewPlayerCompatibilityResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSStatsReceived_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSStatsReceived_t));
            private static System.Action<Steamworks.GSStatsReceived_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSStatsReceived_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSStatsReceived_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSStatsReceived_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSStatsReceived_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSStatsReceived_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSStatsReceived_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSStatsReceived_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSStatsReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSStatsReceived_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSStatsReceived_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSStatsReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSStatsReceived_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSStatsStored_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSStatsStored_t));
            private static System.Action<Steamworks.GSStatsStored_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSStatsStored_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSStatsStored_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSStatsStored_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSStatsStored_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSStatsStored_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSStatsStored_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSStatsStored_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSStatsStored_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSStatsStored_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSStatsStored_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSStatsStored_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSStatsStored_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GSStatsUnloaded_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GSStatsUnloaded_t));
            private static System.Action<Steamworks.GSStatsUnloaded_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GSStatsUnloaded_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GSStatsUnloaded_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GSStatsUnloaded_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GSStatsUnloaded_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GSStatsUnloaded_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GSStatsUnloaded_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GSStatsUnloaded_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GSStatsUnloaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSStatsUnloaded_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GSStatsUnloaded_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GSStatsUnloaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GSStatsUnloaded_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_BrowserReady_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_BrowserReady_t));
            private static System.Action<Steamworks.HTML_BrowserReady_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_BrowserReady_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_BrowserReady_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_BrowserReady_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_BrowserReady_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_BrowserReady_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_BrowserReady_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_BrowserReady_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_BrowserReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_BrowserReady_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_BrowserReady_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_BrowserReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_BrowserReady_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_NeedsPaint_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_NeedsPaint_t));
            private static System.Action<Steamworks.HTML_NeedsPaint_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_NeedsPaint_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_NeedsPaint_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_NeedsPaint_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_NeedsPaint_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_NeedsPaint_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_NeedsPaint_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_NeedsPaint_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_NeedsPaint_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_NeedsPaint_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_NeedsPaint_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_NeedsPaint_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_NeedsPaint_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_StartRequest_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_StartRequest_t));
            private static System.Action<Steamworks.HTML_StartRequest_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_StartRequest_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_StartRequest_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_StartRequest_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_StartRequest_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_StartRequest_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_StartRequest_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_StartRequest_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_StartRequest_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_StartRequest_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_StartRequest_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_StartRequest_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_StartRequest_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_CloseBrowser_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_CloseBrowser_t));
            private static System.Action<Steamworks.HTML_CloseBrowser_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_CloseBrowser_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_CloseBrowser_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_CloseBrowser_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_CloseBrowser_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_CloseBrowser_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_CloseBrowser_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_CloseBrowser_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_CloseBrowser_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_CloseBrowser_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_CloseBrowser_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_CloseBrowser_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_CloseBrowser_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_URLChanged_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_URLChanged_t));
            private static System.Action<Steamworks.HTML_URLChanged_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_URLChanged_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_URLChanged_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_URLChanged_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_URLChanged_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_URLChanged_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_URLChanged_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_URLChanged_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_URLChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_URLChanged_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_URLChanged_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_URLChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_URLChanged_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_FinishedRequest_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_FinishedRequest_t));
            private static System.Action<Steamworks.HTML_FinishedRequest_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_FinishedRequest_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_FinishedRequest_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_FinishedRequest_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_FinishedRequest_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_FinishedRequest_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_FinishedRequest_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_FinishedRequest_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_FinishedRequest_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_FinishedRequest_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_FinishedRequest_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_FinishedRequest_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_FinishedRequest_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_OpenLinkInNewTab_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_OpenLinkInNewTab_t));
            private static System.Action<Steamworks.HTML_OpenLinkInNewTab_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_OpenLinkInNewTab_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_OpenLinkInNewTab_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_OpenLinkInNewTab_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_OpenLinkInNewTab_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_OpenLinkInNewTab_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_OpenLinkInNewTab_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_OpenLinkInNewTab_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_OpenLinkInNewTab_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_OpenLinkInNewTab_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_OpenLinkInNewTab_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_OpenLinkInNewTab_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_OpenLinkInNewTab_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_ChangedTitle_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_ChangedTitle_t));
            private static System.Action<Steamworks.HTML_ChangedTitle_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_ChangedTitle_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_ChangedTitle_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_ChangedTitle_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_ChangedTitle_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_ChangedTitle_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_ChangedTitle_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_ChangedTitle_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_ChangedTitle_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_ChangedTitle_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_ChangedTitle_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_ChangedTitle_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_ChangedTitle_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_SearchResults_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_SearchResults_t));
            private static System.Action<Steamworks.HTML_SearchResults_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_SearchResults_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_SearchResults_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_SearchResults_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_SearchResults_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_SearchResults_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_SearchResults_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_SearchResults_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_SearchResults_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_SearchResults_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_SearchResults_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_SearchResults_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_SearchResults_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_CanGoBackAndForward_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_CanGoBackAndForward_t));
            private static System.Action<Steamworks.HTML_CanGoBackAndForward_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_CanGoBackAndForward_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_CanGoBackAndForward_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_CanGoBackAndForward_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_CanGoBackAndForward_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_CanGoBackAndForward_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_CanGoBackAndForward_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_CanGoBackAndForward_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_CanGoBackAndForward_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_CanGoBackAndForward_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_CanGoBackAndForward_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_CanGoBackAndForward_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_CanGoBackAndForward_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_HorizontalScroll_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_HorizontalScroll_t));
            private static System.Action<Steamworks.HTML_HorizontalScroll_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_HorizontalScroll_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_HorizontalScroll_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_HorizontalScroll_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_HorizontalScroll_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_HorizontalScroll_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_HorizontalScroll_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_HorizontalScroll_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_HorizontalScroll_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_HorizontalScroll_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_HorizontalScroll_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_HorizontalScroll_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_HorizontalScroll_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_VerticalScroll_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_VerticalScroll_t));
            private static System.Action<Steamworks.HTML_VerticalScroll_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_VerticalScroll_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_VerticalScroll_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_VerticalScroll_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_VerticalScroll_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_VerticalScroll_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_VerticalScroll_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_VerticalScroll_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_VerticalScroll_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_VerticalScroll_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_VerticalScroll_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_VerticalScroll_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_VerticalScroll_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_LinkAtPosition_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_LinkAtPosition_t));
            private static System.Action<Steamworks.HTML_LinkAtPosition_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_LinkAtPosition_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_LinkAtPosition_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_LinkAtPosition_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_LinkAtPosition_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_LinkAtPosition_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_LinkAtPosition_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_LinkAtPosition_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_LinkAtPosition_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_LinkAtPosition_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_LinkAtPosition_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_LinkAtPosition_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_LinkAtPosition_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_JSAlert_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_JSAlert_t));
            private static System.Action<Steamworks.HTML_JSAlert_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_JSAlert_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_JSAlert_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_JSAlert_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_JSAlert_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_JSAlert_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_JSAlert_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_JSAlert_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_JSAlert_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_JSAlert_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_JSAlert_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_JSAlert_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_JSAlert_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_JSConfirm_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_JSConfirm_t));
            private static System.Action<Steamworks.HTML_JSConfirm_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_JSConfirm_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_JSConfirm_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_JSConfirm_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_JSConfirm_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_JSConfirm_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_JSConfirm_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_JSConfirm_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_JSConfirm_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_JSConfirm_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_JSConfirm_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_JSConfirm_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_JSConfirm_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_FileOpenDialog_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_FileOpenDialog_t));
            private static System.Action<Steamworks.HTML_FileOpenDialog_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_FileOpenDialog_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_FileOpenDialog_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_FileOpenDialog_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_FileOpenDialog_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_FileOpenDialog_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_FileOpenDialog_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_FileOpenDialog_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_FileOpenDialog_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_FileOpenDialog_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_FileOpenDialog_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_FileOpenDialog_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_FileOpenDialog_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_NewWindow_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_NewWindow_t));
            private static System.Action<Steamworks.HTML_NewWindow_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_NewWindow_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_NewWindow_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_NewWindow_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_NewWindow_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_NewWindow_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_NewWindow_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_NewWindow_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_NewWindow_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_NewWindow_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_NewWindow_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_NewWindow_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_NewWindow_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_SetCursor_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_SetCursor_t));
            private static System.Action<Steamworks.HTML_SetCursor_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_SetCursor_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_SetCursor_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_SetCursor_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_SetCursor_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_SetCursor_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_SetCursor_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_SetCursor_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_SetCursor_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_SetCursor_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_SetCursor_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_SetCursor_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_SetCursor_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_StatusText_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_StatusText_t));
            private static System.Action<Steamworks.HTML_StatusText_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_StatusText_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_StatusText_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_StatusText_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_StatusText_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_StatusText_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_StatusText_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_StatusText_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_StatusText_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_StatusText_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_StatusText_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_StatusText_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_StatusText_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_ShowToolTip_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_ShowToolTip_t));
            private static System.Action<Steamworks.HTML_ShowToolTip_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_ShowToolTip_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_ShowToolTip_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_ShowToolTip_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_ShowToolTip_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_ShowToolTip_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_ShowToolTip_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_ShowToolTip_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_ShowToolTip_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_ShowToolTip_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_ShowToolTip_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_ShowToolTip_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_ShowToolTip_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_UpdateToolTip_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_UpdateToolTip_t));
            private static System.Action<Steamworks.HTML_UpdateToolTip_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_UpdateToolTip_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_UpdateToolTip_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_UpdateToolTip_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_UpdateToolTip_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_UpdateToolTip_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_UpdateToolTip_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_UpdateToolTip_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_UpdateToolTip_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_UpdateToolTip_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_UpdateToolTip_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_UpdateToolTip_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_UpdateToolTip_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTML_HideToolTip_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTML_HideToolTip_t));
            private static System.Action<Steamworks.HTML_HideToolTip_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTML_HideToolTip_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTML_HideToolTip_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTML_HideToolTip_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTML_HideToolTip_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTML_HideToolTip_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTML_HideToolTip_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTML_HideToolTip_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTML_HideToolTip_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_HideToolTip_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTML_HideToolTip_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTML_HideToolTip_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTML_HideToolTip_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTTPRequestCompleted_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTTPRequestCompleted_t));
            private static System.Action<Steamworks.HTTPRequestCompleted_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTTPRequestCompleted_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTTPRequestCompleted_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTTPRequestCompleted_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTTPRequestCompleted_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTTPRequestCompleted_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTTPRequestCompleted_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTTPRequestCompleted_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTTPRequestCompleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTTPRequestCompleted_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTTPRequestCompleted_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTTPRequestCompleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTTPRequestCompleted_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTTPRequestHeadersReceived_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTTPRequestHeadersReceived_t));
            private static System.Action<Steamworks.HTTPRequestHeadersReceived_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTTPRequestHeadersReceived_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTTPRequestHeadersReceived_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTTPRequestHeadersReceived_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTTPRequestHeadersReceived_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTTPRequestHeadersReceived_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTTPRequestHeadersReceived_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTTPRequestHeadersReceived_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTTPRequestHeadersReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTTPRequestHeadersReceived_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTTPRequestHeadersReceived_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTTPRequestHeadersReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTTPRequestHeadersReceived_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class HTTPRequestDataReceived_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.HTTPRequestDataReceived_t));
            private static System.Action<Steamworks.HTTPRequestDataReceived_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.HTTPRequestDataReceived_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.HTTPRequestDataReceived_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.HTTPRequestDataReceived_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.HTTPRequestDataReceived_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.HTTPRequestDataReceived_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.HTTPRequestDataReceived_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.HTTPRequestDataReceived_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.HTTPRequestDataReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTTPRequestDataReceived_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.HTTPRequestDataReceived_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.HTTPRequestDataReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.HTTPRequestDataReceived_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamInventoryResultReady_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamInventoryResultReady_t));
            private static System.Action<Steamworks.SteamInventoryResultReady_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamInventoryResultReady_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamInventoryResultReady_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamInventoryResultReady_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamInventoryResultReady_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamInventoryResultReady_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamInventoryResultReady_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamInventoryResultReady_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamInventoryResultReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryResultReady_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamInventoryResultReady_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamInventoryResultReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryResultReady_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamInventoryFullUpdate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamInventoryFullUpdate_t));
            private static System.Action<Steamworks.SteamInventoryFullUpdate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamInventoryFullUpdate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamInventoryFullUpdate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamInventoryFullUpdate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamInventoryFullUpdate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamInventoryFullUpdate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamInventoryFullUpdate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamInventoryFullUpdate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamInventoryFullUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryFullUpdate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamInventoryFullUpdate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamInventoryFullUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryFullUpdate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamInventoryDefinitionUpdate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamInventoryDefinitionUpdate_t));
            private static System.Action<Steamworks.SteamInventoryDefinitionUpdate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamInventoryDefinitionUpdate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamInventoryDefinitionUpdate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamInventoryDefinitionUpdate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamInventoryDefinitionUpdate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamInventoryDefinitionUpdate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamInventoryDefinitionUpdate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamInventoryDefinitionUpdate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamInventoryDefinitionUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryDefinitionUpdate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamInventoryDefinitionUpdate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamInventoryDefinitionUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryDefinitionUpdate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamInventoryEligiblePromoItemDefIDs_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamInventoryEligiblePromoItemDefIDs_t));
            private static System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamInventoryEligiblePromoItemDefIDs_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamInventoryEligiblePromoItemDefIDs_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamInventoryEligiblePromoItemDefIDs_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryEligiblePromoItemDefIDs_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamInventoryEligiblePromoItemDefIDs_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamInventoryEligiblePromoItemDefIDs_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamInventoryEligiblePromoItemDefIDs_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FavoritesListChanged_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FavoritesListChanged_t));
            private static System.Action<Steamworks.FavoritesListChanged_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FavoritesListChanged_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FavoritesListChanged_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FavoritesListChanged_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FavoritesListChanged_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FavoritesListChanged_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FavoritesListChanged_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FavoritesListChanged_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FavoritesListChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FavoritesListChanged_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FavoritesListChanged_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FavoritesListChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FavoritesListChanged_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyInvite_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyInvite_t));
            private static System.Action<Steamworks.LobbyInvite_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyInvite_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyInvite_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyInvite_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyInvite_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyInvite_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyInvite_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyInvite_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyInvite_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyInvite_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyInvite_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyInvite_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyInvite_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyEnter_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyEnter_t));
            private static System.Action<Steamworks.LobbyEnter_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyEnter_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyEnter_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyEnter_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyEnter_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyEnter_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyEnter_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyEnter_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyEnter_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyEnter_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyEnter_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyEnter_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyEnter_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyDataUpdate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyDataUpdate_t));
            private static System.Action<Steamworks.LobbyDataUpdate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyDataUpdate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyDataUpdate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyDataUpdate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyDataUpdate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyDataUpdate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyDataUpdate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyDataUpdate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyDataUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyDataUpdate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyDataUpdate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyDataUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyDataUpdate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyChatUpdate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyChatUpdate_t));
            private static System.Action<Steamworks.LobbyChatUpdate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyChatUpdate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyChatUpdate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyChatUpdate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyChatUpdate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyChatUpdate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyChatUpdate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyChatUpdate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyChatUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyChatUpdate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyChatUpdate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyChatUpdate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyChatUpdate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyChatMsg_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyChatMsg_t));
            private static System.Action<Steamworks.LobbyChatMsg_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyChatMsg_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyChatMsg_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyChatMsg_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyChatMsg_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyChatMsg_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyChatMsg_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyChatMsg_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyChatMsg_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyChatMsg_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyChatMsg_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyChatMsg_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyChatMsg_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyGameCreated_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyGameCreated_t));
            private static System.Action<Steamworks.LobbyGameCreated_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyGameCreated_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyGameCreated_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyGameCreated_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyGameCreated_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyGameCreated_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyGameCreated_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyGameCreated_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyGameCreated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyGameCreated_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyGameCreated_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyGameCreated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyGameCreated_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyMatchList_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyMatchList_t));
            private static System.Action<Steamworks.LobbyMatchList_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyMatchList_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyMatchList_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyMatchList_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyMatchList_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyMatchList_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyMatchList_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyMatchList_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyMatchList_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyMatchList_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyMatchList_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyMatchList_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyMatchList_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyKicked_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyKicked_t));
            private static System.Action<Steamworks.LobbyKicked_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyKicked_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyKicked_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyKicked_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyKicked_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyKicked_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyKicked_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyKicked_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyKicked_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyKicked_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyKicked_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyKicked_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyKicked_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LobbyCreated_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LobbyCreated_t));
            private static System.Action<Steamworks.LobbyCreated_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LobbyCreated_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LobbyCreated_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LobbyCreated_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LobbyCreated_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LobbyCreated_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LobbyCreated_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LobbyCreated_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LobbyCreated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyCreated_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LobbyCreated_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LobbyCreated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LobbyCreated_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class FavoritesListAccountsUpdated_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.FavoritesListAccountsUpdated_t));
            private static System.Action<Steamworks.FavoritesListAccountsUpdated_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.FavoritesListAccountsUpdated_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.FavoritesListAccountsUpdated_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.FavoritesListAccountsUpdated_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.FavoritesListAccountsUpdated_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.FavoritesListAccountsUpdated_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.FavoritesListAccountsUpdated_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.FavoritesListAccountsUpdated_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.FavoritesListAccountsUpdated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FavoritesListAccountsUpdated_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.FavoritesListAccountsUpdated_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.FavoritesListAccountsUpdated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.FavoritesListAccountsUpdated_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class PlaybackStatusHasChanged_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.PlaybackStatusHasChanged_t));
            private static System.Action<Steamworks.PlaybackStatusHasChanged_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.PlaybackStatusHasChanged_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.PlaybackStatusHasChanged_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.PlaybackStatusHasChanged_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.PlaybackStatusHasChanged_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.PlaybackStatusHasChanged_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.PlaybackStatusHasChanged_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.PlaybackStatusHasChanged_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.PlaybackStatusHasChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.PlaybackStatusHasChanged_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.PlaybackStatusHasChanged_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.PlaybackStatusHasChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.PlaybackStatusHasChanged_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class VolumeHasChanged_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.VolumeHasChanged_t));
            private static System.Action<Steamworks.VolumeHasChanged_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.VolumeHasChanged_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.VolumeHasChanged_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.VolumeHasChanged_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.VolumeHasChanged_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.VolumeHasChanged_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.VolumeHasChanged_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.VolumeHasChanged_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.VolumeHasChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.VolumeHasChanged_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.VolumeHasChanged_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.VolumeHasChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.VolumeHasChanged_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerRemoteWillActivate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerRemoteWillActivate_t));
            private static System.Action<Steamworks.MusicPlayerRemoteWillActivate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerRemoteWillActivate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerRemoteWillActivate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerRemoteWillActivate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerRemoteWillActivate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerRemoteWillActivate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerRemoteWillActivate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerRemoteWillActivate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerRemoteWillActivate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerRemoteWillActivate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerRemoteWillActivate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerRemoteWillActivate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerRemoteWillActivate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerRemoteWillDeactivate_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerRemoteWillDeactivate_t));
            private static System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerRemoteWillDeactivate_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerRemoteWillDeactivate_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerRemoteWillDeactivate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerRemoteWillDeactivate_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerRemoteWillDeactivate_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerRemoteWillDeactivate_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerRemoteWillDeactivate_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerRemoteToFront_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerRemoteToFront_t));
            private static System.Action<Steamworks.MusicPlayerRemoteToFront_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerRemoteToFront_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerRemoteToFront_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerRemoteToFront_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerRemoteToFront_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerRemoteToFront_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerRemoteToFront_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerRemoteToFront_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerRemoteToFront_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerRemoteToFront_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerRemoteToFront_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerRemoteToFront_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerRemoteToFront_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWillQuit_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWillQuit_t));
            private static System.Action<Steamworks.MusicPlayerWillQuit_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWillQuit_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWillQuit_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWillQuit_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWillQuit_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWillQuit_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWillQuit_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWillQuit_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWillQuit_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWillQuit_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWillQuit_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWillQuit_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWillQuit_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsPlay_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsPlay_t));
            private static System.Action<Steamworks.MusicPlayerWantsPlay_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlay_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlay_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsPlay_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsPlay_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsPlay_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsPlay_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsPlay_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsPlay_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlay_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsPlay_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsPlay_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlay_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsPause_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsPause_t));
            private static System.Action<Steamworks.MusicPlayerWantsPause_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPause_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPause_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsPause_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsPause_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsPause_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsPause_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsPause_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsPause_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPause_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsPause_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsPause_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPause_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsPlayPrevious_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsPlayPrevious_t));
            private static System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsPlayPrevious_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsPlayPrevious_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsPlayPrevious_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlayPrevious_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsPlayPrevious_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsPlayPrevious_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlayPrevious_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsPlayNext_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsPlayNext_t));
            private static System.Action<Steamworks.MusicPlayerWantsPlayNext_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlayNext_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlayNext_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsPlayNext_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsPlayNext_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsPlayNext_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsPlayNext_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsPlayNext_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsPlayNext_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlayNext_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsPlayNext_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsPlayNext_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlayNext_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsShuffled_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsShuffled_t));
            private static System.Action<Steamworks.MusicPlayerWantsShuffled_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsShuffled_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsShuffled_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsShuffled_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsShuffled_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsShuffled_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsShuffled_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsShuffled_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsShuffled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsShuffled_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsShuffled_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsShuffled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsShuffled_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsLooped_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsLooped_t));
            private static System.Action<Steamworks.MusicPlayerWantsLooped_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsLooped_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsLooped_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsLooped_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsLooped_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsLooped_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsLooped_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsLooped_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsLooped_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsLooped_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsLooped_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsLooped_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsLooped_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsVolume_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsVolume_t));
            private static System.Action<Steamworks.MusicPlayerWantsVolume_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsVolume_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsVolume_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsVolume_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsVolume_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsVolume_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsVolume_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsVolume_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsVolume_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsVolume_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsVolume_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsVolume_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsVolume_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerSelectsQueueEntry_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerSelectsQueueEntry_t));
            private static System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerSelectsQueueEntry_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerSelectsQueueEntry_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerSelectsQueueEntry_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerSelectsQueueEntry_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerSelectsQueueEntry_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerSelectsQueueEntry_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerSelectsQueueEntry_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerSelectsPlaylistEntry_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerSelectsPlaylistEntry_t));
            private static System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerSelectsPlaylistEntry_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerSelectsPlaylistEntry_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerSelectsPlaylistEntry_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerSelectsPlaylistEntry_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerSelectsPlaylistEntry_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerSelectsPlaylistEntry_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerSelectsPlaylistEntry_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MusicPlayerWantsPlayingRepeatStatus_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MusicPlayerWantsPlayingRepeatStatus_t));
            private static System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MusicPlayerWantsPlayingRepeatStatus_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MusicPlayerWantsPlayingRepeatStatus_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MusicPlayerWantsPlayingRepeatStatus_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlayingRepeatStatus_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MusicPlayerWantsPlayingRepeatStatus_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MusicPlayerWantsPlayingRepeatStatus_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MusicPlayerWantsPlayingRepeatStatus_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class P2PSessionRequest_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.P2PSessionRequest_t));
            private static System.Action<Steamworks.P2PSessionRequest_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.P2PSessionRequest_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.P2PSessionRequest_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.P2PSessionRequest_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.P2PSessionRequest_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.P2PSessionRequest_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.P2PSessionRequest_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.P2PSessionRequest_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.P2PSessionRequest_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.P2PSessionRequest_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.P2PSessionRequest_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.P2PSessionRequest_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.P2PSessionRequest_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class P2PSessionConnectFail_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.P2PSessionConnectFail_t));
            private static System.Action<Steamworks.P2PSessionConnectFail_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.P2PSessionConnectFail_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.P2PSessionConnectFail_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.P2PSessionConnectFail_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.P2PSessionConnectFail_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.P2PSessionConnectFail_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.P2PSessionConnectFail_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.P2PSessionConnectFail_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.P2PSessionConnectFail_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.P2PSessionConnectFail_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.P2PSessionConnectFail_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.P2PSessionConnectFail_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.P2PSessionConnectFail_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SocketStatusCallback_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SocketStatusCallback_t));
            private static System.Action<Steamworks.SocketStatusCallback_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SocketStatusCallback_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SocketStatusCallback_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SocketStatusCallback_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SocketStatusCallback_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SocketStatusCallback_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SocketStatusCallback_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SocketStatusCallback_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SocketStatusCallback_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SocketStatusCallback_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SocketStatusCallback_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SocketStatusCallback_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SocketStatusCallback_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageAppSyncedClient_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageAppSyncedClient_t));
            private static System.Action<Steamworks.RemoteStorageAppSyncedClient_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncedClient_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncedClient_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageAppSyncedClient_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageAppSyncedClient_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageAppSyncedClient_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageAppSyncedClient_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageAppSyncedClient_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageAppSyncedClient_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncedClient_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageAppSyncedClient_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageAppSyncedClient_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncedClient_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageAppSyncedServer_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageAppSyncedServer_t));
            private static System.Action<Steamworks.RemoteStorageAppSyncedServer_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncedServer_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncedServer_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageAppSyncedServer_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageAppSyncedServer_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageAppSyncedServer_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageAppSyncedServer_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageAppSyncedServer_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageAppSyncedServer_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncedServer_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageAppSyncedServer_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageAppSyncedServer_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncedServer_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageAppSyncProgress_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageAppSyncProgress_t));
            private static System.Action<Steamworks.RemoteStorageAppSyncProgress_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncProgress_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncProgress_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageAppSyncProgress_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageAppSyncProgress_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageAppSyncProgress_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageAppSyncProgress_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageAppSyncProgress_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageAppSyncProgress_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncProgress_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageAppSyncProgress_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageAppSyncProgress_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncProgress_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageAppSyncStatusCheck_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageAppSyncStatusCheck_t));
            private static System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageAppSyncStatusCheck_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageAppSyncStatusCheck_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageAppSyncStatusCheck_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncStatusCheck_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageAppSyncStatusCheck_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageAppSyncStatusCheck_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageAppSyncStatusCheck_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageFileShareResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageFileShareResult_t));
            private static System.Action<Steamworks.RemoteStorageFileShareResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageFileShareResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageFileShareResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageFileShareResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageFileShareResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageFileShareResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageFileShareResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageFileShareResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageFileShareResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageFileShareResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageFileShareResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageFileShareResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageFileShareResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStoragePublishFileResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStoragePublishFileResult_t));
            private static System.Action<Steamworks.RemoteStoragePublishFileResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishFileResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishFileResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStoragePublishFileResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStoragePublishFileResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStoragePublishFileResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStoragePublishFileResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStoragePublishFileResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStoragePublishFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishFileResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStoragePublishFileResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStoragePublishFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishFileResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageDeletePublishedFileResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageDeletePublishedFileResult_t));
            private static System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageDeletePublishedFileResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageDeletePublishedFileResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageDeletePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageDeletePublishedFileResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageDeletePublishedFileResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageDeletePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageDeletePublishedFileResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageEnumerateUserPublishedFilesResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t));
            private static System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateUserPublishedFilesResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageSubscribePublishedFileResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageSubscribePublishedFileResult_t));
            private static System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageSubscribePublishedFileResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageSubscribePublishedFileResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageSubscribePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageSubscribePublishedFileResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageSubscribePublishedFileResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageSubscribePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageSubscribePublishedFileResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageEnumerateUserSubscribedFilesResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t));
            private static System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateUserSubscribedFilesResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageUnsubscribePublishedFileResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageUnsubscribePublishedFileResult_t));
            private static System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageUnsubscribePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUnsubscribePublishedFileResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageUnsubscribePublishedFileResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageUnsubscribePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUnsubscribePublishedFileResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageUpdatePublishedFileResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageUpdatePublishedFileResult_t));
            private static System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageUpdatePublishedFileResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageUpdatePublishedFileResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageUpdatePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUpdatePublishedFileResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageUpdatePublishedFileResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageUpdatePublishedFileResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUpdatePublishedFileResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageDownloadUGCResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageDownloadUGCResult_t));
            private static System.Action<Steamworks.RemoteStorageDownloadUGCResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageDownloadUGCResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageDownloadUGCResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageDownloadUGCResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageDownloadUGCResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageDownloadUGCResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageDownloadUGCResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageDownloadUGCResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageDownloadUGCResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageDownloadUGCResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageDownloadUGCResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageDownloadUGCResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageDownloadUGCResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageGetPublishedFileDetailsResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageGetPublishedFileDetailsResult_t));
            private static System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageGetPublishedFileDetailsResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageGetPublishedFileDetailsResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageGetPublishedFileDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageGetPublishedFileDetailsResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageGetPublishedFileDetailsResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageGetPublishedFileDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageGetPublishedFileDetailsResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageEnumerateWorkshopFilesResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t));
            private static System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateWorkshopFilesResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageGetPublishedItemVoteDetailsResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t));
            private static System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageGetPublishedItemVoteDetailsResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStoragePublishedFileSubscribed_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStoragePublishedFileSubscribed_t));
            private static System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStoragePublishedFileSubscribed_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStoragePublishedFileSubscribed_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStoragePublishedFileSubscribed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileSubscribed_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStoragePublishedFileSubscribed_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStoragePublishedFileSubscribed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileSubscribed_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStoragePublishedFileUnsubscribed_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStoragePublishedFileUnsubscribed_t));
            private static System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStoragePublishedFileUnsubscribed_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStoragePublishedFileUnsubscribed_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStoragePublishedFileUnsubscribed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileUnsubscribed_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStoragePublishedFileUnsubscribed_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStoragePublishedFileUnsubscribed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileUnsubscribed_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStoragePublishedFileDeleted_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStoragePublishedFileDeleted_t));
            private static System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStoragePublishedFileDeleted_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStoragePublishedFileDeleted_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStoragePublishedFileDeleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileDeleted_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStoragePublishedFileDeleted_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStoragePublishedFileDeleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileDeleted_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageUpdateUserPublishedItemVoteResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t));
            private static System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUpdateUserPublishedItemVoteResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageUserVoteDetails_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageUserVoteDetails_t));
            private static System.Action<Steamworks.RemoteStorageUserVoteDetails_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageUserVoteDetails_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageUserVoteDetails_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageUserVoteDetails_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageUserVoteDetails_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageUserVoteDetails_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageUserVoteDetails_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageUserVoteDetails_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageUserVoteDetails_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUserVoteDetails_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageUserVoteDetails_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageUserVoteDetails_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageUserVoteDetails_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageEnumerateUserSharedWorkshopFilesResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t));
            private static System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumerateUserSharedWorkshopFilesResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageSetUserPublishedFileActionResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageSetUserPublishedFileActionResult_t));
            private static System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageSetUserPublishedFileActionResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageSetUserPublishedFileActionResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageSetUserPublishedFileActionResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageSetUserPublishedFileActionResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageSetUserPublishedFileActionResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageSetUserPublishedFileActionResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageSetUserPublishedFileActionResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageEnumeratePublishedFilesByUserActionResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t));
            private static System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageEnumeratePublishedFilesByUserActionResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStoragePublishFileProgress_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStoragePublishFileProgress_t));
            private static System.Action<Steamworks.RemoteStoragePublishFileProgress_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishFileProgress_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishFileProgress_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStoragePublishFileProgress_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStoragePublishFileProgress_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStoragePublishFileProgress_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStoragePublishFileProgress_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStoragePublishFileProgress_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStoragePublishFileProgress_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishFileProgress_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStoragePublishFileProgress_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStoragePublishFileProgress_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishFileProgress_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStoragePublishedFileUpdated_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStoragePublishedFileUpdated_t));
            private static System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStoragePublishedFileUpdated_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStoragePublishedFileUpdated_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStoragePublishedFileUpdated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileUpdated_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStoragePublishedFileUpdated_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStoragePublishedFileUpdated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStoragePublishedFileUpdated_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageFileWriteAsyncComplete_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageFileWriteAsyncComplete_t));
            private static System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageFileWriteAsyncComplete_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageFileWriteAsyncComplete_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageFileWriteAsyncComplete_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageFileWriteAsyncComplete_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageFileWriteAsyncComplete_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageFileWriteAsyncComplete_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageFileWriteAsyncComplete_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class RemoteStorageFileReadAsyncComplete_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.RemoteStorageFileReadAsyncComplete_t));
            private static System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.RemoteStorageFileReadAsyncComplete_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.RemoteStorageFileReadAsyncComplete_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.RemoteStorageFileReadAsyncComplete_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageFileReadAsyncComplete_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.RemoteStorageFileReadAsyncComplete_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.RemoteStorageFileReadAsyncComplete_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.RemoteStorageFileReadAsyncComplete_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ScreenshotReady_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ScreenshotReady_t));
            private static System.Action<Steamworks.ScreenshotReady_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ScreenshotReady_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ScreenshotReady_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ScreenshotReady_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ScreenshotReady_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ScreenshotReady_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ScreenshotReady_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ScreenshotReady_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ScreenshotReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ScreenshotReady_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ScreenshotReady_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ScreenshotReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ScreenshotReady_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ScreenshotRequested_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ScreenshotRequested_t));
            private static System.Action<Steamworks.ScreenshotRequested_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ScreenshotRequested_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ScreenshotRequested_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ScreenshotRequested_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ScreenshotRequested_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ScreenshotRequested_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ScreenshotRequested_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ScreenshotRequested_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ScreenshotRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ScreenshotRequested_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ScreenshotRequested_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ScreenshotRequested_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ScreenshotRequested_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamUGCQueryCompleted_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamUGCQueryCompleted_t));
            private static System.Action<Steamworks.SteamUGCQueryCompleted_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamUGCQueryCompleted_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamUGCQueryCompleted_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamUGCQueryCompleted_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamUGCQueryCompleted_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamUGCQueryCompleted_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamUGCQueryCompleted_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamUGCQueryCompleted_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamUGCQueryCompleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamUGCQueryCompleted_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamUGCQueryCompleted_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamUGCQueryCompleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamUGCQueryCompleted_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamUGCRequestUGCDetailsResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamUGCRequestUGCDetailsResult_t));
            private static System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamUGCRequestUGCDetailsResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamUGCRequestUGCDetailsResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamUGCRequestUGCDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamUGCRequestUGCDetailsResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamUGCRequestUGCDetailsResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamUGCRequestUGCDetailsResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamUGCRequestUGCDetailsResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class CreateItemResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.CreateItemResult_t));
            private static System.Action<Steamworks.CreateItemResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.CreateItemResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.CreateItemResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.CreateItemResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.CreateItemResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.CreateItemResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.CreateItemResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.CreateItemResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.CreateItemResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.CreateItemResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.CreateItemResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.CreateItemResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.CreateItemResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SubmitItemUpdateResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SubmitItemUpdateResult_t));
            private static System.Action<Steamworks.SubmitItemUpdateResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SubmitItemUpdateResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SubmitItemUpdateResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SubmitItemUpdateResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SubmitItemUpdateResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SubmitItemUpdateResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SubmitItemUpdateResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SubmitItemUpdateResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SubmitItemUpdateResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SubmitItemUpdateResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SubmitItemUpdateResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SubmitItemUpdateResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SubmitItemUpdateResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ItemInstalled_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ItemInstalled_t));
            private static System.Action<Steamworks.ItemInstalled_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ItemInstalled_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ItemInstalled_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ItemInstalled_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ItemInstalled_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ItemInstalled_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ItemInstalled_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ItemInstalled_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ItemInstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ItemInstalled_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ItemInstalled_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ItemInstalled_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ItemInstalled_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class DownloadItemResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.DownloadItemResult_t));
            private static System.Action<Steamworks.DownloadItemResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.DownloadItemResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.DownloadItemResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.DownloadItemResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.DownloadItemResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.DownloadItemResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.DownloadItemResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.DownloadItemResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.DownloadItemResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.DownloadItemResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.DownloadItemResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.DownloadItemResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.DownloadItemResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class UserFavoriteItemsListChanged_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.UserFavoriteItemsListChanged_t));
            private static System.Action<Steamworks.UserFavoriteItemsListChanged_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.UserFavoriteItemsListChanged_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.UserFavoriteItemsListChanged_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.UserFavoriteItemsListChanged_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.UserFavoriteItemsListChanged_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.UserFavoriteItemsListChanged_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.UserFavoriteItemsListChanged_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.UserFavoriteItemsListChanged_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.UserFavoriteItemsListChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserFavoriteItemsListChanged_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.UserFavoriteItemsListChanged_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.UserFavoriteItemsListChanged_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserFavoriteItemsListChanged_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SetUserItemVoteResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SetUserItemVoteResult_t));
            private static System.Action<Steamworks.SetUserItemVoteResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SetUserItemVoteResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SetUserItemVoteResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SetUserItemVoteResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SetUserItemVoteResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SetUserItemVoteResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SetUserItemVoteResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SetUserItemVoteResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SetUserItemVoteResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SetUserItemVoteResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SetUserItemVoteResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SetUserItemVoteResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SetUserItemVoteResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GetUserItemVoteResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GetUserItemVoteResult_t));
            private static System.Action<Steamworks.GetUserItemVoteResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GetUserItemVoteResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GetUserItemVoteResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GetUserItemVoteResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GetUserItemVoteResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GetUserItemVoteResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GetUserItemVoteResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GetUserItemVoteResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GetUserItemVoteResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GetUserItemVoteResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GetUserItemVoteResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GetUserItemVoteResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GetUserItemVoteResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class StartPlaytimeTrackingResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.StartPlaytimeTrackingResult_t));
            private static System.Action<Steamworks.StartPlaytimeTrackingResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.StartPlaytimeTrackingResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.StartPlaytimeTrackingResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.StartPlaytimeTrackingResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.StartPlaytimeTrackingResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.StartPlaytimeTrackingResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.StartPlaytimeTrackingResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.StartPlaytimeTrackingResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.StartPlaytimeTrackingResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.StartPlaytimeTrackingResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.StartPlaytimeTrackingResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.StartPlaytimeTrackingResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.StartPlaytimeTrackingResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class StopPlaytimeTrackingResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.StopPlaytimeTrackingResult_t));
            private static System.Action<Steamworks.StopPlaytimeTrackingResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.StopPlaytimeTrackingResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.StopPlaytimeTrackingResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.StopPlaytimeTrackingResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.StopPlaytimeTrackingResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.StopPlaytimeTrackingResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.StopPlaytimeTrackingResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.StopPlaytimeTrackingResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.StopPlaytimeTrackingResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.StopPlaytimeTrackingResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.StopPlaytimeTrackingResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.StopPlaytimeTrackingResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.StopPlaytimeTrackingResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamServersConnected_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamServersConnected_t));
            private static System.Action<Steamworks.SteamServersConnected_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamServersConnected_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamServersConnected_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamServersConnected_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamServersConnected_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamServersConnected_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamServersConnected_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamServersConnected_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamServersConnected_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamServersConnected_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamServersConnected_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamServersConnected_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamServersConnected_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamServerConnectFailure_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamServerConnectFailure_t));
            private static System.Action<Steamworks.SteamServerConnectFailure_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamServerConnectFailure_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamServerConnectFailure_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamServerConnectFailure_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamServerConnectFailure_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamServerConnectFailure_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamServerConnectFailure_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamServerConnectFailure_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamServerConnectFailure_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamServerConnectFailure_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamServerConnectFailure_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamServerConnectFailure_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamServerConnectFailure_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamServersDisconnected_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamServersDisconnected_t));
            private static System.Action<Steamworks.SteamServersDisconnected_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamServersDisconnected_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamServersDisconnected_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamServersDisconnected_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamServersDisconnected_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamServersDisconnected_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamServersDisconnected_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamServersDisconnected_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamServersDisconnected_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamServersDisconnected_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamServersDisconnected_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamServersDisconnected_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamServersDisconnected_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ClientGameServerDeny_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ClientGameServerDeny_t));
            private static System.Action<Steamworks.ClientGameServerDeny_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ClientGameServerDeny_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ClientGameServerDeny_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ClientGameServerDeny_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ClientGameServerDeny_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ClientGameServerDeny_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ClientGameServerDeny_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ClientGameServerDeny_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ClientGameServerDeny_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ClientGameServerDeny_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ClientGameServerDeny_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ClientGameServerDeny_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ClientGameServerDeny_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class IPCFailure_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.IPCFailure_t));
            private static System.Action<Steamworks.IPCFailure_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.IPCFailure_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.IPCFailure_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.IPCFailure_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.IPCFailure_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.IPCFailure_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.IPCFailure_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.IPCFailure_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.IPCFailure_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.IPCFailure_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.IPCFailure_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.IPCFailure_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.IPCFailure_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LicensesUpdated_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LicensesUpdated_t));
            private static System.Action<Steamworks.LicensesUpdated_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LicensesUpdated_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LicensesUpdated_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LicensesUpdated_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LicensesUpdated_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LicensesUpdated_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LicensesUpdated_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LicensesUpdated_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LicensesUpdated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LicensesUpdated_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LicensesUpdated_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LicensesUpdated_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LicensesUpdated_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class ValidateAuthTicketResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.ValidateAuthTicketResponse_t));
            private static System.Action<Steamworks.ValidateAuthTicketResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.ValidateAuthTicketResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.ValidateAuthTicketResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.ValidateAuthTicketResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.ValidateAuthTicketResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.ValidateAuthTicketResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.ValidateAuthTicketResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.ValidateAuthTicketResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.ValidateAuthTicketResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ValidateAuthTicketResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.ValidateAuthTicketResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.ValidateAuthTicketResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.ValidateAuthTicketResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class MicroTxnAuthorizationResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.MicroTxnAuthorizationResponse_t));
            private static System.Action<Steamworks.MicroTxnAuthorizationResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.MicroTxnAuthorizationResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.MicroTxnAuthorizationResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.MicroTxnAuthorizationResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.MicroTxnAuthorizationResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.MicroTxnAuthorizationResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.MicroTxnAuthorizationResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.MicroTxnAuthorizationResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.MicroTxnAuthorizationResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MicroTxnAuthorizationResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.MicroTxnAuthorizationResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.MicroTxnAuthorizationResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.MicroTxnAuthorizationResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class EncryptedAppTicketResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.EncryptedAppTicketResponse_t));
            private static System.Action<Steamworks.EncryptedAppTicketResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.EncryptedAppTicketResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.EncryptedAppTicketResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.EncryptedAppTicketResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.EncryptedAppTicketResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.EncryptedAppTicketResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.EncryptedAppTicketResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.EncryptedAppTicketResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.EncryptedAppTicketResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.EncryptedAppTicketResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.EncryptedAppTicketResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.EncryptedAppTicketResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.EncryptedAppTicketResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GetAuthSessionTicketResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GetAuthSessionTicketResponse_t));
            private static System.Action<Steamworks.GetAuthSessionTicketResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GetAuthSessionTicketResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GetAuthSessionTicketResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GetAuthSessionTicketResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GetAuthSessionTicketResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GetAuthSessionTicketResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GetAuthSessionTicketResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GetAuthSessionTicketResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GetAuthSessionTicketResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GetAuthSessionTicketResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GetAuthSessionTicketResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GetAuthSessionTicketResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GetAuthSessionTicketResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GameWebCallback_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GameWebCallback_t));
            private static System.Action<Steamworks.GameWebCallback_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GameWebCallback_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GameWebCallback_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GameWebCallback_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GameWebCallback_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GameWebCallback_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GameWebCallback_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GameWebCallback_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GameWebCallback_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameWebCallback_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GameWebCallback_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GameWebCallback_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GameWebCallback_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class StoreAuthURLResponse_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.StoreAuthURLResponse_t));
            private static System.Action<Steamworks.StoreAuthURLResponse_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.StoreAuthURLResponse_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.StoreAuthURLResponse_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.StoreAuthURLResponse_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.StoreAuthURLResponse_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.StoreAuthURLResponse_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.StoreAuthURLResponse_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.StoreAuthURLResponse_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.StoreAuthURLResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.StoreAuthURLResponse_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.StoreAuthURLResponse_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.StoreAuthURLResponse_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.StoreAuthURLResponse_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class UserStatsReceived_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.UserStatsReceived_t));
            private static System.Action<Steamworks.UserStatsReceived_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.UserStatsReceived_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.UserStatsReceived_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.UserStatsReceived_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.UserStatsReceived_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.UserStatsReceived_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.UserStatsReceived_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.UserStatsReceived_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.UserStatsReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserStatsReceived_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.UserStatsReceived_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.UserStatsReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserStatsReceived_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class UserStatsStored_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.UserStatsStored_t));
            private static System.Action<Steamworks.UserStatsStored_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.UserStatsStored_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.UserStatsStored_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.UserStatsStored_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.UserStatsStored_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.UserStatsStored_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.UserStatsStored_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.UserStatsStored_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.UserStatsStored_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserStatsStored_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.UserStatsStored_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.UserStatsStored_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserStatsStored_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class UserAchievementStored_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.UserAchievementStored_t));
            private static System.Action<Steamworks.UserAchievementStored_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.UserAchievementStored_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.UserAchievementStored_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.UserAchievementStored_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.UserAchievementStored_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.UserAchievementStored_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.UserAchievementStored_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.UserAchievementStored_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.UserAchievementStored_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserAchievementStored_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.UserAchievementStored_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.UserAchievementStored_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserAchievementStored_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LeaderboardFindResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LeaderboardFindResult_t));
            private static System.Action<Steamworks.LeaderboardFindResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LeaderboardFindResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LeaderboardFindResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LeaderboardFindResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LeaderboardFindResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LeaderboardFindResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LeaderboardFindResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LeaderboardFindResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LeaderboardFindResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardFindResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LeaderboardFindResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LeaderboardFindResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardFindResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LeaderboardScoresDownloaded_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LeaderboardScoresDownloaded_t));
            private static System.Action<Steamworks.LeaderboardScoresDownloaded_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LeaderboardScoresDownloaded_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LeaderboardScoresDownloaded_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LeaderboardScoresDownloaded_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LeaderboardScoresDownloaded_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LeaderboardScoresDownloaded_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LeaderboardScoresDownloaded_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LeaderboardScoresDownloaded_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LeaderboardScoresDownloaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardScoresDownloaded_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LeaderboardScoresDownloaded_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LeaderboardScoresDownloaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardScoresDownloaded_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LeaderboardScoreUploaded_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LeaderboardScoreUploaded_t));
            private static System.Action<Steamworks.LeaderboardScoreUploaded_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LeaderboardScoreUploaded_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LeaderboardScoreUploaded_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LeaderboardScoreUploaded_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LeaderboardScoreUploaded_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LeaderboardScoreUploaded_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LeaderboardScoreUploaded_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LeaderboardScoreUploaded_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LeaderboardScoreUploaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardScoreUploaded_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LeaderboardScoreUploaded_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LeaderboardScoreUploaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardScoreUploaded_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class NumberOfCurrentPlayers_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.NumberOfCurrentPlayers_t));
            private static System.Action<Steamworks.NumberOfCurrentPlayers_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.NumberOfCurrentPlayers_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.NumberOfCurrentPlayers_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.NumberOfCurrentPlayers_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.NumberOfCurrentPlayers_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.NumberOfCurrentPlayers_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.NumberOfCurrentPlayers_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.NumberOfCurrentPlayers_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.NumberOfCurrentPlayers_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.NumberOfCurrentPlayers_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.NumberOfCurrentPlayers_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.NumberOfCurrentPlayers_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.NumberOfCurrentPlayers_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class UserStatsUnloaded_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.UserStatsUnloaded_t));
            private static System.Action<Steamworks.UserStatsUnloaded_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.UserStatsUnloaded_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.UserStatsUnloaded_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.UserStatsUnloaded_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.UserStatsUnloaded_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.UserStatsUnloaded_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.UserStatsUnloaded_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.UserStatsUnloaded_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.UserStatsUnloaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserStatsUnloaded_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.UserStatsUnloaded_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.UserStatsUnloaded_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserStatsUnloaded_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class UserAchievementIconFetched_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.UserAchievementIconFetched_t));
            private static System.Action<Steamworks.UserAchievementIconFetched_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.UserAchievementIconFetched_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.UserAchievementIconFetched_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.UserAchievementIconFetched_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.UserAchievementIconFetched_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.UserAchievementIconFetched_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.UserAchievementIconFetched_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.UserAchievementIconFetched_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.UserAchievementIconFetched_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserAchievementIconFetched_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.UserAchievementIconFetched_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.UserAchievementIconFetched_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.UserAchievementIconFetched_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GlobalAchievementPercentagesReady_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GlobalAchievementPercentagesReady_t));
            private static System.Action<Steamworks.GlobalAchievementPercentagesReady_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GlobalAchievementPercentagesReady_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GlobalAchievementPercentagesReady_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GlobalAchievementPercentagesReady_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GlobalAchievementPercentagesReady_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GlobalAchievementPercentagesReady_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GlobalAchievementPercentagesReady_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GlobalAchievementPercentagesReady_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GlobalAchievementPercentagesReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GlobalAchievementPercentagesReady_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GlobalAchievementPercentagesReady_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GlobalAchievementPercentagesReady_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GlobalAchievementPercentagesReady_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LeaderboardUGCSet_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LeaderboardUGCSet_t));
            private static System.Action<Steamworks.LeaderboardUGCSet_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LeaderboardUGCSet_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LeaderboardUGCSet_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LeaderboardUGCSet_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LeaderboardUGCSet_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LeaderboardUGCSet_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LeaderboardUGCSet_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LeaderboardUGCSet_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LeaderboardUGCSet_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardUGCSet_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LeaderboardUGCSet_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LeaderboardUGCSet_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LeaderboardUGCSet_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GlobalStatsReceived_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GlobalStatsReceived_t));
            private static System.Action<Steamworks.GlobalStatsReceived_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GlobalStatsReceived_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GlobalStatsReceived_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GlobalStatsReceived_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GlobalStatsReceived_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GlobalStatsReceived_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GlobalStatsReceived_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GlobalStatsReceived_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GlobalStatsReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GlobalStatsReceived_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GlobalStatsReceived_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GlobalStatsReceived_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GlobalStatsReceived_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class IPCountry_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.IPCountry_t));
            private static System.Action<Steamworks.IPCountry_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.IPCountry_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.IPCountry_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.IPCountry_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.IPCountry_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.IPCountry_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.IPCountry_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.IPCountry_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.IPCountry_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.IPCountry_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.IPCountry_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.IPCountry_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.IPCountry_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class LowBatteryPower_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.LowBatteryPower_t));
            private static System.Action<Steamworks.LowBatteryPower_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.LowBatteryPower_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.LowBatteryPower_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.LowBatteryPower_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.LowBatteryPower_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.LowBatteryPower_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.LowBatteryPower_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.LowBatteryPower_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.LowBatteryPower_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LowBatteryPower_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.LowBatteryPower_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.LowBatteryPower_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.LowBatteryPower_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamAPICallCompleted_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamAPICallCompleted_t));
            private static System.Action<Steamworks.SteamAPICallCompleted_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamAPICallCompleted_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamAPICallCompleted_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamAPICallCompleted_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamAPICallCompleted_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamAPICallCompleted_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamAPICallCompleted_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamAPICallCompleted_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamAPICallCompleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamAPICallCompleted_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamAPICallCompleted_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamAPICallCompleted_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamAPICallCompleted_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class SteamShutdown_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.SteamShutdown_t));
            private static System.Action<Steamworks.SteamShutdown_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.SteamShutdown_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.SteamShutdown_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.SteamShutdown_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.SteamShutdown_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.SteamShutdown_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.SteamShutdown_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.SteamShutdown_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.SteamShutdown_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamShutdown_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.SteamShutdown_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.SteamShutdown_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.SteamShutdown_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class CheckFileSignature_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.CheckFileSignature_t));
            private static System.Action<Steamworks.CheckFileSignature_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.CheckFileSignature_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.CheckFileSignature_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.CheckFileSignature_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.CheckFileSignature_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.CheckFileSignature_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.CheckFileSignature_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.CheckFileSignature_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.CheckFileSignature_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.CheckFileSignature_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.CheckFileSignature_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.CheckFileSignature_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.CheckFileSignature_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GamepadTextInputDismissed_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GamepadTextInputDismissed_t));
            private static System.Action<Steamworks.GamepadTextInputDismissed_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GamepadTextInputDismissed_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GamepadTextInputDismissed_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GamepadTextInputDismissed_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GamepadTextInputDismissed_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GamepadTextInputDismissed_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GamepadTextInputDismissed_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GamepadTextInputDismissed_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GamepadTextInputDismissed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GamepadTextInputDismissed_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GamepadTextInputDismissed_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GamepadTextInputDismissed_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GamepadTextInputDismissed_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class BroadcastUploadStart_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.BroadcastUploadStart_t));
            private static System.Action<Steamworks.BroadcastUploadStart_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.BroadcastUploadStart_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.BroadcastUploadStart_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.BroadcastUploadStart_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.BroadcastUploadStart_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.BroadcastUploadStart_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.BroadcastUploadStart_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.BroadcastUploadStart_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.BroadcastUploadStart_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.BroadcastUploadStart_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.BroadcastUploadStart_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.BroadcastUploadStart_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.BroadcastUploadStart_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class BroadcastUploadStop_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.BroadcastUploadStop_t));
            private static System.Action<Steamworks.BroadcastUploadStop_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.BroadcastUploadStop_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.BroadcastUploadStop_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.BroadcastUploadStop_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.BroadcastUploadStop_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.BroadcastUploadStop_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.BroadcastUploadStop_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.BroadcastUploadStop_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.BroadcastUploadStop_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.BroadcastUploadStop_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.BroadcastUploadStop_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.BroadcastUploadStop_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.BroadcastUploadStop_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }


        public static class GetVideoURLResult_t {
            private static readonly int _size = Marshal.SizeOf(typeof(Steamworks.GetVideoURLResult_t));
            private static System.Action<Steamworks.GetVideoURLResult_t> _callbackAction;
            private static readonly Dictionary<ulong, System.Action<Steamworks.GetVideoURLResult_t, bool>> _callResultActions = new Dictionary<ulong, System.Action<Steamworks.GetVideoURLResult_t, bool>>();
            private static IntPtr _vtablePtr;
            private static CCallbackBase _callbackBase;
            private static GCHandle _callbackBasePtr;

            private static int CallbackCount {
                get { return _callbackAction != null ? _callbackAction.GetInvocationList().Length : 0; }
            }

            public static void RegisterCallback(System.Action<Steamworks.GetVideoURLResult_t> action) {
                BuildCallbackData();

                if (CallbackCount == 0) {
                    NativeMethods.SteamAPI_RegisterCallback(_callbackBasePtr.AddrOfPinnedObject(), Steamworks.GetVideoURLResult_t.k_iCallback);
                }

                _callbackAction += action;
            }

            public static void UnregisterCallback(System.Action<Steamworks.GetVideoURLResult_t> action) {
                if (CallbackCount == 0)
                    throw new System.Exception("callback is already unregistered");

                _callbackAction -= action;

                if (CallbackCount == 0) {
                    if ((_callbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) != CCallbackBase.k_ECallbackFlagsRegistered)
                        throw new System.Exception("callback is not registered");

                    NativeMethods.SteamAPI_UnregisterCallback(_callbackBasePtr.AddrOfPinnedObject());
                }
            }

            public static void RegisterCallResult(System.Action<Steamworks.GetVideoURLResult_t, bool> action, SteamAPICall_t hApiCall) {
                BuildCallbackData();

                NativeMethods.SteamAPI_RegisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);

                _callResultActions.Add(hApiCall.m_SteamAPICall, action);
            }

            public static void UnregisterCallResult(SteamAPICall_t hApiCall) {
                if (_callResultActions.Remove(hApiCall.m_SteamAPICall)) {
                    NativeMethods.SteamAPI_UnregisterCallResult(_callbackBasePtr.AddrOfPinnedObject(), hApiCall.m_SteamAPICall);
                }
            }

            private static void BuildCallbackData() {
                if (_vtablePtr == IntPtr.Zero) {
                    if (CallbackCount > 0)
                        throw new System.Exception("trying to build callback while callbacks are registered");

                    _vtablePtr = SteamCallbackBase.BuildVTable(OnRunCallback, OnRunCallResult, OnGetCallbackSizeBytes);
                    SteamCallbackBase.BuildCCallbackBase(_vtablePtr, Steamworks.GetVideoURLResult_t.k_iCallback, out _callbackBase, out _callbackBasePtr);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallback(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam) {
                try {
                    var result = (Steamworks.GetVideoURLResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GetVideoURLResult_t));
                    _callbackAction(result);
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static void OnRunCallResult(
#if !STDCALL
                IntPtr thisptr,
#endif
                IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
                try {
                    System.Action<Steamworks.GetVideoURLResult_t, bool> action;
                    if (_callResultActions.TryGetValue(hSteamAPICall, out action)) {
                        var result = (Steamworks.GetVideoURLResult_t)Marshal.PtrToStructure(pvParam, typeof(Steamworks.GetVideoURLResult_t));
                        action(result, bFailed);
                        _callResultActions.Remove(hSteamAPICall);
                    }
                } catch (Exception e) {
                    CallbackDispatcher.ExceptionHandler(e);
                }
            }

            [MonoPInvokeCallback]
            private static int OnGetCallbackSizeBytes(
#if !STDCALL
                IntPtr thisptr
#endif
            ) {
                return _size;
            }
        }



    }
}

#endif