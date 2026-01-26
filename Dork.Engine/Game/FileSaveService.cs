using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Dork.Engine.Game
{
    public sealed class FileSaveService : ISaveService
    {
        private readonly string _path;

        public FileSaveService(string path = "save.json")
        {
            _path = path;
        }

        public bool Exists() => File.Exists(_path);

        public void Write(SaveGame save)
        {
            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        public SaveGame Read()
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<SaveGame>(json)
                   ?? throw new InvalidOperationException("Save file exists but could not be read.");
        }
    }
}
