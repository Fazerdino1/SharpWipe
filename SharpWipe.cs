// MetadataCleaner.cs
// Efface 4 metadonnees video via le Windows Property Store (IPropertyStore COM)
// Compilation : csc.exe MetadataCleaner.cs /out:MetadataCleaner.exe
// Usage : MetadataCleaner.exe [dossier]  (sans argument = dossier courant)
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

class MetadataCleaner
{
    // ---- Structures COM ----

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    // PROPVARIANT minimal : vt=0 (VT_EMPTY) = efface la valeur
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public IntPtr pVal;
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint cProps);
        [PreserveSig] int GetAt(uint iProp, out PROPERTYKEY pkey);
        [PreserveSig] int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        [PreserveSig] int SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        [PreserveSig] int Commit();
    }

    // ---- P/Invoke ----

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    static extern int SHGetPropertyStoreFromParsingName(
        string pszPath, IntPtr pbc, uint flags,
        ref Guid riid, out IPropertyStore ppv);

    static readonly Guid IID_IPropertyStore =
        new Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");

    // GPS_READWRITE = 2
    const uint GPS_READWRITE = 2;

    // ---- Proprietes cibles ----
    // System.Title      {F29F85E0-...} pid=2
    // System.Comment    {F29F85E0-...} pid=6  => Description/Resume dans Explorer
    // System.Keywords   {F29F85E0-...} pid=5  => Tags
    // System.Category   {D5CDD502-...} pid=2

    static PROPERTYKEY[] KEYS;
    static string[]      KEY_NAMES;

    static readonly string[] VIDEO_EXTS =
    {
        "*.mp4", "*.m4v", "*.mov",
        "*.avi", "*.wmv", "*.mkv",
        "*.flv", "*.3gp"
    };

    static void Main(string[] args)
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }

        KEYS = new PROPERTYKEY[4];
        KEYS[0] = MakeKey("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 2);
        KEYS[1] = MakeKey("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 6);
        KEYS[2] = MakeKey("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 5);
        KEYS[3] = MakeKey("D5CDD502-2E9C-101B-9397-08002B2CF9AE", 2);

        KEY_NAMES = new string[4];
        KEY_NAMES[0] = "Titre       (System.Title)";
        KEY_NAMES[1] = "Description (System.Comment)";
        KEY_NAMES[2] = "Tags        (System.Keywords)";
        KEY_NAMES[3] = "Categorie   (System.Category)";

        string folder;
        if (args.Length > 0 && Directory.Exists(args[0]))
            folder = Path.GetFullPath(args[0]);
        else
            folder = Directory.GetCurrentDirectory();

        Console.WriteLine("==============================================");
        Console.WriteLine("  MetadataCleaner - Windows natif");
        Console.WriteLine("==============================================");
        Console.WriteLine("Dossier : " + folder);
        Console.WriteLine();

        int total = 0, ok = 0, partial = 0, failed = 0;

        foreach (string ext in VIDEO_EXTS)
        {
            foreach (string file in Directory.GetFiles(folder, ext,
                         SearchOption.TopDirectoryOnly))
            {
                total++;
                int r = ProcessFile(file);
                if      (r == 2) ok++;
                else if (r == 1) partial++;
                else             failed++;
            }
        }

        Console.WriteLine();
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("  Traites : " + total);
        Console.WriteLine("  OK      : " + ok);
        Console.WriteLine("  Partiel : " + partial + "  (propriete lecture seule)");
        Console.WriteLine("  Echec   : " + failed);
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine();
        Console.WriteLine("Note : MKV = handler Windows lecture seule => Partiel normal.");
        Console.WriteLine();
        Console.Write("Appuyez sur une touche...");
        Console.ReadKey();
    }

    static int ProcessFile(string filePath)
    {
        string name = Path.GetFileName(filePath);
        string display = name.Length > 50 ? name.Substring(0, 47) + "..." : name.PadRight(50);
        Console.Write("  " + display + " => ");

        Guid iid = IID_IPropertyStore;
        IPropertyStore store = null;
        int hr = SHGetPropertyStoreFromParsingName(
                     filePath, IntPtr.Zero, GPS_READWRITE, ref iid, out store);

        if (hr != 0 || store == null)
        {
            Console.WriteLine("ECHEC [0x" + hr.ToString("X8") + "]");
            return 0;
        }

        bool anyRO = false;
        try
        {
            for (int i = 0; i < KEYS.Length; i++)
            {
                PROPERTYKEY k     = KEYS[i];
                PROPVARIANT empty = new PROPVARIANT(); // vt=0 => VT_EMPTY
                int setHr = store.SetValue(ref k, ref empty);
                if (setHr != 0)
                {
                    if (!anyRO) Console.WriteLine();
                    Console.WriteLine("    [RO] " + KEY_NAMES[i]
                                      + " => 0x" + setHr.ToString("X8"));
                    anyRO = true;
                }
            }

            int commitHr = store.Commit();
            if (commitHr != 0)
            {
                if (!anyRO) Console.WriteLine();
                Console.WriteLine("    COMMIT ECHEC => 0x" + commitHr.ToString("X8"));
                return 0;
            }
        }
        finally
        {
            Marshal.ReleaseComObject(store);
        }

        if (anyRO) { Console.WriteLine("    => PARTIEL"); return 1; }
        Console.WriteLine("OK");
        return 2;
    }

    static PROPERTYKEY MakeKey(string fmtidStr, uint pid)
    {
        PROPERTYKEY k;
        k.fmtid = new Guid(fmtidStr);
        k.pid   = pid;
        return k;
    }
}
