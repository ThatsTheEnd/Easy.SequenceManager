# Easy Sequence Manager

The Easy Sequence Manager is a C# library designed to parse and execute sequences of steps and nested sub-sequences defined in JSON files. It handles the execution order of these sequences, including parallel and synchronous execution of steps.

## Features

- Load sequences from JSON files.
- Supports nested sub-sequences.
- Handle parallel and synchronous execution of steps.
- Extensible design for additional sequence element types.

## Usage

To use the Sequence Manager, you need to create a JSON file that defines your sequences and steps. Then, instantiate `SequenceManager` in your C# project and call `LoadJsonSequence` with the path to your JSON file.

### JSON Structure

The JSON file should follow this structure:

```json
{
  "SequenceManager": {
    "FileVersion": "1.0.0",
    "CheckSum": "checksum_here",
    "Sequence": {
      "Elements": [
        {
          "Type": "Step",
          "Name": "Step Name",
          "IsSynchronous": true,
          "IsParallel": false,
          "Documentation": "Description of step",
          "Parameters": [
            // Parameters for the step
          ]
        },
        {
          "Type": "SubSequence",
          "Name": "SubSequence Name",
          "SubSequenceFilePath": "path/to/subsequence.json"
        }
        // Other steps or sub-sequences
      ]
    }
  }
}
```
## Classes
The class diagram is shown in the file [class_diagram.puml](class_diagram.puml) and also visualized here:

![Easy_SequenceManager_Class_Diagram.png](Easy_SequenceManager_Class_Diagram.png)

- SequenceManager: Main class to load and manage sequences.
- Sequence: Represents a sequence of steps or sub-sequences.
- Step: Represents an individual step in a sequence.
- SubSequence: Represents a nested sequence.
- Parameter: Represents parameters used in steps.

## Methods
LoadJsonSequence(string filePath): Loads a sequence from a JSON file.
GetNextElementsToExecute(): Returns the next set of elements (steps or sub-sequences) to be executed.

## Example
```c#
var sequenceManager = new SequenceManager();
sequenceManager.LoadJsonSequence("path/to/sequence.json");

while (true)
{
    var nextElements = sequenceManager.Sequence.GetNextElementsToExecute();
    if (!nextElements.Any())
        break;

    // Execute each element in nextElements
}

```

# Development
This project is open for enhancements and contributions. Feel free to fork and improve the functionality or to add support for more complex sequence structures.