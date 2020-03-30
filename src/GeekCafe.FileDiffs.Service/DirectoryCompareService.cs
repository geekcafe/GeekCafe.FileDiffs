using System;
using System.IO;
using System.Collections.Generic;
using GeekCafe.FileDiffs.Service.Model;

namespace GeekCafe.FileDiffs.Service
{
    public class DirectoryCompareService: FileCollection
    {
        private enum Direction
        {
            Left,
            Right
        }

        private string _leftPath = "";
        private string _rightPath = "";
        

        public DirectoryCompareService()
        {
        }


        


        public void Compare(string dirLeft, string dirRight)
        {

            _leftPath = dirLeft;
            _rightPath = dirRight;

            Files = new Dictionary<string, FileCompareModel>();

            Build(dirLeft, Direction.Left);
            Build(dirRight, Direction.Right);
            Conpare();

            
           
        }

        private string GetKey(string file)
        {
            file = file.Replace(_leftPath, "");
            file = file.Replace(_rightPath, "");

            return file;
        }

        private void Build(string path, Direction direction)
        {
            path = Path.GetFullPath(path);

            foreach (var file in Directory.GetFiles(path))
            {
                var key = GetKey(file);
                Files.TryGetValue(key, out var model);
                

                Console.WriteLine($"Checking Path {file}");
                if(model == null)
                {
                    model = new FileCompareModel();
                    Files.Add(key, model);
                }
                

                if (direction == Direction.Left) { model.LeftPath = file; } else { model.RightPath = file; }

                Files[key] = model;
            }


            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                Build(dir, direction);
            }


            

        }

        private void Conpare()
        {
            foreach (var model in Files)
            {

                model.Value.IsEqual = (FileCompareService.IsEqual(model.Value.LeftPath, model.Value.RightPath));

                Console.WriteLine("Comparing Files");
                Console.WriteLine($"\tLeft: {model.Value.LeftPath}");
                Console.WriteLine($"\tRight: {model.Value.RightPath}");
                Console.WriteLine($"\tIs Equal: {model.Value.IsEqual}");
                Console.WriteLine("");
            }

        }

        public bool IsDifferent()
        {
            foreach (var model in Files)
            {
                if (!model.Value.IsEqual) return true;
            }

            return false;
        }
    }
}
