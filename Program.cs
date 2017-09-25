using JpmodLib.Utils;
using System;
using System.IO;
using System.Reflection;

namespace DoubleFine
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      int length = args.Length;
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool verbose = false;
      XGetopt xgetopt = new XGetopt();
      char ch;
      while ((int) (ch = xgetopt.Getopt(length, args, "updv")) != 0)
      {
        switch (ch)
        {
          case 'd':
            flag3 = true;
            continue;
          case 'p':
            flag2 = true;
            continue;
          case 'u':
            flag1 = true;
            continue;
          case 'v':
            verbose = true;
            continue;
          default:
            Program.error(string.Format("unknown option: -{0}", (object) ch));
            return;
        }
      }
      if (flag2 && flag1 || !flag2 && !flag1)
        Program.error("You can set only one command option.");
      else if (length - xgetopt.Optind < 1 && length - xgetopt.Optind > 2)
      {
        Program.error("You need to set at least input file path. Output file path is optional.");
      }
      else
      {
        string fullPath = Path.GetFullPath(args[xgetopt.Optind]);
        string dst = length - xgetopt.Optind != 1 ? Path.GetFullPath(args[xgetopt.Optind + 1]) : (!flag1 ? Path.GetFullPath(args[xgetopt.Optind]) : Path.GetFullPath(Path.GetFileNameWithoutExtension(args[xgetopt.Optind])));
        try
        {
          if (flag1)
            Program.unpack(fullPath, dst, verbose);
          else
            Program.pack(fullPath, dst, verbose);
        }
        catch (FileNotFoundException ex)
        {
          Program.error(ex.Message);
          if (!flag3)
            return;
          Program.debugException((Exception) ex);
        }
        catch (IOException ex)
        {
          Program.error(ex.Message);
          if (!flag3)
            return;
          Program.debugException((Exception) ex);
        }
        catch (Exception ex)
        {
          Program.error(ex.Message);
          if (!flag3)
            return;
          Program.debugException(ex);
        }
      }
    }

    private static void unpack(string src, string dst, bool verbose)
    {
      HFile hfile = new HFile();
      hfile.Load(src);
      if ((int) hfile.Header.Version[0] == 5)
        Console.WriteLine("Game: Stacking");
      else if ((int) hfile.Header.Version[0] == 2)
      {
        Console.WriteLine("Game: Costume Quest");
      }
      else
      {
        Program.error("Unknown version number. This file is not supported.");
        return;
      }
      Console.WriteLine(string.Format("File count: {0}", (object) hfile.Header.FileCount));
      new PFile(hfile).Unpack(src, dst);
      new HFileInfo().Save(dst, hfile);
    }

    private static void pack(string src, string dst, bool verbose)
    {
      HFile hfile = new HFileInfo().Load(src);
      if ((int) hfile.Header.Version[0] == 5)
        Console.WriteLine("Game: Stacking");
      else if ((int) hfile.Header.Version[0] == 2)
        Console.WriteLine("Game: Costume Quest");
      Console.WriteLine(string.Format("File count: {0}", (object) hfile.Header.FileCount));
      new PFile(hfile).Pack(dst, src, hfile);
      hfile.Save(dst);
    }

    private static void error(string message)
    {
      if (message != null && message.Length > 0)
      {
        Console.WriteLine("");
        Console.WriteLine("Error:");
        Console.WriteLine(" " + message);
      }
      Program.help();
    }

    private static void debugException(Exception e)
    {
      Console.WriteLine("Source:");
      Console.WriteLine(e.Source);
      Console.WriteLine("");
      Console.WriteLine("StackTrace:");
      Console.WriteLine(e.StackTrace);
    }

    private static void help()
    {
      Console.WriteLine("");
      Console.WriteLine("Usage:");
      Console.WriteLine(" Unpack: " + Program.getExeName() + " -u <~h file path> [<unpacked data path>]");
      Console.WriteLine(" Pack:   " + Program.getExeName() + " -p <unpacked data path> [<~h and ~p file name>]");
      Console.WriteLine("");
      Console.WriteLine("options:");
      Console.WriteLine("  -u        Unpack option");
      Console.WriteLine("  -p        Pack option");
      Console.WriteLine("");
      Console.WriteLine("arguments:");
      Console.WriteLine("  <~h file path>        ~h header file path");
      Console.WriteLine("  <unpacked data path>  Unpacked data directory path");
      Console.WriteLine("  <~h and ~p file name> File path for ~h and ~p");
      Console.WriteLine("");
    }

    private static string getExeName()
    {
      return new FileInfo(Assembly.GetEntryAssembly().Location).Name;
    }
  }
}
