using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace UEK_Harmonogram
{
    class FilesStorage
    {
        StorageFolder folder = ApplicationData.Current.LocalFolder;

        public async Task StringToFile(string content, string fileName)
        {
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, content);
        }

        public async Task<string> FileToString(string fileName)
        {
            var plik = await this.GetFileIfExists(fileName);
            if (plik != null)
                return await FileIO.ReadTextAsync(plik);
            else
                return null;
        }

        public async Task<StorageFile> GetFileIfExists(string fileName)
        {
            if (await folder.TryGetItemAsync(fileName) != null)
                return await folder.GetFileAsync(fileName);
            else
                return null;
        }

        public async Task<bool> IsFileExists(string fileName)
        {
            var item = await folder.TryGetItemAsync(fileName);
            return item != null;
        }

        //tmp
        public async Task DeleteAllFiles()
        {
            IReadOnlyList<StorageFile> Files = await folder.GetFilesAsync();
            for (int i = 0; i < Files.Count; i++)
                await Files[i].DeleteAsync(StorageDeleteOption.Default);
        }

        public async Task DeleteUnusedFiles(ObservableCollection<Zestaw> packages)
        {
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            HashSet<string> fileNames = new HashSet<string>();

            for (int i = 0; i < packages.Count; i++)
            {
                var groups = packages[i].Groups;
                if (groups != null)
                {
                    for (int j = 0; j < groups.Count; j++)
                    {
                        var group = groups[j];
                        if (group != null)
                            fileNames.Add(group.Type + group.Id + ".xml");
                    }
                }
            }

            for (int i = 0; i < files.Count; i++)
            {
                //if (files[i].Name == LessonsList) continue;
                var result = fileNames.Contains(files[i].Name);
                if (!result)
                    await files[i].DeleteAsync(StorageDeleteOption.Default);
            }
        }

        // tmp
        public async void GetListOfFiles()
        {
            IReadOnlyList<StorageFile> files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            string text = "";
            foreach (StorageFile file in files)
            {
                text = text + file.Name.ToString() + "\n";
            }
            var msg = new Windows.UI.Popups.MessageDialog(text);
            await msg.ShowAsync();
        }
    }
}
