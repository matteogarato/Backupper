namespace BackupperConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var b = new Backupper(args.Any(a => a == "-s"));
            b.Run();
        }
    }
}