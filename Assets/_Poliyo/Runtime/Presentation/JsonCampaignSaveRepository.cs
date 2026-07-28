using System;
using System.IO;
using Newtonsoft.Json;
using Poliyo.Application;
using UnityEngine;

namespace Poliyo.Presentation
{
/// <summary>
/// Local JSON persistence adapter. The application layer owns the DTO and validation; this class only manages files.
/// </summary>
public sealed class JsonCampaignSaveRepository : ICampaignSaveRepository
{
    private readonly string _rootDirectory;

    public JsonCampaignSaveRepository(string rootDirectory = null)
    {
        _rootDirectory = string.IsNullOrWhiteSpace(rootDirectory)
            ? Path.Combine(UnityEngine.Application.persistentDataPath, "Poliyo", "Saves")
            : rootDirectory;
    }

    public void Save(string slotId, CampaignSaveData saveData)
    {
        CampaignSaveMapper.Validate(saveData);
        string path = GetPath(slotId);
        Directory.CreateDirectory(_rootDirectory);

        string temporaryPath = path + ".tmp";
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(temporaryPath, json);

        if (File.Exists(path))
        {
            File.Replace(temporaryPath, path, null);
        }
        else
        {
            File.Move(temporaryPath, path);
        }
    }

    public CampaignSaveData Load(string slotId)
    {
        string path = GetPath(slotId);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Campaign save was not found.", path);
        }

        CampaignSaveData saveData = JsonConvert.DeserializeObject<CampaignSaveData>(File.ReadAllText(path));
        CampaignSaveMapper.Validate(saveData);
        return saveData;
    }

    public bool Exists(string slotId) => File.Exists(GetPath(slotId));

    private string GetPath(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId)) throw new ArgumentException("A save slot id is required.", nameof(slotId));
        if (slotId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || slotId.Contains(".."))
        {
            throw new ArgumentException("The save slot id contains invalid path characters.", nameof(slotId));
        }

        return Path.Combine(_rootDirectory, slotId + ".json");
    }
}
}
