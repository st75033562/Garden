﻿
//
// This file is auto-generated. Please don't modify it!
//
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace OpenCVForUnity
{

// C++: class SuperpixelSLIC
//javadoc: SuperpixelSLIC
    public class SuperpixelSLIC : Algorithm
    {
        protected override void Dispose (bool disposing)
        {
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5
try {
if (disposing) {
}
if (IsEnabledDispose) {
if (nativeObj != IntPtr.Zero)
ximgproc_SuperpixelSLIC_delete(nativeObj);
nativeObj = IntPtr.Zero;
}
} finally {
base.Dispose (disposing);
}
#else
            return;
#endif
        }

        public SuperpixelSLIC (IntPtr addr) : base(addr)
        {
        }


        //
        // C++:  int getNumberOfSuperpixels()
        //

        //javadoc: SuperpixelSLIC::getNumberOfSuperpixels()
        public  int getNumberOfSuperpixels ()
        {
            ThrowIfDisposed ();
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        int retVal = ximgproc_SuperpixelSLIC_getNumberOfSuperpixels_10(nativeObj);
        
        return retVal;
#else
            return -1;
#endif
        }


        //
        // C++:  void enforceLabelConnectivity(int min_element_size = 25)
        //

        //javadoc: SuperpixelSLIC::enforceLabelConnectivity(min_element_size)
        public  void enforceLabelConnectivity (int min_element_size)
        {
            ThrowIfDisposed ();
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_enforceLabelConnectivity_10(nativeObj, min_element_size);
        
        return;
#else
            return;
#endif
        }

        //javadoc: SuperpixelSLIC::enforceLabelConnectivity()
        public  void enforceLabelConnectivity ()
        {
            ThrowIfDisposed ();
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_enforceLabelConnectivity_11(nativeObj);
        
        return;
#else
            return;
#endif
        }


        //
        // C++:  void getLabelContourMask(Mat& image, bool thick_line = true)
        //

        //javadoc: SuperpixelSLIC::getLabelContourMask(image, thick_line)
        public  void getLabelContourMask (Mat image, bool thick_line)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();

#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_getLabelContourMask_10(nativeObj, image.nativeObj, thick_line);
        
        return;
#else
            return;
#endif
        }

        //javadoc: SuperpixelSLIC::getLabelContourMask(image)
        public  void getLabelContourMask (Mat image)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();

#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_getLabelContourMask_11(nativeObj, image.nativeObj);
        
        return;
#else
            return;
#endif
        }


        //
        // C++:  void getLabels(Mat& labels_out)
        //

        //javadoc: SuperpixelSLIC::getLabels(labels_out)
        public  void getLabels (Mat labels_out)
        {
            ThrowIfDisposed ();
            if (labels_out != null)
                labels_out.ThrowIfDisposed ();

#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_getLabels_10(nativeObj, labels_out.nativeObj);
        
        return;
#else
            return;
#endif
        }


        //
        // C++:  void iterate(int num_iterations = 10)
        //

        //javadoc: SuperpixelSLIC::iterate(num_iterations)
        public  void iterate (int num_iterations)
        {
            ThrowIfDisposed ();
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_iterate_10(nativeObj, num_iterations);
        
        return;
#else
            return;
#endif
        }

        //javadoc: SuperpixelSLIC::iterate()
        public  void iterate ()
        {
            ThrowIfDisposed ();
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR) || UNITY_5

        
        ximgproc_SuperpixelSLIC_iterate_11(nativeObj);
        
        return;
#else
            return;
#endif
        }


    
        #if UNITY_IOS && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
        #else
        const string LIBNAME = "opencvforunity";
        #endif


        // C++:  int getNumberOfSuperpixels()
        [DllImport(LIBNAME)]
        private static extern int ximgproc_SuperpixelSLIC_getNumberOfSuperpixels_10 (IntPtr nativeObj);

        // C++:  void enforceLabelConnectivity(int min_element_size = 25)
        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_enforceLabelConnectivity_10 (IntPtr nativeObj, int min_element_size);

        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_enforceLabelConnectivity_11 (IntPtr nativeObj);

        // C++:  void getLabelContourMask(Mat& image, bool thick_line = true)
        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_getLabelContourMask_10 (IntPtr nativeObj, IntPtr image_nativeObj, bool thick_line);

        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_getLabelContourMask_11 (IntPtr nativeObj, IntPtr image_nativeObj);

        // C++:  void getLabels(Mat& labels_out)
        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_getLabels_10 (IntPtr nativeObj, IntPtr labels_out_nativeObj);

        // C++:  void iterate(int num_iterations = 10)
        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_iterate_10 (IntPtr nativeObj, int num_iterations);

        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_iterate_11 (IntPtr nativeObj);

        // native support for java finalize()
        [DllImport(LIBNAME)]
        private static extern void ximgproc_SuperpixelSLIC_delete (IntPtr nativeObj);

    }
}
