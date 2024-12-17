
namespace ATS_API.SaveLoading;

/// <summary>
/// Indicate the state of the file.
/// </summary>
public enum SaveFileState
{
    /// <summary>
    /// The file is new:
    /// (1) The save does not exists, we create a new one instead of loading a file.
    /// (2) This is the first time before the new data saves.
    /// This enum indicates the SaveData is newly created, 
    /// you may want to do some initialize here.
    /// </summary>
    NewFile,
    /// <summary>
    /// Load the existing saves.
    /// It is not newly created save.
    /// </summary>
    LoadedFile,
}