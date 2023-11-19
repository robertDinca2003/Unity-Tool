using YamlDotNet.RepresentationModel;

namespace unityFileReader;





class UnityFileReader
{
    private static readonly List<string> FileList = new List<string>();
    private static string? _unityProjectPath ="";
    private static string? _outputFolderPath ="";
    
    static void Main(string?[] args)
    {
        
        if (args.Length >= 2)
        {
            _unityProjectPath = args[0];
            _outputFolderPath = args[1];
            
            
            Console.WriteLine($"Unity Project Path: {_unityProjectPath}");
            Console.WriteLine($"Output Folder Path: {_outputFolderPath}");

            WorkingOnUnityProject(_unityProjectPath,_outputFolderPath);
        }
        else
        {
            Console.WriteLine("Error: Not enough arguments found!\n Usage: ./tool.exe unity_project_path output_folder_path");   
            Console.WriteLine("Input manually:");
            _unityProjectPath = Console.ReadLine();
            _outputFolderPath = Console.ReadLine();
            
            WorkingOnUnityProject(_unityProjectPath,_outputFolderPath);
        }
    }

    static void GetAllFiles(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(@path);
        
        FileInfo[] files = dirInfo.GetFiles();
        foreach (FileInfo f in files)
        {
            FileList.Add(f.FullName);
        }
        foreach (DirectoryInfo dir in dirInfo.GetDirectories())  
        {
            GetAllFiles(dir.FullName);
        }
    }

    static void CreateFileCs(List<string> usedScripts,  List<KeyValuePair<string,string>> guidScript)
    {
        string fileUsed = "Relative Path,GUID";
        string fileUnused = "Relative Path,GUID";
        foreach (var path in guidScript)
        {
            if (usedScripts.Contains(path.Key))
            {
                if (_unityProjectPath != null)
                    fileUsed = fileUsed + '\n' + path.Key.Substring(_unityProjectPath.Length + 1) + ',' + path.Value;
            }
            else
            {
                if (_unityProjectPath != null)
                    fileUnused = fileUnused + '\n' + path.Key.Substring(_unityProjectPath.Length + 1) + ',' +
                                 path.Value;
            }
        }
        Console.WriteLine("------------");
        Console.WriteLine(fileUsed);
        Console.WriteLine(fileUnused);
        if (Directory.Exists(_outputFolderPath))
        {
            File.WriteAllText(Path.Combine(_outputFolderPath,"UsedScripts.txt"),fileUsed);
            File.WriteAllText(Path.Combine(_outputFolderPath,"UnusedScripts.txt"),fileUnused);
        }
        else
        {
            Console.WriteLine("WHO DELETED THE FOLDER GENERATED EARLIER");
            
        }
        
    }

    static void WorkingOnUnityProject(string? projectPath, string? outputPath)
    {
        List<string> csList = new List<string>();
        List<string> sceneList = new List<string>();
        List<KeyValuePair<string, string>> metaCsList = new List<KeyValuePair<string, string>>();
        List<string> usedGuid = new List<string>();

        Console.WriteLine("Processing Unity project...");
        if (!Directory.Exists(projectPath))
        {
            Console.WriteLine($"Error: Invalid project path {projectPath}");
            return;
        }

        if (!Directory.Exists(outputPath))
        {
            Console.WriteLine($"Path {outputPath} has not been found.\n Creating new folder at {outputPath}");
            if (@outputPath != null) Directory.CreateDirectory(@outputPath);
        }

        GetAllFiles(projectPath);
        Console.WriteLine(FileList.Count());
        foreach (string f in FileList)
        {
            //Console.WriteLine(f);
            if (f.Substring(f.Length - 3) == ".cs")
            {
                csList.Add(f);
                Console.WriteLine(f);
            }

            if (f.Substring(f.Length - 6) == ".unity")
            {
                sceneList.Add(f);
                Console.WriteLine(f);
            }
        }

        foreach (string script in csList)
        {
            Console.WriteLine();
            string e = script + ".meta";
            string[] temp = File.ReadAllLines(@e);
            foreach (var line in temp)
            {
                if (line.Substring(0, 6) == "guid: ")
                {
                    metaCsList.Add(new KeyValuePair<string, string>(script, line.Substring(6)));
                }
            }
        }



        foreach (var guid in metaCsList)
        {
            Console.WriteLine(guid);
        }

        foreach (var scene in sceneList)
        {
            string temp = scene;
            string sceneFile = File.ReadAllText(@temp);
            foreach (var guid in metaCsList)
            {
                if (sceneFile.Contains(guid.Value))
                {
                    usedGuid.Add(guid.Key);
                }
            }
        }

        CreateFileCs(usedGuid, metaCsList);

        var input = new StreamReader(sceneList[1]);
        var yaml = new YamlStream();
        yaml.Load(input);


        foreach (var document in yaml.Documents)
        {
            var rootMappingNode = (YamlMappingNode)document.RootNode;
            Console.WriteLine(rootMappingNode);
            Console.WriteLine();
        }

    }
}