using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

// A standalone regression runner: no test framework or registered custom TLB required.
internal static class Program
{
    private static int failures;

    [DllImport("oleaut32.dll")]
    private static extern int VariantClear(IntPtr variant);

    private static void Check(bool condition, string name)
    {
        Console.WriteLine((condition ? "PASS " : "FAIL ") + name);
        if (!condition) ++failures;
    }

    private static void CheckTypeLib(Assembly assembly, string path)
    {
        try
        {
            Type type = assembly.GetType("OleViewDotNet.TypeLib.COMTypeLib", true);
            object lib = type.GetMethod("FromFile").Invoke(null, new object[] { path });
            string text = (string)type.GetMethod("FormatText").Invoke(lib, new object[] { Type.Missing });
            Check(!String.IsNullOrEmpty(text), "parse and format " + path);
        }
        catch (Exception ex)
        {
            Check(false, "parse " + path + ": " + ex.GetBaseException());
        }
    }

    private static int Main(string[] args)
    {
        Console.OutputEncoding = new System.Text.UTF8Encoding(false);
        int next = 0;
        string assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OleViewDotNet.exe");
        if (args.Length > 1 && args[0] == "--assembly")
        {
            assemblyPath = Path.GetFullPath(args[1]);
            next = 2;
        }
        Assembly assembly = Assembly.LoadFrom(assemblyPath);
        Console.WriteLine("Process: " + (IntPtr.Size * 8) + "-bit; " + assembly.Location);
        Type desc = assembly.GetType("OleViewDotNet.Interop.PARAMDESCEX", true);
        MethodInfo read = assembly.GetType("OleViewDotNet.TypeLib.COMTypeLibUtils", true)
            .GetMethod("ReadDefaultValue", BindingFlags.Public | BindingFlags.Static);
        Check(Marshal.OffsetOf(desc, "varDefaultValue").ToInt32() == 8, "VARIANT offset is 8");
        int nativeSize = IntPtr.Size == 4 ? 24 : 32;
        Check(Marshal.SizeOf(desc) == nativeSize, "PARAMDESCEX matches native size");
        Check(read.Invoke(null, new object[] { IntPtr.Zero }) == null, "null PARAMDESCEX");

        object[] values = { 0, 123, -1, (short)42, false, true, 1.25, 12.5m, "default", "" };
        foreach (object expected in values)
        {
            // Build the native layout independently of the structure under test.
            IntPtr ptr = Marshal.AllocHGlobal(nativeSize);
            IntPtr variant = IntPtr.Add(ptr, 8);
            try
            {
                for (int i = 0; i < nativeSize; ++i) Marshal.WriteByte(ptr, i, 0);
                Marshal.WriteInt32(ptr, nativeSize);
                Marshal.GetNativeVariantForObject(expected, variant);
                object actual = read.Invoke(null, new object[] { ptr });
                Check(Object.Equals(actual, expected), expected.GetType().Name + " default: " + expected);
            }
            catch (Exception ex)
            {
                Check(false, expected.GetType().Name + " default: " + ex.GetBaseException());
            }
            finally
            {
                VariantClear(variant);
                Marshal.FreeHGlobal(ptr);
            }
        }

        string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
        CheckTypeLib(assembly, Path.Combine(system, "stdole2.tlb"));
        CheckTypeLib(assembly, Path.Combine(system, "scrrun.dll"));
        CheckTypeLib(assembly, Path.Combine(system, "shell32.dll"));
        for (; next < args.Length; ++next) CheckTypeLib(assembly, Path.GetFullPath(args[next]));
        Console.WriteLine("Failures: " + failures);
        return failures == 0 ? 0 : 1;
    }
}
