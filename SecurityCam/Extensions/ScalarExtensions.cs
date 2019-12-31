using System;
using OpenCvSharp;

namespace SecurityCam
{
    public static class ScalarExtensions
    {
        public static double Diff(this Scalar oldDev, Scalar newDev)
        {
            return Math.Abs(oldDev.Val0 - newDev.Val0)
                   + Math.Abs(oldDev.Val1 - newDev.Val1)
                   + Math.Abs(oldDev.Val2 - newDev.Val2);
        }
    }
}