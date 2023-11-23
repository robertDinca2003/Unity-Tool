

namespace unityFileReader;

class GameObjects
{
    public string MName = "";
    public string ObjId = "";
    public readonly List<string> Components = new List<string>();
}

class Transforms
{
    public string TransId = "";
    public string MGameObject = "";
    public string MFather = "";
    public readonly List<string> MChildren = new List<string>();
}


class UnityFileReader
{
    private static readonly List<string> FileList = new List<string>();
    private static string? _unityProjectPath ="";
    private static string? _outputFolderPath ="";

    private static string _answerHierarchy = "";

    private static void GenerateTree(int n ,string s, Dictionary<string,GameObjects> objects, Dictionary<string,Transforms> transforms)
    {
        for(int i = 0;  i <2*n ; i++)
        {
            _answerHierarchy += "-";
        }
        _answerHierarchy += objects[transforms[s].MGameObject].MName + '\n';
        foreach (var child in transforms[s].MChildren)
        {
            GenerateTree(n+1,child,objects,transforms); 
        }
            
        
        
    }
    private static void GenerateHierarchy(string content)
    {
        var input = @content;
        List<GameObjects> allObjects = new List<GameObjects>();
        List<Transforms> allTransformsList = new List<Transforms>();
        List<string> rootsId = new List<string>();

        Dictionary<string, GameObjects> dictObjects = new Dictionary<string, GameObjects>();
        Dictionary<string, Transforms> dictTransform = new Dictionary<string, Transforms>();
        
        bool isGameObject = false;
        bool isTransform = false;
        bool isSceneRoot = false;
        bool hasChildren = false;
        foreach (string line in File.ReadLines(input))
        {
            if (line.Length>8 && line.Substring(0, 9) == "--- !u!1 ")
            {
                GameObjects newObj = new GameObjects
                {
                    ObjId = line.Substring(line.IndexOf('&') + 1)
                };
                isGameObject = true;
                isTransform = false;
                allObjects.Add(newObj);
                continue;
            }

            if (line.Length>8 && isGameObject && line.Substring(0, 9) == "--- !u!4 ")
            {
                Transforms newTransform = new Transforms();
                isTransform = true;
                newTransform.TransId = line.Substring(line.IndexOf('&') + 1);
                allTransformsList.Add(newTransform);
                continue;
            }

            if (line.Contains("SceneRoots:"))
            {isSceneRoot = true;
                isGameObject = false;
                isTransform = false;
                continue;
            }

            if (isGameObject && !isTransform)
            {
                if (line.Contains("m_Name"))
                {
                    string temp = line.Substring(line.IndexOf("m_Name: ", StringComparison.Ordinal)+8);
                    allObjects[^1].MName =temp;
                }

                if (line.Contains("component: "))
                {
                    string temp = line.Substring(line.IndexOf("fileID: ", StringComparison.Ordinal)+8);
                    allObjects[^1].Components.Add(temp.Substring(0,temp.Length-1));
                }
            }

            if (isTransform && isGameObject)
            {
                if (line.Contains("m_Children:"))
                {
                    hasChildren = true;
                    continue;
                }
                if (line.Contains("m_Children: []"))
                {
                    hasChildren = false;
                    continue;
                }
                if (line.Contains("m_Father: "))
                {
                    hasChildren = false;
                    string temp = line.Substring(line.IndexOf("fileID: ", StringComparison.Ordinal) + 8);
                    allTransformsList[^1].MFather = temp.Substring(0, temp.Length - 1);
                }
                if (hasChildren && line.Contains("fileID: "))
                {
                    string temp = line.Substring(line.IndexOf("fileID: ", StringComparison.Ordinal) + 8);
                    allTransformsList[^1].MChildren.Add(temp.Substring(0,temp.Length-1));
                }
                
                

                if (line.Contains("m_GameObject: "))
                {
                    string temp = line.Substring(line.IndexOf("fileID: ", StringComparison.Ordinal) + 8);
                    allTransformsList[^1].MGameObject = temp.Substring(0, temp.Length - 1);
                }
            }

            if (isSceneRoot)
            {
                if (line.Contains("fileID: "))
                {
                    string temp = line.Substring(line.IndexOf("fileID: ", StringComparison.Ordinal)+8);
                    rootsId.Add(temp.Substring(0,temp.Length-1));

                }
            }
            
            
        }

        foreach (var objects in allObjects)
        {
            dictObjects[objects.ObjId] = objects;
        }

        foreach (var transform in allTransformsList)
        {
            dictTransform[transform.TransId] = transform;
        }
        
        foreach (var s in rootsId)
        {
            GenerateTree(0,s,dictObjects,dictTransform);
        }
        //Console.WriteLine(answerHierarchy);
        if (Directory.Exists(_outputFolderPath))
        {
            string temp = _outputFolderPath;
            FileInfo file = new FileInfo(@content);
            temp += file.Name;
            temp += ".dump";
            Console.WriteLine(file.Name+".dump was created in the output folder" );
            File.WriteAllText(Path.Combine(_outputFolderPath,temp),_answerHierarchy);
            _answerHierarchy = "";
        }
        else
        {
            Console.WriteLine("WHO DELETED THE FOLDER GENERATED EARLIER");
            
        }
    }
      



