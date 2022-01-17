
namespace Dimensions.Bll.File
{
    public class FileContentBuilderFactory
    {
        private FileContentBuilderFactory()
        {

        }

        public static IFileContentBuilder CreateContentBuilder(string type)
        {
            return type switch
            {
                FileType.Mdd => new MddFileContentBuilder(),
                FileType.Dms => new DmsFileContentBuilder(),
                FileType.Tab => new TabFileContentBuilder(),
                FileType.Edt => new EdtFileContentBuilder(),
                FileType.Bat => new BatFileContentBuilder(),
                FileType.Top => new TopFileContentBuilder(),
                FileType.Log => new LogFileContentBuilder(),
                _ => null,
            };
        }
    }
}
