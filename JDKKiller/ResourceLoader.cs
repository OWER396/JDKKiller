using System.Reflection;

namespace JDKKiller
{
    public class ResourceLoader
    {
        public static Icon GetResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
            byte[] data;

            Stream? s = assembly.GetManifestResourceStream(resourceName);
            data = new byte[s.Length];
            s.Read(data, 0, data.Length);
            using (MemoryStream ms = new MemoryStream(data))
            {
                return new Icon(ms);
            }
        }
    }
}
