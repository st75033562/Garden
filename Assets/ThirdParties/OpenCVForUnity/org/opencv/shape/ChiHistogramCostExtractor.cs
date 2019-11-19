﻿
//
// This file is auto-generated. Please don't modify it!
//
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace OpenCVForUnity
{

// C++: class ChiHistogramCostExtractor
//javadoc: ChiHistogramCostExtractor
    public class ChiHistogramCostExtractor : HistogramCostExtractor
    {
        protected override void Dispose (bool disposing)
        {
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5
try {
if (disposing) {
}
if (IsEnabledDispose) {
if (nativeObj != IntPtr.Zero)
shape_ChiHistogramCostExtractor_delete(nativeObj);
nativeObj = IntPtr.Zero;
}
} finally {
base.Dispose (disposing);
}
#else
            return;
#endif
        }

        protected ChiHistogramCostExtractor (IntPtr addr) : base(addr)
        {
        }


    
        #if UNITY_IOS && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
        #else
        const string LIBNAME = "opencvforunity";
        #endif


        // native support for java finalize()
        [DllImport(LIBNAME)]
        private static extern void shape_ChiHistogramCostExtractor_delete (IntPtr nativeObj);

    }
}
