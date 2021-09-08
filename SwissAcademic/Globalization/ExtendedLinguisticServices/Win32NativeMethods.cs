// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAPICodePack.ExtendedLinguisticServices
{

    internal static class Win32NativeMethods
    {
        [DllImport("elscore.dll", EntryPoint = "MappingGetServices", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern uint MappingGetServices(ref Win32EnumOptions enumOptions, ref IntPtr services, ref uint servicesCount);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden

        // For performance reasons, if we need to pass NULL as the MappingEnumOptions
        [DllImport("elscore.dll", EntryPoint = "MappingGetServices", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern uint MappingGetServices(IntPtr enumOptions, ref IntPtr services, ref uint servicesCount);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden

        [DllImport("elscore.dll", EntryPoint = "MappingRecognizeText", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern uint MappingRecognizeText(IntPtr service, IntPtr text, uint length, uint index, IntPtr options, ref Win32PropertyBag bag);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden

        [DllImport("elscore.dll", EntryPoint = "MappingDoAction", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern uint MappingDoAction(ref Win32PropertyBag bag, uint rangeIndex, string action);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden

        [DllImport("elscore.dll", EntryPoint = "MappingFreePropertyBag", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern uint MappingFreePropertyBag(ref Win32PropertyBag bag);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden

        [DllImport("elscore.dll", EntryPoint = "MappingFreeServices", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern uint MappingFreeServices(IntPtr pServiceInfo);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden

        [DllImport("elscore.dll", EntryPoint = "MappingFreeServices", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
        internal static extern void MappingFreeServicesVoid(IntPtr pServiceInfo);
#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
    }

}
