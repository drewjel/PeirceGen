using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ros_parse
{
    class ParseCorpus
    {
        static int count = 0;
        static int tf_count = 0;
        static int failed = 0;
        static int tf_project = 0;

        public enum NodeKind
        {
            /*     if(auto con = dyn_cast<CXXConstructExpr>(stmt))
        {
          node_type = "constructor";
        }
        else if(auto mem = dyn_cast<CXXMemberCallExpr>(stmt))
        {
          node_type = "member call";
        }
        else if(auto op = dyn_cast<CXXOperatorCallExpr>(stmt))
        {
          node_type="operator call";
        }
        else if(auto ce = dyn_cast<CallExpr>(stmt))
        {
          node_type = "call";
        }
        else if(auto binop = dyn_cast<BinaryOperator>(stmt))
        {
          node_type="binary operator";
             * */
             none,
             cons,
             memcall,
             opcall,
             call,
             binop
        }

        class TreeNode
        {
            /*
             * 
             * 558,554,class tf2::Quaternion,none,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:9
                559,558,class tf2::Quaternion,constructor,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:9
                560,559,const tf2Scalar,none,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:20
                561,560,getYaw,call,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:20
                562,561,double (*)(const class tf2::Quaternion &),none,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:20
             * 
             * */
             public int ID { get; set; }
             public int ParentID { get; set; }
             public TreeNode Parent { get; set; }
             
             public string Type { get; set; }

             public NodeKind Kind { get; set; }

             public string Name { get; set; }

             public string Loc { get; set; }


             private List<TreeNode> children_;
             public List<TreeNode> children { get { return children_ ?? (children_ = new List<TreeNode>()); } set { children_ = value; } } 
        }

        class ResultRecord
        {
            public string FileName { get; set; }
            public string Stub { get; set; }
            public string Name { get; set; }

            public int Count { get; set; }

            public override bool Equals(object obj)
            {
                var rec = (ResultRecord)obj;

                return rec.Stub == this.Stub && rec.Name == this.Name;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public (string, string) ToTuple()
            {
                return (this.Stub, this.Name);
            }
        }

        static void parseTree(List<TreeNode> to, List<string> from)
            {
            /*
             *0 sentinel parent id
             *
             * 558,554,class tf2::Quaternion,none,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:9
                559,558,class tf2::Quaternion,constructor,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:9
                560,559,const tf2Scalar,none,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:20
                561,560,getYaw,call,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:20
                562,561,double (*)(const class tf2::Quaternion &),none,/peirce/rostmp/navigation/global_planner/src/orientation_filter.cpp:144:20
             * 
             * */
            try
            {
                foreach (var str in from)
                {
                    var toks = str.Split(new string[] { "$$$$" }, StringSplitOptions.None);
                    try
                    {
                        to.Add(new TreeNode()
                        {
                            ID = int.Parse(toks[0]),
                            ParentID = int.Parse(toks[1]),
                            Type = toks[2].Replace("class ","").Replace("const ","").Replace(" *","").Replace(" &",""),
                            Kind = (NodeKind)Enum.Parse(typeof(NodeKind), toks[3]),
                            Name = toks[4],
                            Loc = toks[5]
                        });
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine(ex.Message);
                    }
                }

                //map parents and children
                foreach (var node in to)
                {
                    try {
                    node.Parent = node.ParentID == 0 ? null : to.Single(n => n.ID == node.ParentID);
                    if (node.ParentID != 0) to.Single(n => n.ID == node.ParentID).children.Add(node);
                    
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
        }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            }

        static List<ResultRecord> searchTree(List<TreeNode> tree, string fname)
            {
                

                //check for patterns :
                //1. tf member calls
                //2. tf constructors
                //3. tf arg to function or something else
                //4. tf assignment
                //5. tf binary operation
                //6. tf operator call (i.e. assignment
                List<int> parsedIds = new List<int>();

            List<ResultRecord> results = new List<ResultRecord>();
            try
            {
                foreach (var node in tree.Where(n_ => n_.Type.Contains("tf") || n_.Type.Contains("tf2")))
                {
                    if (parsedIds.Contains(node.ID))
                        continue;

                    switch (node.Kind)
                    {
                        case NodeKind.binop:
                            {
                                results.Add(new ResultRecord()
                                {
                                    Stub = "(" + node.children[0].Type + ") " + node.Type + " (" + node.children[0].Type + ")",
                                    Name = "Binary Op: " + node.Type,
                                    Count = 1,
                                    FileName = fname
                                });
                                parsedIds.Add(node.ID);
                                continue;
                            }
                        case NodeKind.call:
                            {
                                results.Add(new ResultRecord()
                                {
                                    Stub = node.Type + " " + node.Name + "(" + string.Join(",", node.children.GetRange(1, node.children.Count - 1).Select(n_ => n_.Type)) + ")",
                                    Name = "Call Expr: " + node.Name,
                                    Count = 1,
                                    FileName = fname
                                });
                                parsedIds.Add(node.ID);
                                continue;
                            }
                        case NodeKind.cons:
                            {
                                results.Add(new ResultRecord()
                                {
                                    Stub = node.Type + "(" + string.Join(",", node.children.Select(n_ => n_.Type)) + ")",
                                    Name = "Constructor : " + node.Type,
                                    Count = 1,
                                    FileName = fname
                                });
                                parsedIds.Add(node.ID);
                                continue;
                            }
                        case NodeKind.memcall:
                            {
                                results.Add(new ResultRecord()
                                {
                                    Stub = node.children[0].children[0].Type + "." + node.Name + "(" + string.Join(",", node.children.GetRange(1, node.children.Count - 1).Select(n_ => n_.Type)) + ")",
                                    Name = "Member Call : " + node.Name,
                                    Count = 1,
                                    FileName = fname
                                });
                                continue;
                            }
                        case NodeKind.opcall:
                            {
                                try
                                {
                                    results.Add(new ResultRecord()
                                    {
                                        Stub = node.Type + " " + node.Name + "(" + string.Join(",", node.children.GetRange(1, node.children.Count - 1).Select(n_ => n_.Type)) + ")",
                                        Name = "Operator Call : " + node.Name,
                                        Count = 1,
                                        FileName = fname
                                    });
                                    parsedIds.Add(node.ID);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                                continue;
                            }
                        case NodeKind.none:
                            continue;
                    }


                }
                foreach (var node in tree.Where(n_ => n_.Type.Contains("tf") || n_.Type.Contains("tf2")).Where(n_ => !parsedIds.Contains(n_.ID)))
                {
                    var parentID = node.ParentID;
                    var parent = node;
                    while (parentID != 0)
                    {
                        parent = parent.Parent;
                        parentID = parent.ParentID;

                        switch (parent.Kind)
                        {
                            case NodeKind.binop:
                                {
                                    results.Add(new ResultRecord()
                                    {
                                        Stub = "(" + parent.children[0].Type + ") " + parent.Type + " (" + parent.children[0].Type + ")",
                                        Name = "Binary Op: " + parent.Type,
                                        Count = 1,
                                        FileName = fname
                                    });
                                    parsedIds.Add(parent.ID);
                                    continue;
                                }
                            case NodeKind.call:
                                {
                                    results.Add(new ResultRecord()
                                    {
                                        Stub = parent.Type + " " + node.Name + "(" + string.Join(",", parent.children.GetRange(1, parent.children.Count - 1).Select(n_ => n_.Type)) + ")",
                                        Name = "Call Expr: " + parent.Name,
                                        Count = 1,
                                        FileName = fname
                                    });
                                    parsedIds.Add(parent.ID);
                                    continue;
                                }
                            case NodeKind.cons:
                                {
                                    results.Add(new ResultRecord()
                                    {
                                        Stub = parent.Type + "(" + string.Join(",", parent.children.Select(n_ => n_.Type)) + ")",
                                        Name = "Constructor : " + parent.Type,
                                        Count = 1,
                                        FileName = fname
                                    });
                                    parsedIds.Add(parent.ID);
                                    continue;
                                }
                            case NodeKind.memcall:
                                {
                                    try
                                    {
                                        results.Add(new ResultRecord()
                                        {
                                            Stub = parent.children[0].children[0].Type + "." + parent.Name + "(" + string.Join(",", parent.children.GetRange(1, parent.children.Count - 1).Select(n_ => n_.Type)) + ")",
                                            Name = "Member Call : " + parent.Name,
                                            Count = 1,
                                            FileName = fname
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                    continue;
                                }
                            case NodeKind.opcall:
                                {
                                    results.Add(new ResultRecord()
                                    {
                                        Stub = parent.Type + " " + parent.Name + "(" + string.Join(",", parent.children.GetRange(1, parent.children.Count - 1).Select(n_ => n_.Type)) + ")",
                                        Name = "Operator Call : " + parent.Name,
                                        Count = 1,
                                        FileName = fname
                                    });
                                    parsedIds.Add(parent.ID);
                                    continue;
                                }
                            case NodeKind.none:
                                continue;
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (results.Count > 0)
                tf_count++;

            return results;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        static List<ResultRecord> searchForSource(string sourceDirectory, List<string> includes)
        {
            List<ResultRecord> recs = new List<ResultRecord>();
            try
            {
                foreach (var f in Directory.EnumerateFiles(sourceDirectory).Where(f_ => Regex.Match(f_, @"cpp$").Success))
                {
                    var text = File.ReadAllText(f);
                    if (!text.Contains("include <tf") && !text.Contains("include \"tf"))
                        continue;//don't bother crawling ast

                    //-extra-arg=-I/opt/ros/melodic/include/
                    //-extra-arg=-I/peirce/rostmp/navigation/global_planner/include

                    //odd directory behavior
                    var parser_cmd = "./parser " + f + " " + string.Join(" ", includes);

                    parser_cmd = parser_cmd.Replace("\\", "/");
                    parser_cmd = parser_cmd.Replace("D:/ros_repos", "/peirce");

                    var guid = Guid.NewGuid();

                    var outputfile = "/peirce/parser/" + guid + ".txt";

                    parser_cmd += " > " + outputfile;


                    var shelloutput = new List<string>();
                    var output = new List<string>();
                    try
                    {
                        Process cmd;
                       // ProcessStartInfo s;


                        cmd = new Process();
                        cmd.StartInfo.FileName = "cmd.exe";
                        cmd.StartInfo.RedirectStandardInput = true;
                        //cmd.StartInfo.RedirectStandardOutput = true;
                        cmd.StartInfo.CreateNoWindow = true;
                        cmd.StartInfo.UseShellExecute = false;
                        //cmd.StartInfo.RedirectStandardOutput = true;
                        //cmd.OutputDataReceived += (sender, args) => shelloutput.Add(args.Data);
                        cmd.Start();
                        // Thread.Sleep(2000);
                        //ShowWindow(cmd.Handle, 5);
                        //cmd.BeginOutputReadLine();
                        // cmd.StandardInput.WriteLine("cd " + copyloc);
                        //  cmd.StandardInput.Flush();
                        // cmd.StandardInput.WriteLine("copy /Y "  + f);
                        // cmd.StandardInput.Flush();
                        // cmd.StandardInput.WriteLine("");

                        cmd.StandardInput.WriteLine(@"docker exec peirce_docker /bin/bash -c ""cd /peirce/parser && " + parser_cmd + @"""");
                        cmd.StandardInput.Flush();
                        // var str = cmd.StandardOutput.ReadToEnd();
                        Thread.Sleep(10000);
                        try
                        {
                            output = File.ReadAllLines("D:\\ros_repos\\parser\\" + guid + ".txt").ToList();
                        }
                        catch (Exception ex)
                        {

                        }

                        bool isValid = output.Where(o => !string.IsNullOrEmpty(o)).ToList().TrueForAll(o => !o.Contains("fatal error") && !o.Contains("file not found")); //include not resolving

                        count++;
                        failed = isValid ? failed : failed + 1;
                        float pct = (failed) / ((float)count);
                        if (isValid || true)
                        {
                            List<TreeNode> progTree = new List<TreeNode>();
                            parseTree(progTree, output.Where(o => !string.IsNullOrEmpty(o)).ToList());
                            recs.AddRange(searchTree(progTree, f));
                        }
                        else
                        {
                            Console.WriteLine("Failed file : " + f);
                            output.ForEach(o => { if ((o.Contains("fatal error") || o.Contains("file not found"))) Console.WriteLine(o); });
                        }

                        //cmd.
                        //why on earth does del break this directory???
                        cmd.StandardInput.WriteLine("docker exec peirce_docker /bin/bash -c \"rm -rf " + outputfile + "\"");
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        Thread.Sleep(2000);
                        cmd.WaitForExit();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error");
                    }

                   // bool isValid = output.Where(o => !string.IsNullOrEmpty(o)).ToList().TrueForAll(o => !o.Contains("fatal error") && !o.Contains("file not found")); //include not resolving

                    //else
                    //    return default(List<ResultRecord>);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return recs;
        }

        static List<ResultRecord> searchForCatkin(string currentDirectory, List<string> includes)
        {
            List<ResultRecord> results = new List<ResultRecord>();

            if (Directory.EnumerateFiles(currentDirectory).ToList().Any(f => Regex.Match(f, @"\\package.xml$").Success))
            {
                try
                {
                    var p = Directory.EnumerateFiles(currentDirectory).ToList().Single(f => Regex.Match(f, @"\\package.xml$").Success);
                    var pstr = File.ReadAllText(p);

                    if (pstr.Contains(">tf") && Directory.EnumerateDirectories(currentDirectory).Any(d => Regex.Match(d, @"\\src$").Success))
                    {
                        tf_project += 1;
                        results.AddRange(searchForSource(Directory.EnumerateDirectories(currentDirectory).Single(d => Regex.Match(d, @"\\src$").Success),
                                includes));
                    }
                }
                catch (Exception ex)
                {
                    var q = Directory.EnumerateDirectories(currentDirectory).ToList();
                    var t = Directory.EnumerateFiles(currentDirectory).ToList();
                }
            }
            else
            {
                Directory.EnumerateDirectories(currentDirectory).ToList().ForEach(d => results.AddRange(searchForCatkin(d, includes)));
            }

            return results;
        }

        static void searchForIncludeDirs(string currentDirectory, List<string> toFill)
        {
            var folders = Directory.EnumerateDirectories(currentDirectory).ToList();

            if (folders.Any(f => Regex.Match(f, @"\\include$").Success))
                toFill.Add(folders.Single(f => Regex.Match(f, @"\\include$").Success));

            foreach (var f in folders)
                searchForIncludeDirs(f, toFill);
        }

        static List<string> buildClangArgs(List<string> projSpecificIncludes)
        {
            return projSpecificIncludes.Select(
               f => "-extra-arg=-I" + f
            ).ToList().Append("-extra-arg=-I/opt/ros/melodic/include/").ToList();
        }

        static void Main(string[] args)
        {

            var folders = Directory.EnumerateDirectories("D:\\ros_repos").ToList();

            //ConcurrentDictionary
            object update_lock = new object();
            //header
            List<string> projects = new List<string>();
            //rows
            Dictionary<(string, string), List<int>> AllResults = new Dictionary<(string, string), List<int>>();

            var top50csv = File.ReadAllLines("D:\\ros_repos\\Raw ROS Corpus Data - basereport.csv");
            var top50 = new List<string>();
            for(int i = 1;i<51;i++)
            {
                top50.Add(top50csv[i].Split(',')[0]);
            }


            Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                (f) =>
                {
                    try
                    {
                        if (!top50.Any(top => f.Contains(top)))
                            return;

                        string projName = "";
                        var results = new List<ResultRecord>();
                        string[] lines = null;

                        Dictionary<(string, string), int> ProjLevelCounts = new Dictionary<(string, string), int>();

                        foreach (var ftop in Directory.EnumerateFileSystemEntries(f))
                        {
                            if (File.GetAttributes(ftop).HasFlag(FileAttributes.Directory))
                            {
                                var includes = new List<string>();
                                searchForIncludeDirs(ftop, includes);
                                includes = buildClangArgs(includes);


                                results = searchForCatkin(ftop, includes).ToList();
                            }
                            else
                            {
                                projName = ftop.Split('\\').Reverse().First();
                                projName = projName.Substring(0, projName.Length - 4);
                                lines = File.ReadAllLines(ftop);

                            }
                        }

                        ProjLevelCounts = results.GroupBy(res => res.ToTuple(), res => res.Count, (k, v) => (key : k, sum : v.Sum())).ToDictionary(grp => grp.key, grp => grp.sum);
                        //can serialize these into project-specific results, or just merge with everything else...

                        lock(update_lock)
                        {
                            foreach(var count in ProjLevelCounts)
                            {
                                if(AllResults.ContainsKey(count.Key))
                                {
                                    AllResults[count.Key].Add(count.Value);
                                }
                                else
                                {
                                    AllResults[count.Key] = Enumerable.Repeat(0, projects.Count).Append(count.Value).ToList();
                                }
                            }
                            foreach(var count in AllResults)
                            {
                                if (!ProjLevelCounts.ContainsKey(count.Key))
                                    AllResults[count.Key].Add(0);
                            }


                            projects.Add(projName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            );

            var all_lines = new List<string>();

            all_lines.Add(string.Join(",", projects));
            all_lines[0] = ",ProjectName," + all_lines[0];
            all_lines.AddRange(
                AllResults.Select(kv => "\""+kv.Key.Item1+"\"" + "," + kv.Key.Item2 + "," + string.Join(",", kv.Value.Select(int_ => string.Empty + int_)))
            );

            File.WriteAllLines("basereport.csv", all_lines);
        }
    }
}
