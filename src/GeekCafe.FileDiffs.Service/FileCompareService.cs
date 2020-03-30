using System;
using System.IO;

namespace GeekCafe.FileDiffs.Service
{
    public class FileCompareService
    {
        
        public static bool IsEqual(string pathLeft, string pathRight)
        {
            byte[] fileLeft = (pathLeft.Length > 0 &&  File.Exists(pathLeft)) ? File.ReadAllBytes(pathLeft) : null;
            byte[] fileRight = (pathRight.Length > 0 &&  File.Exists(pathRight)) ? File.ReadAllBytes(pathRight) : null;

            if (fileLeft == null && fileRight == null) return true;
            if (fileLeft == null || fileRight == null) return false;

            if (fileLeft.Length == fileRight.Length)
            {
                for (int i = 0; i < fileLeft.Length; i++)
                {
                    if (fileLeft[i] != fileRight[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
