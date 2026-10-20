namespace TPToolkitLib.TPSettings
{
    internal class TPAppSettings
    {
        #region AppSettings.ini
        public string TPGameDirectory { get; set; } = string.Empty;
        public string EffectsDirectory { get; set; } = string.Empty;
        public string FlagTexturesDirectory { get; set; } = string.Empty;
        public string GuiTexturesDirectory { get; set; } = string.Empty;
        public string MeshesDirectory { get; set; } = string.Empty;
        public string MeshScenesDirectory { get; set; } = string.Empty;
        public string MeshTexturesDirectory { get; set; } = string.Empty;
        public string RenderEffectsDirectory { get; set; } = string.Empty;
        public string SoundDirectory { get; set; } = string.Empty;
        public string WorldFilesDirectory { get; set; } = string.Empty;
        public string WorldObjectFilesDirectory { get; set; } = string.Empty;
        #endregion
        #region Strings.ini
        public string GameHeaderFilesDirectory { get; set; } = string.Empty;
        public string GameStringsEnglish { get; set; } = string.Empty;
        #endregion
    }
}
