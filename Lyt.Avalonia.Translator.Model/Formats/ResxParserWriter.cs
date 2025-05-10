namespace Lyt.Avalonia.Translator.Model.Formats;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

public static class ResxParserWriter
{
    private const string ResourceDictionaryHeader =
@"
<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
  <xsd:schema xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" id=""root"">
    <xsd:element name=""data"">
      <xsd:complexType>
        <xsd:sequence>
          <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2""/>
        </xsd:sequence>
        <xsd:attribute name=""name"" type=""xsd:string""/>
        <xsd:attribute name=""type"" type=""xsd:string""/>
        <xsd:attribute name=""mimetype"" type=""xsd:string""/>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
    <value>2.0</value>
  </resheader>
  <resheader name=""reader"">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
";

    private const string ResourceDictionaryFooter =
@"
</root>
";

    private const string ResourceDictionaryEntryFormat =
@"
  <data name=""{0}"">
    <value>{1}</value>
  </data>
";

    public static Tuple<bool, Dictionary<string, string>> ParseResourceFile(string sourcePath)
    {
        try
        {
            var xmlElement = XElement.Load(sourcePath);
            var entries =
                (from element in xmlElement.Descendants("data")
                 where (element.Attribute("name") is not null) &&
                       (element.Element("value") is not null)
                 select new Entry
                 {
                     Key = element.Attribute("name")!.Value,
                     Value = element.Element("value")!.Value,
                 })
                 .ToList();

            Dictionary<string, string> dictionary = [];
            foreach (var entry in entries)
            {
                string key = entry.Key;
                string value = entry.Value;
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                dictionary.Add(key, value);
            }

            return Tuple.Create(true, dictionary);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            if (Debugger.IsAttached) { Debugger.Break(); }
            return Tuple.Create(false, new Dictionary<string, string>());
        }
    }

    public static bool CreateResourceFile(string destinationPath, Dictionary<string, string> dictionary)
    {
        try
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(ResourceDictionaryHeader);
            foreach (var item in dictionary)
            {
                string key = item.Key;
                string value = item.Value;
                string line = string.Format(ResourceDictionaryEntryFormat, key, value);
                stringBuilder.Append(line);
            }

            stringBuilder.Append(ResourceDictionaryFooter);
            File.WriteAllText(destinationPath, stringBuilder.ToString());

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            if (Debugger.IsAttached) { Debugger.Break(); }
            return false;
        }
    }

    public class Entry
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
