using System;
namespace GeekCafe.FileDiffs.Service
{
    public class DirectoryCompareService
    {
        public DirectoryCompareService()
        {
        }

        public static bool IsEqual(string pathLeft, string pathRight)
        {
            var dirsLeft = System.IO.Directory.GetDirectories(pathLeft);
            var dirsRight = System.IO.Directory.GetDirectories(pathRight);

            if (dirsLeft.Length != dirsRight.Length)
            {
                return false;
            }

            return false;
        }
}
