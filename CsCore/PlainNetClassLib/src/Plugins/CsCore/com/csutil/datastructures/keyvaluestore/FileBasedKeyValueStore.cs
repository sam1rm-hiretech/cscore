using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.keyvaluestore {

    public class FileBasedKeyValueStore : IKeyValueStore {

        /// <summary> Will create a new store instance </summary>
        /// <param name="folderName"> e.g. "MyPersistedElems1" </param>
        public static FileBasedKeyValueStore New(string folderName) {
            return new FileBasedKeyValueStore(EnvironmentV2.instance.GetRootAppDataFolder().GetChildDir(folderName));
        }

        private DirectoryEntry folderForAllFiles;
        public IKeyValueStore fallbackStore { get; set; }

        private class PrimitiveWrapper { public object val; }

        public FileBasedKeyValueStore(DirectoryEntry folderForAllFiles) { this.folderForAllFiles = folderForAllFiles; }

        public void Dispose() { fallbackStore?.Dispose(); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            Task<T> fallbackGet = fallbackStore.Get(key, defaultValue, (fallbackValue) => InternalSet(key, fallbackValue));
            var fileForKey = GetFile(key);
            if (fileForKey.Exists) { return (T)InternalGet(fileForKey, typeof(T)); }
            return await fallbackGet;
        }

        private object InternalGet(FileEntry fileForKey, Type type) {
            if (type.IsPrimitive) { return fileForKey.LoadAs<PrimitiveWrapper>().val; }
            return fileForKey.LoadAs(type);
        }

        public FileEntry GetFile(string key) { return folderForAllFiles.GetChild(key); }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            var objType = value.GetType();
            if (objType.IsPrimitive) { value = new PrimitiveWrapper() { val = value }; }
            var file = GetFile(key);
            var oldVal = file.IsNotNullAndExists() ? InternalGet(file, objType) : null;
            if (objType == typeof(string)) {
                file.SaveAsText((string)value);
            } else {
                file.SaveAsText(JsonWriter.GetWriter().Write(value));
            }
            return oldVal;
        }

        public async Task<bool> Remove(string key) {
            var res = GetFile(key).DeleteV2();
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            folderForAllFiles.DeleteV2();
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public async Task<bool> ContainsKey(string key) {
            if (GetFile(key).IsNotNullAndExists()) { return true; }
            if (fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return false;
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            var result = folderForAllFiles.GetFiles().Map(x => x.Name);
            return await fallbackStore.ConcatAllKeys(result);
        }

    }

}