    static bool GetAllFiles(string path, int depth)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(@path);

        if (dirInfo.Name != "Assets" && depth == 0)
        {
            foreach (var newDir in dirInfo.GetDirectories())
            {
                if (newDir.Name == "Assets")
                {
                    dirInfo = new DirectoryInfo(@newDir.FullName);
                    
                    break;
                }
            }
        }
        if (!dirInfo.FullName.Substring(dirInfo.FullName.Length-6).Contains("Assets") && depth == 0)
        {
            Console.WriteLine("No Assets Folder Found");
            return false;
        }
        FileInfo[] files = dirInfo.GetFiles();
        foreach (FileInfo f in files)
        {
            FileList.Add(f.FullName);
        }
        foreach (DirectoryInfo dir in dirInfo.GetDirectories())  
        {
            GetAllFiles(dir.FullName,depth+1);
        }

        return true;
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
                    fileUsed = fileUsed + '\n' + path.Key.Substring(_unityProjectPath.Length ) + ',' + path.Value;
            }
            else
            {
                if (_unityProjectPath != null)
                    fileUnused = fileUnused + '\n' + path.Key.Substring(_unityProjectPath.Length ) + ',' +
                                 path.Value;
            }
        }
       
        if (Directory.Exists(_outputFolderPath))
        {
            Console.WriteLine("UsedScripts.txt was created in the output folder");
            string temp = _outputFolderPath + "UsedScripts.txt";
            File.WriteAllText(@temp,fileUsed);
            temp = _outputFolderPath + "UnusedScripts.txt";
            Console.WriteLine("UnusedScripts.txt was created in the output folder");
            File.WriteAllText(temp,fileUnused);
        }
        else
        {
            Console.WriteLine("WHO DELETED THE FOLDER GENERATED EARLIER");
            
        }
        
    }

    static void WorkingOnUnityProject(string? projectPath, string? outputPath)
    {
        if (projectPath == null || outputPath == null)
        {
            Console.WriteLine("Rerun the program, it occured an problem with the paths");
        }
        if (projectPath != null && projectPath[^1] != '\\' && projectPath[^1] != '/')
        {
            if (projectPath.Contains('/'))
            {
                projectPath += "/";
                _unityProjectPath += "/";
            }

            if (projectPath.Contains('\\'))
            {
                projectPath += "\\";
                _unityProjectPath += "\\";
            }
        }
        if (outputPath != null && outputPath[^1] != '\\' && outputPath[^1] != '/')
        {
            if (outputPath.Contains('/'))
            {
                outputPath += "/";
                _outputFolderPath += "/";
            }
            if(outputPath.Contains('\\'))
            {
                outputPath += "\\";
                _outputFolderPath += "/";
            }
        }
        
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

        if (!GetAllFiles(projectPath,0))
        {
            Console.WriteLine("Rerun the program!");
            return;
        }

        foreach (string f in FileList)
        {
            if (f.Substring(f.Length - 3) == ".cs")
            {
                csList.Add(f);
            }

            if (f.Substring(f.Length - 6) == ".unity")
            {
                sceneList.Add(f);
            }
        }

        foreach (string script in csList)
        {
            string e = script + ".meta";
            if(File.Exists(@e))
                continue;
            string[] temp = File.ReadAllLines(@e);
            foreach (var line in temp)
            {
                    if (line.Length>=6 && line.Substring(0, 6) == "guid: ")
                    {
                        metaCsList.Add(new KeyValuePair<string, string>(script, line.Substring(6)));
                    } 
            }
            
            
            
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

        foreach (var scene in sceneList)
        {
            GenerateHierarchy(scene);
        }

        
    }

    static void Main(string?[] args)
    {
        if (args.Length == 2)
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
            Console.WriteLine("Unity Project Path");
            _unityProjectPath = Console.ReadLine();
            Console.WriteLine("Output Folder Path");
            _outputFolderPath = Console.ReadLine();
            
            WorkingOnUnityProject(_unityProjectPath,_outputFolderPath);
        }
    }
}