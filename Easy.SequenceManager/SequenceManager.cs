using Easy.SequenceManager;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Easy.SequenceManager
{
    public class SequenceManager
{
    // Properties to store sequence file details
    public string FileVersion { get; set; }
    public string CheckSum { get; set; }
    public string BaseDirectory { get; private set; }
    public Sequence Sequence { get; set; }

    /// <summary>
    /// Loads a sequence from a JSON file and initializes the sequence elements.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="JsonException">Thrown when there is an error processing the JSON file.</exception>
    public void LoadJsonSequence(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");

        BaseDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
        LoadSequence(filePath); // Load the sequence from the file
    }

    /// <summary>
    /// Private helper method to load a sequence from the given JSON file.
    /// </summary>
    /// <param name="filePath">Path to the JSON file containing the sequence.</param>
    private void LoadSequence(string filePath)
    {
        string jsonText = File.ReadAllText(filePath);
        JObject jObject = JObject.Parse(jsonText);

        JToken sequenceManagerToken = jObject["SequenceManager"] ?? throw new JsonException("Missing 'SequenceManager' section in JSON.");
        FileVersion = (string)sequenceManagerToken["FileVersion"];
        CheckSum = (string)sequenceManagerToken["CheckSum"];

        Sequence = new Sequence();
        JArray elementsArray = (JArray)sequenceManagerToken["Sequence"]["Elements"];
        foreach (var element in elementsArray)
        {
            string type = (string)element["Type"];
            switch (type)
            {
                case "Step":
                    Sequence.Elements.Add(element.ToObject<Step>());
                    break;
                case "SubSequence":
                    SubSequence subSequence = element.ToObject<SubSequence>();
                    subSequence.LoadSubSequence(BaseDirectory); // Load the sub-sequence
                    Sequence.Elements.Add(subSequence);
                    break;
            }
        }
    }
}
  }
