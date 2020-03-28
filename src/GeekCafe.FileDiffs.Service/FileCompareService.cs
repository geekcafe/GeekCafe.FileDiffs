using System;
using System.IO;

namespace GeekCafe.FileDiffs.Service
{
    public class FileCompareService
    {
        
        public static bool IsEqual(string pathLeft, string pathRight)
        {
            byte[] fileLeft = File.ReadAllBytes(pathLeft);
            byte[] fileRight = File.ReadAllBytes(pathRight);
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
