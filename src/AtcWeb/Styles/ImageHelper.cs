namespace AtcWeb.Styles
{
    public static class ImageHelper
    {
        public static string GetProgramIconPathForLanguage(string language)
            => language switch
            {
                "C#" => "images/programs/csharp.svg",
                "PowerShell" => "images/programs/powershell.svg",
                "Python" => "images/programs/python.svg",
                _ => "images/programs/text.svg"
            };
    }
}